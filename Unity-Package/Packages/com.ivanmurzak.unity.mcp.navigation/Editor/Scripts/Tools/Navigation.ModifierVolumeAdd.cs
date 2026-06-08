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
        public const string ModifierVolumeAddToolId = "navigation-modifier-volume-add";

        [AiTool
        (
            ModifierVolumeAddToolId,
            Title = "Navigation / Add NavMeshModifierVolume",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add and configure a `NavMeshModifierVolume` on a GameObject. A modifier volume marks a " +
            "box-shaped region of space so that any NavMesh baked inside it is assigned a specific area (e.g. a " +
            "non-walkable or higher-cost zone).")]
        [AiSkillBody("Add a `NavMeshModifierVolume` to a GameObject. Unlike a NavMeshModifier (which is tied to " +
            "geometry), a volume affects whatever NavMesh is generated inside its box region.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject to add the volume to (required).\n" +
            "- `size` — the box size of the volume (default 4,4,4).\n" +
            "- `center` — the box center offset (default 0,0,0).\n" +
            "- `area` — the NavMesh area index assigned inside the volume (default 0 = Walkable).\n\n" +
            "## Behavior\n\n" +
            "Adds (or reuses) a `NavMeshModifierVolume`, assigns size / center / area, marks the scene dirty, and " +
            "repaints. Runs on the Unity main thread.")]
        [Description("Adds and configures a NavMeshModifierVolume on a GameObject (box size/center and the area assigned inside it).")]
        public ModifierVolumeAddResponse AddModifierVolume
        (
            [Description("Reference to the GameObject to add the NavMeshModifierVolume to.")]
            GameObjectRef gameObjectRef,
            [Description("Box size of the volume.")]
            Vector3? size = null,
            [Description("Box center offset of the volume.")]
            Vector3? center = null,
            [Description("NavMesh area index assigned inside the volume (0 = Walkable).")]
            int area = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var volume = go.GetComponent<NavMeshModifierVolume>();
                if (volume == null)
                    volume = go.AddComponent<NavMeshModifierVolume>();

                volume.size = size ?? new Vector3(4f, 4f, 4f);
                volume.center = center ?? Vector3.zero;
                volume.area = area;

                MarkDirtyAndRepaint(volume, go.scene);

                return new ModifierVolumeAddResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    volumeRef = new ComponentRef(volume),
                    size = volume.size,
                    center = volume.center,
                    area = volume.area,
                    success = true
                };
            });
        }

        public class ModifierVolumeAddResponse
        {
            [Description("Reference to the GameObject hosting the volume.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshModifierVolume component.")]
            public ComponentRef? volumeRef;

            [Description("Resolved box size.")]
            public Vector3 size;

            [Description("Resolved box center.")]
            public Vector3 center;

            [Description("Resolved area index.")]
            public int area;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
