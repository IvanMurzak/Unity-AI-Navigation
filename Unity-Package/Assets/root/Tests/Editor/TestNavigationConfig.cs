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
using System.Collections;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Navigation.Editor.Tests
{
    public class TestNavigationConfig : BaseTest
    {
        [UnityTest]
        public IEnumerator SetBakeSettings_VoxelSizeOnSurface()
        {
            var go = CreateGameObjectWithNavMeshSurface(GO_SurfaceName);

            var tool = new Tool_Navigation();
            var result = tool.SetBakeSettings(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                voxelSize: 0.25f);

            Assert.IsTrue(result.success, "SetBakeSettings should succeed");
            Assert.IsTrue(result.voxelSizeApplied, "voxel size should have been applied");

            var surface = go.GetComponent<NavMeshSurface>();
            Assert.IsTrue(surface!.overrideVoxelSize, "overrideVoxelSize should be enabled");
            Assert.AreEqual(0.25f, surface.voxelSize, 0.0001f, "voxelSize should be set on the surface");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetBakeSettings_AgentRadiusOnHumanoidType()
        {
            var tool = new Tool_Navigation();
            // Edit the default Humanoid agent type (id 0). Read the original first to restore it.
            var original = NavMesh.GetSettingsByID(0);
            var newRadius = original.agentRadius + 0.13f;

            var result = tool.SetBakeSettings(agentTypeId: 0, agentRadius: newRadius);

            Assert.IsTrue(result.success, "SetBakeSettings should succeed");
            Assert.IsTrue(result.agentSettingsApplied, "agent settings should have been applied");
            Assert.AreEqual(newRadius, NavMesh.GetSettingsByID(0).agentRadius, 0.001f,
                "Humanoid agent radius should be updated in project settings");

            // Restore the original radius so the project setting is not left mutated.
            tool.SetBakeSettings(agentTypeId: 0, agentRadius: original.agentRadius);

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetAgentDestination_AssignsDestination()
        {
            var go = CreateGameObjectWithNavMeshAgent(GO_AgentName);

            var tool = new Tool_Navigation();
            var dest = new Vector3(7, 0, -3);
            var result = tool.SetAgentDestination(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                destination: dest);

            Assert.IsTrue(result.success, "SetAgentDestination should succeed");
            Assert.AreEqual(dest, result.destination, "Destination should be reported back");

            yield return null;
        }

        [UnityTest]
        public IEnumerator List_FindsSurfacesAndAgents()
        {
            CreateGameObjectWithNavMeshSurface("ListSurfaceA");
            CreateGameObjectWithNavMeshSurface("ListSurfaceB");
            CreateGameObjectWithNavMeshAgent("ListAgentA");

            var tool = new Tool_Navigation();
            var result = tool.ListNavigation(includeInactive: true);

            Assert.GreaterOrEqual(result.surfaceCount, 2, "Should find at least the two created surfaces");
            Assert.GreaterOrEqual(result.agentCount, 1, "Should find at least the created agent");

            yield return null;
        }
    }
}
