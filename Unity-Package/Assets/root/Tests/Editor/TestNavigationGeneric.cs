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
using System.Collections;
using com.IvanMurzak.ReflectorNet.Model;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Navigation.Editor.Tests
{
    public class TestNavigationGeneric : BaseTest
    {
        [UnityTest]
        public IEnumerator Get_SerializesNavMeshSurface()
        {
            var go = CreateGameObjectWithNavMeshSurface(GO_SurfaceName);
            var surface = go.GetComponent<NavMeshSurface>();

            var tool = new Tool_Navigation();
            var result = tool.GetComponentData(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                componentRef: new ComponentRef(surface!.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.data, "Serialized data should not be null");
            StringAssert.Contains("NavMeshSurface", result.componentType, "Component type should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Get_FirstNavMeshComponent_WhenNoComponentRef()
        {
            var go = CreateGameObjectWithNavMeshSurface(GO_SurfaceName);

            var tool = new Tool_Navigation();
            var result = tool.GetComponentData(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result.data, "Should serialize the first NavMesh component");
            StringAssert.Contains("NavMesh", result.componentType, "Resolved component should be a NavMesh type");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Modify_ModifierArea_ViaFieldsChannel()
        {
            var go = new GameObject("ModifierFieldsHost");
            var modifier = go.AddComponent<NavMeshModifier>();
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            const int newArea = 4;
            // 'm_Area' is the [SerializeField] private backing field of NavMeshModifier.area.
            // It must be supplied through the 'fields' channel (AddField). ReflectorNet's TryModify
            // resolves 'props' as PropertyInfo only and 'fields' as FieldInfo only — no cross-fallback.
            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: modifier.GetType().Name,
                    type: typeof(NavMeshModifier),
                    value: null)
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: "m_Area",
                    value: newArea));

            var tool = new Tool_Navigation();
            var result = tool.ModifyComponent(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                data: diff,
                componentRef: new ComponentRef(modifier.GetInstanceID()));

            Assert.IsTrue(result.success, "Modification should succeed");
            Assert.AreEqual(newArea, modifier.area, "area should be modified via the fields channel (m_Area)");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyJson_ModifierArea_Dispatch()
        {
            var go = new GameObject("ModifierJsonHost");
            var modifier = go.AddComponent<NavMeshModifier>();

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""componentRef"": {{ ""instanceID"": {modifier.GetInstanceID()} }},
                ""data"": {{
                    ""typeName"": ""Unity.AI.Navigation.NavMeshModifier"",
                    ""fields"": [
                        {{
                            ""name"": ""m_Area"",
                            ""typeName"": ""System.Int32"",
                            ""value"": 5
                        }}
                    ]
                }}
            }}";

            var result = RunToolAllowWarnings(Tool_Navigation.ModifyToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(5, modifier.area, "area should be modified via JSON dispatch through the fields channel");

            yield return null;
        }
    }
}
