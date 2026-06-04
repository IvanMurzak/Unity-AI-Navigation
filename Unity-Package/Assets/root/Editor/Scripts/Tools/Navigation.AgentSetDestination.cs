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
using UnityEngine.AI;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        public const string AgentSetDestinationToolId = "navigation-agent-set-destination";

        [AiTool
        (
            AgentSetDestinationToolId,
            Title = "Navigation / Set Agent Destination",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the destination of a `NavMeshAgent` so it pathfinds toward a world-space point. " +
            "Optionally provide a target GameObject instead of explicit coordinates. The agent must be on a baked " +
            "NavMesh for a path to be computed (most relevant in play mode).")]
        [AiSkillBody("Set a `NavMeshAgent`'s destination. The agent computes a path across the baked NavMesh and " +
            "begins moving toward the destination (path computation/movement happens in play mode; in edit mode the " +
            "destination is still assigned).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `NavMeshAgent` (required).\n" +
            "- `destination` — the world-space destination point. Ignored when `targetRef` is provided.\n" +
            "- `targetRef` — optional GameObject whose position is used as the destination.\n\n" +
            "## Behavior\n\n" +
            "Resolves the agent, computes the destination (from `targetRef` if given, else `destination`), and — when " +
            "the agent is active and placed on a NavMesh — calls `SetDestination`. Off the NavMesh (e.g. before " +
            "baking, in edit mode) it reports the intended destination without mutating, so the call is always safe. " +
            "Returns `accepted` plus the agent's `hasPath` / `pathPending` state. Runs on the Unity main thread.")]
        [Description("Sets a NavMeshAgent's destination from a world point or a target GameObject and starts pathfinding.")]
        public AgentSetDestinationResponse SetAgentDestination
        (
            [Description("Reference to the GameObject containing the NavMeshAgent component.")]
            GameObjectRef gameObjectRef,
            [Description("World-space destination point (ignored when targetRef is provided).")]
            Vector3? destination = null,
            [Description("Optional GameObject whose position is used as the destination.")]
            GameObjectRef? targetRef = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var agent = ResolveNavMeshAgent(gameObjectRef, nameof(gameObjectRef));

                var targetTransform = ResolveOptionalTransform(targetRef, nameof(targetRef));
                var dest = targetTransform != null
                    ? targetTransform.position
                    : (destination ?? agent.transform.position);

                // SetDestination / the destination setter only work on an active agent placed on a
                // NavMesh — calling them otherwise logs an error. Off the NavMesh we report the intended
                // destination without mutating, so the call is safe in edit mode / before baking.
                bool accepted = false;
                if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                    accepted = agent.SetDestination(dest);

                MarkDirtyAndRepaint(agent, agent.gameObject.scene);

                return new AgentSetDestinationResponse
                {
                    gameObjectRef = new GameObjectRef(agent.gameObject),
                    agentRef = new ComponentRef(agent),
                    destination = dest,
                    accepted = accepted,
                    isOnNavMesh = agent.isOnNavMesh,
                    hasPath = agent.hasPath,
                    pathPending = agent.pathPending,
                    success = true
                };
            });
        }

        public class AgentSetDestinationResponse
        {
            [Description("Reference to the NavMeshAgent GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshAgent component.")]
            public ComponentRef? agentRef;

            [Description("The resolved destination point.")]
            public Vector3 destination;

            [Description("Whether SetDestination accepted the request (false if the agent is off the NavMesh).")]
            public bool accepted;

            [Description("Whether the agent is currently on a baked NavMesh.")]
            public bool isOnNavMesh;

            [Description("Whether the agent has a computed path.")]
            public bool hasPath;

            [Description("Whether a path is still being computed.")]
            public bool pathPending;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
