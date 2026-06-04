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
#if UNITY_6000_5_OR_NEWER
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
        public const string AgentAddToolId = "navigation-agent-add";

        [AiTool
        (
            AgentAddToolId,
            Title = "Navigation / Add NavMeshAgent",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add and configure a `NavMeshAgent` (from the built-in `com.unity.modules.ai`) on a " +
            "GameObject — or create a new GameObject to host it. Configure agent type, speed, acceleration, angular " +
            "speed, stopping distance, and the area mask. Returns the GameObject reference and instanceId.")]
        [AiSkillBody("Add a `NavMeshAgent` to a GameObject so it can pathfind across a baked NavMesh. Use " +
            "'navigation-agent-set-destination' to move it.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — optional GameObject to add the agent to. When omitted, a new GameObject is created.\n" +
            "- `name` — optional name for the new GameObject (default `NavMeshAgent`); ignored when `gameObjectRef` is given.\n" +
            "- `agentTypeId` — optional NavMesh agent-type id (default 0 = Humanoid).\n" +
            "- `speed` — optional max movement speed (default 3.5).\n" +
            "- `acceleration` — optional max acceleration (default 8).\n" +
            "- `angularSpeed` — optional max turning speed in deg/s (default 120).\n" +
            "- `stoppingDistance` — optional stop distance from the destination (default 0).\n" +
            "- `areaMask` — optional traversable-area bitmask (default -1 = all areas).\n\n" +
            "## Behavior\n\n" +
            "Adds (or reuses) a `NavMeshAgent`, assigns the configured properties, marks the scene dirty, repaints, " +
            "and returns the GameObject reference, the agent component reference, and the instanceId. Runs on the " +
            "Unity main thread.")]
        [Description("Adds and configures a NavMeshAgent on a GameObject (or a newly created one). Sets agent type, " +
            "speed, acceleration, angular speed, stopping distance, and area mask.")]
        public AgentAddResponse AddAgent
        (
            [Description("Optional GameObject to add the NavMeshAgent to. When omitted, a new GameObject is created.")]
            GameObjectRef? gameObjectRef = null,
            [Description("Name for the new GameObject (ignored when gameObjectRef is provided).")]
            string? name = null,
            [Description("NavMesh agent-type id (0 = Humanoid by default).")]
            int agentTypeId = 0,
            [Description("Maximum movement speed.")]
            float speed = 3.5f,
            [Description("Maximum acceleration.")]
            float acceleration = 8f,
            [Description("Maximum turning speed in degrees per second.")]
            float angularSpeed = 120f,
            [Description("Distance from the destination at which the agent stops.")]
            float stoppingDistance = 0f,
            [Description("Traversable-area bitmask (-1 = all areas).")]
            int areaMask = -1
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go;
                if (gameObjectRef != null && gameObjectRef.IsValid(out _))
                    go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                else
                    go = new GameObject(string.IsNullOrEmpty(name) ? "NavMeshAgent" : name);

                var agent = go.GetComponent<NavMeshAgent>();
                if (agent == null)
                    agent = go.AddComponent<NavMeshAgent>();

                agent.agentTypeID = agentTypeId;
                agent.speed = speed;
                agent.acceleration = acceleration;
                agent.angularSpeed = angularSpeed;
                agent.stoppingDistance = stoppingDistance;
                agent.areaMask = areaMask;

                MarkDirtyAndRepaint(agent, go.scene);

                return new AgentAddResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    agentRef = new ComponentRef(agent),
                    instanceId = go.GetEntityId(),
                    gameObjectName = go.name,
                    agentTypeId = agent.agentTypeID,
                    speed = agent.speed,
                    acceleration = agent.acceleration,
                    stoppingDistance = agent.stoppingDistance,
                    areaMask = agent.areaMask,
                    success = true
                };
            });
        }

        public class AgentAddResponse
        {
            [Description("Reference to the NavMeshAgent GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshAgent component.")]
            public ComponentRef? agentRef;

            [Description("Instance id of the GameObject hosting the agent.")]
            public UnityEngine.EntityId instanceId;

            [Description("Name of the GameObject.")]
            public string gameObjectName = string.Empty;

            [Description("Resolved agent-type id.")]
            public int agentTypeId;

            [Description("Resolved speed.")]
            public float speed;

            [Description("Resolved acceleration.")]
            public float acceleration;

            [Description("Resolved stopping distance.")]
            public float stoppingDistance;

            [Description("Resolved area mask.")]
            public int areaMask;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
#endif
