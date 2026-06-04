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
    public class TestNavigationLifecycle : BaseTest
    {
        [UnityTest]
        public IEnumerator AddSurface_CreatesNewGameObjectWithSurface()
        {
            var tool = new Tool_Navigation();
            var result = tool.AddSurface(name: GO_SurfaceName, collectObjects: 0, defaultArea: 0);

            Assert.IsTrue(result.success, "AddSurface should succeed");
            Assert.IsNotNull(result.gameObjectRef, "GameObject ref should be returned");

            var go = GameObject.Find(GO_SurfaceName);
            Assert.IsNotNull(go, "The new GameObject should exist in the scene");
            Assert.IsNotNull(go!.GetComponent<NavMeshSurface>(), "It should host a NavMeshSurface");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddSurface_OnExistingGameObject()
        {
            var go = new GameObject("ExistingSurfaceHost");

            var tool = new Tool_Navigation();
            var result = tool.AddSurface(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                collectObjects: 1,
                defaultArea: 1);

            Assert.IsTrue(result.success, "AddSurface should succeed on existing GameObject");
            Assert.AreEqual(1, result.collectObjects, "collectObjects should be Volume (1)");
            Assert.AreEqual(1, result.defaultArea, "defaultArea should be 1");
            Assert.IsNotNull(go.GetComponent<NavMeshSurface>(), "Surface should be added to the existing GameObject");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddAgent_ConfiguresSpeedAndStoppingDistance()
        {
            var tool = new Tool_Navigation();
            var result = tool.AddAgent(name: GO_AgentName, speed: 5.5f, stoppingDistance: 1.25f, areaMask: 3);

            Assert.IsTrue(result.success, "AddAgent should succeed");
            Assert.AreEqual(5.5f, result.speed, 0.001f, "speed should be configured");
            Assert.AreEqual(1.25f, result.stoppingDistance, 0.001f, "stoppingDistance should be configured");

            var go = GameObject.Find(GO_AgentName);
            Assert.IsNotNull(go, "The new agent GameObject should exist");
            var agent = go!.GetComponent<NavMeshAgent>();
            Assert.IsNotNull(agent, "It should host a NavMeshAgent");
            Assert.AreEqual(5.5f, agent!.speed, 0.001f, "Agent speed should be set on the component");
            Assert.AreEqual(3, agent.areaMask, "Agent areaMask should be set");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddModifier_SetsOverrideAreaAndFlags()
        {
            var go = new GameObject("ModifierHost");

            var tool = new Tool_Navigation();
            var result = tool.AddModifier(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                overrideArea: true,
                area: 4,
                ignoreFromBuild: false,
                applyToChildren: true);

            Assert.IsTrue(result.success, "AddModifier should succeed");
            var modifier = go.GetComponent<NavMeshModifier>();
            Assert.IsNotNull(modifier, "NavMeshModifier should be added");
            Assert.IsTrue(modifier!.overrideArea, "overrideArea should be true");
            Assert.AreEqual(4, modifier.area, "area should be 4");
            Assert.IsTrue(modifier.applyToChildren, "applyToChildren should be true");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddModifierVolume_SetsSizeAndArea()
        {
            var go = new GameObject("VolumeHost");

            var tool = new Tool_Navigation();
            var size = new Vector3(2, 3, 4);
            var result = tool.AddModifierVolume(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                size: size,
                area: 1);

            Assert.IsTrue(result.success, "AddModifierVolume should succeed");
            var volume = go.GetComponent<NavMeshModifierVolume>();
            Assert.IsNotNull(volume, "NavMeshModifierVolume should be added");
            Assert.AreEqual(size, volume!.size, "size should be set");
            Assert.AreEqual(1, volume.area, "area should be 1");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddLink_SetsEndpointsAndArea()
        {
            var go = new GameObject("LinkHost");

            var tool = new Tool_Navigation();
            var start = new Vector3(0, 0, -1);
            var end = new Vector3(0, 0, 3);
            var result = tool.AddLink(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                startPoint: start,
                endPoint: end,
                width: 1.5f,
                bidirectional: false,
                area: 2);

            Assert.IsTrue(result.success, "AddLink should succeed");
            var link = go.GetComponent<NavMeshLink>();
            Assert.IsNotNull(link, "NavMeshLink should be added");
            Assert.AreEqual(start, link!.startPoint, "startPoint should be set");
            Assert.AreEqual(end, link.endPoint, "endPoint should be set");
            Assert.AreEqual(1.5f, link.width, 0.001f, "width should be set");
            Assert.IsFalse(link.bidirectional, "bidirectional should be false");

            yield return null;
        }

        [UnityTest]
        public IEnumerator BakeSurface_ClearLeavesNoData()
        {
            var go = CreateGameObjectWithNavMeshSurface(GO_SurfaceName);

            var tool = new Tool_Navigation();
            // Clearing an unbaked surface should still succeed and report no data.
            var result = tool.BakeSurface(new GameObjectRef(go.GetInstanceID()), clear: true);

            Assert.IsTrue(result.success, "BakeSurface(clear) should succeed");
            Assert.IsTrue(result.cleared, "cleared flag should be true");
            Assert.IsFalse(result.hasNavMeshData, "Surface should have no NavMeshData after clear");

            yield return null;
        }
    }
}
