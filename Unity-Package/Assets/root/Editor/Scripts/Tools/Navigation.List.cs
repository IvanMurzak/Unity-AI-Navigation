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
using System.Collections.Generic;
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
        public const string ListToolId = "navigation-list";

        [AiTool
        (
            ListToolId,
            Title = "Navigation / List Surfaces and Agents",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("List every `NavMeshSurface` and `NavMeshAgent` in the active scene with their key " +
            "settings (agent type, baked-data presence for surfaces; speed, on-NavMesh state for agents). Read-only.")]
        [AiSkillBody("Enumerate the NavMesh components in the active scene so you can inspect what is set up.\n\n" +
            "## Inputs\n\n" +
            "- `includeInactive` (bool, default true) — include components on inactive/disabled GameObjects.\n\n" +
            "## Behavior\n\n" +
            "Finds all `NavMeshSurface` and `NavMeshAgent` instances, reports each surface's agent-type id and " +
            "whether it has baked NavMeshData, and each agent's agent-type id, speed, and whether it is currently on " +
            "a NavMesh. Read-only. Runs on the Unity main thread.")]
        [Description("Lists all NavMeshSurfaces and NavMeshAgents in the active scene with key settings. Read-only.")]
        public NavigationListResponse ListNavigation
        (
            [Description("If true (default), include components on inactive/disabled GameObjects.")]
            bool includeInactive = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
#if UNITY_2023_1_OR_NEWER
                var surfaces = UnityEngine.Object.FindObjectsByType<NavMeshSurface>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                var agents = UnityEngine.Object.FindObjectsByType<NavMeshAgent>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                var surfaces = UnityEngine.Object.FindObjectsOfType<NavMeshSurface>(includeInactive);
                var agents = UnityEngine.Object.FindObjectsOfType<NavMeshAgent>(includeInactive);
#endif

                var surfaceItems = new List<SurfaceListItem>(surfaces.Length);
                foreach (var surface in surfaces)
                {
                    if (surface == null)
                        continue;
                    surfaceItems.Add(new SurfaceListItem
                    {
                        gameObjectRef = new GameObjectRef(surface.gameObject),
                        surfaceRef = new ComponentRef(surface),
                        name = surface.name,
                        agentTypeId = surface.agentTypeID,
                        hasNavMeshData = surface.navMeshData != null
                    });
                }

                var agentItems = new List<AgentListItem>(agents.Length);
                foreach (var agent in agents)
                {
                    if (agent == null)
                        continue;
                    agentItems.Add(new AgentListItem
                    {
                        gameObjectRef = new GameObjectRef(agent.gameObject),
                        agentRef = new ComponentRef(agent),
                        name = agent.name,
                        agentTypeId = agent.agentTypeID,
                        speed = agent.speed,
                        isOnNavMesh = agent.isOnNavMesh
                    });
                }

                return new NavigationListResponse
                {
                    surfaceCount = surfaceItems.Count,
                    agentCount = agentItems.Count,
                    surfaces = surfaceItems.ToArray(),
                    agents = agentItems.ToArray()
                };
            });
        }

        public class NavigationListResponse
        {
            [Description("Number of NavMeshSurfaces found.")]
            public int surfaceCount;

            [Description("Number of NavMeshAgents found.")]
            public int agentCount;

            [Description("The NavMeshSurfaces in the active scene.")]
            public SurfaceListItem[] surfaces = Array.Empty<SurfaceListItem>();

            [Description("The NavMeshAgents in the active scene.")]
            public AgentListItem[] agents = Array.Empty<AgentListItem>();
        }

        public class SurfaceListItem
        {
            [Description("Reference to the surface GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshSurface component.")]
            public ComponentRef? surfaceRef;

            [Description("Name of the GameObject.")]
            public string name = string.Empty;

            [Description("Agent-type id this surface bakes for.")]
            public int agentTypeId;

            [Description("Whether the surface has baked NavMeshData.")]
            public bool hasNavMeshData;
        }

        public class AgentListItem
        {
            [Description("Reference to the agent GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshAgent component.")]
            public ComponentRef? agentRef;

            [Description("Name of the GameObject.")]
            public string name = string.Empty;

            [Description("Agent-type id of the agent.")]
            public int agentTypeId;

            [Description("Movement speed of the agent.")]
            public float speed;

            [Description("Whether the agent is currently on a baked NavMesh.")]
            public bool isOnNavMesh;
        }
    }
}
