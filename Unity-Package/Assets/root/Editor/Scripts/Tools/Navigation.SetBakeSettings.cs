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
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string SetBakeSettingsToolId = "navigation-set-bake-settings";

        [AiTool
        (
            SetBakeSettingsToolId,
            Title = "Navigation / Set Bake Settings",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Tune NavMesh bake settings. Agent radius / height / max-slope / step-height belong to a " +
            "NavMesh agent type (in project NavMesh settings) and apply to every surface baking for that type. The " +
            "voxel size is a per-surface override; pass `gameObjectRef` (a NavMeshSurface) with `voxelSize` to set it.")]
        [AiSkillBody("Configure the parameters that control how a NavMesh is voxelized and baked.\n\n" +
            "Agent radius / height / max-slope / step-height are stored on the **agent type** (id `agentTypeId`, " +
            "default 0 = Humanoid) in the project's NavMesh settings, so they affect all surfaces baking for that " +
            "type. The **voxel size** is a per-`NavMeshSurface` override.\n\n" +
            "## Inputs\n\n" +
            "- `agentTypeId` — the NavMesh agent type to edit (default 0 = Humanoid).\n" +
            "- `agentRadius` — optional agent radius (meters).\n" +
            "- `agentHeight` — optional agent height (meters).\n" +
            "- `maxSlope` — optional maximum walkable slope (degrees, 0–60).\n" +
            "- `stepHeight` — optional maximum step / climb height (meters).\n" +
            "- `gameObjectRef` — optional NavMeshSurface GameObject whose voxel-size override to set.\n" +
            "- `voxelSize` — optional voxel size for that surface (only applied when `gameObjectRef` is provided; " +
            "sets `overrideVoxelSize = true`).\n\n" +
            "## Behavior\n\n" +
            "Edits the agent type's bake settings via the `NavMeshProjectSettings` serialized object (only the " +
            "provided values change), and optionally enables and sets the surface voxel-size override. Marks dirty " +
            "and repaints. Runs on the Unity main thread.")]
        [Description("Sets NavMesh bake settings: agent radius/height/max-slope/step-height (per agent type) and an " +
            "optional per-surface voxel size override.")]
        public SetBakeSettingsResponse SetBakeSettings
        (
            [Description("NavMesh agent-type id to edit (0 = Humanoid by default).")]
            int agentTypeId = 0,
            [Description("Agent radius in meters.")]
            float? agentRadius = null,
            [Description("Agent height in meters.")]
            float? agentHeight = null,
            [Description("Maximum walkable slope in degrees (0-60).")]
            float? maxSlope = null,
            [Description("Maximum step / climb height in meters.")]
            float? stepHeight = null,
            [Description("Optional NavMeshSurface GameObject whose voxel-size override to set.")]
            GameObjectRef? gameObjectRef = null,
            [Description("Voxel size for the surface (applied only when gameObjectRef is provided).")]
            float? voxelSize = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var response = new SetBakeSettingsResponse { agentTypeId = agentTypeId };

                if (agentRadius.HasValue || agentHeight.HasValue || maxSlope.HasValue || stepHeight.HasValue)
                {
                    var settingsSingleton = UnityEditor.Unsupported.GetSerializedAssetInterfaceSingleton("NavMeshProjectSettings");
                    if (settingsSingleton == null)
                        throw new Exception(Error.NavMeshProjectSettingsUnavailable());

                    var so = new SerializedObject(settingsSingleton);
                    var settingsArray = so.FindProperty("m_Settings");
                    if (settingsArray == null)
                        throw new Exception(Error.NavMeshProjectSettingsUnavailable());

                    SerializedProperty? agentType = null;
                    for (int i = 0; i < settingsArray.arraySize; i++)
                    {
                        var element = settingsArray.GetArrayElementAtIndex(i);
                        var idProp = element.FindPropertyRelative("agentTypeID");
                        if (idProp != null && idProp.intValue == agentTypeId)
                        {
                            agentType = element;
                            break;
                        }
                    }

                    if (agentType == null)
                        throw new Exception(Error.AgentTypeNotFound(agentTypeId));

                    if (agentRadius.HasValue)
                        agentType.FindPropertyRelative("agentRadius").floatValue = agentRadius.Value;
                    if (agentHeight.HasValue)
                        agentType.FindPropertyRelative("agentHeight").floatValue = agentHeight.Value;
                    if (stepHeight.HasValue)
                        agentType.FindPropertyRelative("agentClimb").floatValue = stepHeight.Value;
                    if (maxSlope.HasValue)
                        agentType.FindPropertyRelative("agentSlope").floatValue = maxSlope.Value;

                    so.ApplyModifiedProperties();

                    var resolved = NavMesh.GetSettingsByID(agentTypeId);
                    response.agentRadius = resolved.agentRadius;
                    response.agentHeight = resolved.agentHeight;
                    response.maxSlope = resolved.agentSlope;
                    response.stepHeight = resolved.agentClimb;
                    response.agentSettingsApplied = true;
                }

                if (gameObjectRef != null && gameObjectRef.IsValid(out _) && voxelSize.HasValue)
                {
                    var surface = ResolveNavMeshSurface(gameObjectRef, nameof(gameObjectRef));
                    surface.overrideVoxelSize = true;
                    surface.voxelSize = voxelSize.Value;
                    MarkDirtyAndRepaint(surface, surface.gameObject.scene);
                    response.surfaceRef = new ComponentRef(surface);
                    response.voxelSize = surface.voxelSize;
                    response.voxelSizeApplied = true;
                }

                response.success = true;
                return response;
            });
        }

        public class SetBakeSettingsResponse
        {
            [Description("The edited agent-type id.")]
            public int agentTypeId;

            [Description("Whether agent-type bake settings were applied.")]
            public bool agentSettingsApplied;

            [Description("Resolved agent radius after the edit.")]
            public float agentRadius;

            [Description("Resolved agent height after the edit.")]
            public float agentHeight;

            [Description("Resolved maximum slope after the edit.")]
            public float maxSlope;

            [Description("Resolved step / climb height after the edit.")]
            public float stepHeight;

            [Description("Reference to the surface whose voxel size was set, if any.")]
            public ComponentRef? surfaceRef;

            [Description("Whether the surface voxel size override was applied.")]
            public bool voxelSizeApplied;

            [Description("Resolved voxel size for the surface, if set.")]
            public float voxelSize;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
