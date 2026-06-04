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
using com.IvanMurzak.McpPlugin;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [AiToolType]
    public partial class Tool_Navigation
    {
        public static class Error
        {
            public static string GameObjectNotFound()
                => "[Error] GameObject not found. Provide a valid reference to an existing GameObject.";

            public static string NavMeshSurfaceNotFound()
                => "[Error] NavMeshSurface component not found on the target GameObject. " +
                   "Make sure the GameObject has a NavMeshSurface component attached (use 'navigation-surface-add').";

            public static string NavMeshAgentNotFound()
                => "[Error] NavMeshAgent component not found on the target GameObject. " +
                   "Make sure the GameObject has a NavMeshAgent component attached (use 'navigation-agent-add').";

            public static string ComponentNotFound(string componentTypeName)
                => $"[Error] Component '{componentTypeName}' not found on the target GameObject.";

            public static string NoNavMeshComponent()
                => "[Error] No NavMesh component (NavMeshSurface / NavMeshModifier / NavMeshModifierVolume / " +
                   "NavMeshLink / NavMeshAgent) found on the specified GameObject.";

            public static string AgentTypeNotFound(int agentTypeId)
                => $"[Error] NavMesh agent type with id '{agentTypeId}' was not found in the project's NavMesh settings.";

            public static string NavMeshProjectSettingsUnavailable()
                => "[Error] Could not access the NavMeshProjectSettings singleton to edit agent-type bake settings.";

            public static string ReflectorNotAvailable()
                => "[Error] ReflectorNet reflector is not available.";
        }
    }
}
