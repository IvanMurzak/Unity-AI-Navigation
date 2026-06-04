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
#if !UNITY_6000_5_OR_NEWER
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string SurfaceAddToolId = "navigation-surface-add";

        [AiTool
        (
            SurfaceAddToolId,
            Title = "Navigation / Add NavMeshSurface",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add and configure a `NavMeshSurface` component (from `com.unity.ai.navigation`) on a " +
            "GameObject — or create a new GameObject to host it. Configure agent type, geometry-collection mode, " +
            "layer mask, default area, and the box volume (size/center). Returns the GameObject reference and instanceId.")]
        [AiSkillBody("Add a `NavMeshSurface` to a GameObject. A NavMeshSurface defines a region of the scene that is " +
            "baked into a runtime NavMesh that agents can traverse. Use 'navigation-set-bake-settings' to tune the " +
            "agent radius/height/slope/step and voxel size, then 'navigation-surface-bake' to bake.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — optional GameObject to add the surface to. When omitted, a new GameObject is created.\n" +
            "- `name` — optional name for the new GameObject (default `NavMesh Surface`); ignored when `gameObjectRef` is given.\n" +
            "- `agentTypeId` — optional NavMesh agent-type id this surface bakes for (default 0 = Humanoid).\n" +
            "- `collectObjects` — optional geometry collection mode: `0` All, `1` Volume, `2` Children (default 0 All).\n" +
            "- `defaultArea` — optional default NavMesh area index applied to collected geometry (default 0 = Walkable).\n" +
            "- `layerMask` — optional include-layers bitmask for geometry collection (default -1 = Everything).\n" +
            "- `size` — optional box volume size (only used when `collectObjects` = Volume).\n" +
            "- `center` — optional box volume center offset.\n\n" +
            "## Behavior\n\n" +
            "Adds (or reuses) a `NavMeshSurface`, assigns the configured properties, marks the scene dirty, repaints, " +
            "and returns the GameObject reference, the surface component reference, and the instanceId. Runs on the " +
            "Unity main thread.")]
        [Description("Adds and configures a NavMeshSurface on a GameObject (or a newly created one). Sets agent type, " +
            "collection mode, layer mask, default area, and the box volume.")]
        public SurfaceAddResponse AddSurface
        (
            [Description("Optional GameObject to add the NavMeshSurface to. When omitted, a new GameObject is created.")]
            GameObjectRef? gameObjectRef = null,
            [Description("Name for the new GameObject (ignored when gameObjectRef is provided).")]
            string? name = null,
            [Description("NavMesh agent-type id this surface bakes for (0 = Humanoid by default).")]
            int agentTypeId = 0,
            [Description("Geometry collection mode: 0 = All, 1 = Volume, 2 = Children.")]
            int collectObjects = 0,
            [Description("Default NavMesh area index applied to collected geometry (0 = Walkable).")]
            int defaultArea = 0,
            [Description("Include-layers bitmask for geometry collection (-1 = Everything).")]
            int layerMask = -1,
            [Description("Box volume size (used only when collectObjects = Volume).")]
            Vector3? size = null,
            [Description("Box volume center offset.")]
            Vector3? center = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go;
                if (gameObjectRef != null && gameObjectRef.IsValid(out _))
                    go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                else
                    go = new GameObject(string.IsNullOrEmpty(name) ? "NavMesh Surface" : name);

                var surface = go.GetComponent<NavMeshSurface>();
                if (surface == null)
                    surface = go.AddComponent<NavMeshSurface>();

                surface.agentTypeID = agentTypeId;
                surface.collectObjects = (CollectObjects)collectObjects;
                surface.defaultArea = defaultArea;
                surface.layerMask = layerMask;
                if (size.HasValue)
                    surface.size = size.Value;
                if (center.HasValue)
                    surface.center = center.Value;

                MarkDirtyAndRepaint(surface, go.scene);

                return new SurfaceAddResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    surfaceRef = new ComponentRef(surface),
                    instanceId = go.GetInstanceID(),
                    gameObjectName = go.name,
                    agentTypeId = surface.agentTypeID,
                    collectObjects = (int)surface.collectObjects,
                    defaultArea = surface.defaultArea,
                    success = true
                };
            });
        }

        public class SurfaceAddResponse
        {
            [Description("Reference to the NavMeshSurface GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshSurface component.")]
            public ComponentRef? surfaceRef;

            [Description("Instance id of the GameObject hosting the surface.")]
            public int instanceId;

            [Description("Name of the GameObject.")]
            public string gameObjectName = string.Empty;

            [Description("Resolved agent-type id.")]
            public int agentTypeId;

            [Description("Resolved geometry collection mode.")]
            public int collectObjects;

            [Description("Resolved default area index.")]
            public int defaultArea;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
#endif
