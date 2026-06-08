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
        public const string LinkAddToolId = "navigation-link-add";

        [AiTool
        (
            LinkAddToolId,
            Title = "Navigation / Add NavMeshLink",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add and configure a `NavMeshLink` on a GameObject. A NavMeshLink connects two points on " +
            "(or off) the NavMesh — e.g. a jump-across or a doorway — so agents can traverse gaps the baked mesh " +
            "does not cover. Points are local to the GameObject's transform.")]
        [AiSkillBody("Add a `NavMeshLink` to a GameObject. The link bridges `startPoint` and `endPoint` (in the " +
            "GameObject's local space), letting agents cross between otherwise-disconnected NavMesh regions.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject to add the link to (required).\n" +
            "- `startPoint` — local-space start of the link (default 0,0,-2.5).\n" +
            "- `endPoint` — local-space end of the link (default 0,0,2.5).\n" +
            "- `width` — link width in meters (default 0).\n" +
            "- `bidirectional` — when `true`, the link is traversable both ways (default true).\n" +
            "- `costModifier` — cost override; negative means use the default cost (default -1).\n" +
            "- `area` — NavMesh area index of the link (default 2 = Jump).\n\n" +
            "## Behavior\n\n" +
            "Adds (or reuses) a `NavMeshLink`, assigns the configured fields (which auto-rebuild the link), marks the " +
            "scene dirty, and repaints. Runs on the Unity main thread.")]
        [Description("Adds and configures a NavMeshLink on a GameObject (start/end points, width, bidirectional, cost, area).")]
        public LinkAddResponse AddLink
        (
            [Description("Reference to the GameObject to add the NavMeshLink to.")]
            GameObjectRef gameObjectRef,
            [Description("Local-space start point of the link.")]
            Vector3? startPoint = null,
            [Description("Local-space end point of the link.")]
            Vector3? endPoint = null,
            [Description("Link width in meters.")]
            float width = 0f,
            [Description("If true, the link is traversable in both directions.")]
            bool bidirectional = true,
            [Description("Cost modifier for traversing the link; negative uses the default cost.")]
            int costModifier = -1,
            [Description("NavMesh area index of the link (2 = Jump by default).")]
            int area = 2
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var link = go.GetComponent<NavMeshLink>();
                if (link == null)
                    link = go.AddComponent<NavMeshLink>();

                link.startPoint = startPoint ?? new Vector3(0f, 0f, -2.5f);
                link.endPoint = endPoint ?? new Vector3(0f, 0f, 2.5f);
                link.width = width;
                link.bidirectional = bidirectional;
                link.costModifier = costModifier;
                link.area = area;

                MarkDirtyAndRepaint(link, go.scene);

                return new LinkAddResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    linkRef = new ComponentRef(link),
                    startPoint = link.startPoint,
                    endPoint = link.endPoint,
                    width = link.width,
                    bidirectional = link.bidirectional,
                    area = link.area,
                    success = true
                };
            });
        }

        public class LinkAddResponse
        {
            [Description("Reference to the GameObject hosting the link.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the NavMeshLink component.")]
            public ComponentRef? linkRef;

            [Description("Resolved local start point.")]
            public Vector3 startPoint;

            [Description("Resolved local end point.")]
            public Vector3 endPoint;

            [Description("Resolved width.")]
            public float width;

            [Description("Resolved bidirectional flag.")]
            public bool bidirectional;

            [Description("Resolved area index.")]
            public int area;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
