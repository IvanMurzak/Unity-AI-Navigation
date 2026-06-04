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
using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string SurfaceBakeToolId = "navigation-surface-bake";

        [AiTool
        (
            SurfaceBakeToolId,
            Title = "Navigation / Bake or Clear NavMeshSurface",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Bake (build) the NavMesh for a `NavMeshSurface`, or clear its existing baked data. " +
            "Pass `clear = true` to remove the baked NavMeshData instead of building it.")]
        [AiSkillBody("Build or clear the runtime NavMesh for a `NavMeshSurface`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `NavMeshSurface` (required).\n" +
            "- `clear` — when `true`, removes the surface's baked NavMeshData (`RemoveData`) instead of baking. " +
            "Default `false` (bake via `BuildNavMesh`).\n\n" +
            "## Behavior\n\n" +
            "When baking, collects the surface's geometry sources and builds the NavMeshData, then marks the surface " +
            "dirty so the result is saved. When clearing, removes the active NavMeshData from the surface. Marks the " +
            "scene dirty and repaints. Runs on the Unity main thread.")]
        [Description("Bakes (BuildNavMesh) or clears (RemoveData) the NavMesh of a NavMeshSurface. Pass clear=true to clear.")]
        public SurfaceBakeResponse BakeSurface
        (
            [Description("Reference to the GameObject containing the NavMeshSurface component.")]
            GameObjectRef gameObjectRef,
            [Description("If true, clears the baked NavMeshData instead of baking it.")]
            bool clear = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var surface = ResolveNavMeshSurface(gameObjectRef, nameof(gameObjectRef));

                if (clear)
                {
                    surface.RemoveData();
                    surface.navMeshData = null;
                }
                else
                {
                    surface.BuildNavMesh();
                    if (surface.navMeshData != null)
                        EditorUtility.SetDirty(surface.navMeshData);
                }

                MarkDirtyAndRepaint(surface, surface.gameObject.scene);

                return new SurfaceBakeResponse
                {
                    gameObjectRef = new GameObjectRef(surface.gameObject),
                    surfaceRef = new ComponentRef(surface),
                    cleared = clear,
                    hasNavMeshData = surface.navMeshData != null,
                    success = true
                };
            });
        }

        public class SurfaceBakeResponse
        {
            [Description("Reference to the NavMeshSurface GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshSurface component.")]
            public ComponentRef? surfaceRef;

            [Description("Whether the surface was cleared (true) or baked (false).")]
            public bool cleared;

            [Description("Whether the surface has baked NavMeshData after the operation.")]
            public bool hasNavMeshData;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
