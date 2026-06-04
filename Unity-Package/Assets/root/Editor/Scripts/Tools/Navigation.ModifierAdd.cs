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
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string ModifierAddToolId = "navigation-modifier-add";

        [AiTool
        (
            ModifierAddToolId,
            Title = "Navigation / Add NavMeshModifier",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add and configure a `NavMeshModifier` on a GameObject. A NavMeshModifier overrides how " +
            "that object (and optionally its children) is treated during baking — its NavMesh area, whether it is " +
            "ignored, and link generation.")]
        [AiSkillBody("Add a `NavMeshModifier` to a GameObject so its geometry is baked with overridden parameters.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject to add the modifier to (required).\n" +
            "- `overrideArea` — when `true`, the object's NavMesh area is overridden with `area`.\n" +
            "- `area` — the NavMesh area index to assign when `overrideArea` is true (default 0 = Walkable).\n" +
            "- `ignoreFromBuild` — when `true`, the object is excluded from the bake entirely.\n" +
            "- `applyToChildren` — when `true`, the override also applies to child objects.\n\n" +
            "## Behavior\n\n" +
            "Adds (or reuses) a `NavMeshModifier`, assigns the configured flags, marks the scene dirty, and repaints. " +
            "Runs on the Unity main thread.")]
        [Description("Adds and configures a NavMeshModifier on a GameObject (override area, ignore-from-build, apply-to-children).")]
        public ModifierAddResponse AddModifier
        (
            [Description("Reference to the GameObject to add the NavMeshModifier to.")]
            GameObjectRef gameObjectRef,
            [Description("If true, override the NavMesh area of this object with 'area'.")]
            bool overrideArea = false,
            [Description("NavMesh area index to assign when overrideArea is true (0 = Walkable).")]
            int area = 0,
            [Description("If true, exclude this object from the NavMesh bake.")]
            bool ignoreFromBuild = false,
            [Description("If true, apply the override to child objects as well.")]
            bool applyToChildren = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var modifier = go.GetComponent<NavMeshModifier>();
                if (modifier == null)
                    modifier = go.AddComponent<NavMeshModifier>();

                modifier.overrideArea = overrideArea;
                modifier.area = area;
                modifier.ignoreFromBuild = ignoreFromBuild;
                modifier.applyToChildren = applyToChildren;

                MarkDirtyAndRepaint(modifier, go.scene);

                return new ModifierAddResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    modifierRef = new ComponentRef(modifier),
                    overrideArea = modifier.overrideArea,
                    area = modifier.area,
                    ignoreFromBuild = modifier.ignoreFromBuild,
                    applyToChildren = modifier.applyToChildren,
                    success = true
                };
            });
        }

        public class ModifierAddResponse
        {
            [Description("Reference to the GameObject hosting the modifier.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshModifier component.")]
            public ComponentRef? modifierRef;

            [Description("Resolved overrideArea flag.")]
            public bool overrideArea;

            [Description("Resolved area index.")]
            public int area;

            [Description("Resolved ignoreFromBuild flag.")]
            public bool ignoreFromBuild;

            [Description("Resolved applyToChildren flag.")]
            public bool applyToChildren;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
