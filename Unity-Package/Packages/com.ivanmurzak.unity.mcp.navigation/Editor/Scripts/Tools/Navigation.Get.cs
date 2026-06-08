/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Navigation)     │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using Microsoft.Extensions.Logging;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string GetToolId = "navigation-get";

        [AiTool
        (
            GetToolId,
            Title = "Navigation / Get Component",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Generic read: serialize any NavMesh `Component` (NavMeshSurface / NavMeshModifier / " +
            "NavMeshModifierVolume / NavMeshLink / NavMeshAgent) on a GameObject via ReflectorNet. Pair with " +
            "'navigation-modify' to write changes back. Read-only.")]
        [AiSkillBody("Serialize any NavMesh component on a GameObject using ReflectorNet. This is the generic escape " +
            "hatch for fields not covered by the dedicated tools.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the component (required).\n" +
            "- `componentRef` — optional. Resolves a specific component when the GameObject has more than one NavMesh " +
            "component; otherwise the first recognized NavMesh component is used.\n" +
            "- `deepSerialization` — when `true`, recurses through nested objects; otherwise only top-level members.\n\n" +
            "## Behavior\n\n" +
            "Finds the target NavMesh component, serializes it via ReflectorNet, and returns the serialized member " +
            "plus the resolved component type name. Read-only. Runs on the Unity main thread.")]
        [Description("Generic: serialize any NavMesh Component on a GameObject via ReflectorNet. Read-only. " +
            "Use navigation-modify to write changes back.")]
        public NavigationGetResponse GetComponentData
        (
            [Description("Reference to the GameObject containing the NavMesh component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional reference to a specific NavMesh component if the GameObject has multiple.")]
            ComponentRef? componentRef = null,
            [Description("Performs deep serialization including nested objects. Otherwise only top-level members.")]
            bool deepSerialization = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var (component, index) = FindNavigationComponent(go, componentRef);
                if (component == null)
                    throw new Exception(Error.NoNavMeshComponent());

                var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception(Error.ReflectorNotAvailable());
                var logger = UnityLoggerFactory.LoggerFactory.CreateLogger<Tool_Navigation>();

                return new NavigationGetResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    componentRef = new ComponentRef(component),
                    componentIndex = index,
                    componentType = component.GetType().FullName ?? component.GetType().Name,
                    data = reflector.Serialize(
                        obj: component,
                        name: component.GetType().Name,
                        recursive: deepSerialization,
                        logger: logger)
                };
            });
        }

        /// <summary>
        /// Locate a NavMesh component on the GameObject. When componentRef resolves, returns the matching
        /// component; otherwise returns the first component recognized as a NavMesh component.
        /// </summary>
        static (UnityEngine.Component? component, int index) FindNavigationComponent(GameObject go, ComponentRef? componentRef)
        {
            var all = go.GetComponents<UnityEngine.Component>();
            for (int i = 0; i < all.Length; i++)
            {
                var comp = all[i];
                if (comp == null)
                    continue;

                if (componentRef != null && componentRef.IsValid(out _))
                {
                    if (componentRef.Matches(comp, i))
                        return (comp, i);
                }
                else if (IsNavigationComponent(comp))
                {
                    return (comp, i);
                }
            }
            return (null, -1);
        }

        static bool IsNavigationComponent(UnityEngine.Component comp)
        {
            var type = comp.GetType();
            if (type == typeof(UnityEngine.AI.NavMeshAgent) ||
                type == typeof(UnityEngine.AI.NavMeshObstacle))
                return true;

            var ns = type.Namespace;
            return ns != null && ns.StartsWith("Unity.AI.Navigation", StringComparison.Ordinal);
        }

        public class NavigationGetResponse
        {
            [Description("Reference to the GameObject containing the component.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the serialized component.")]
            public ComponentRef? componentRef;

            [Description("Index of the component in the GameObject's component list.")]
            public int componentIndex = -1;

            [Description("Full type name of the serialized component.")]
            public string componentType = string.Empty;

            [Description("Serialized component data.")]
            public SerializedMember? data;
        }
    }
}
