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
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Navigation
    {
        /// <summary>Resolve an optional GameObjectRef to a Transform (null when the ref is null/empty/invalid).</summary>
        static Transform? ResolveOptionalTransform(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null || !gameObjectRef.IsValid(out _))
                return null;

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new ArgumentException(error, paramName);
            if (go == null)
                throw new ArgumentException(Error.GameObjectNotFound(), paramName);

            return go.transform;
        }

        /// <summary>Resolve a required GameObjectRef to its GameObject (throws on failure).</summary>
        static GameObject ResolveGameObject(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(paramName);
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, paramName);

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new Exception(error);
            if (go == null)
                throw new Exception(Error.GameObjectNotFound());

            return go;
        }

        /// <summary>Resolve a required GameObjectRef to a NavMeshSurface (throws on failure).</summary>
        static NavMeshSurface ResolveNavMeshSurface(GameObjectRef? gameObjectRef, string paramName)
        {
            var go = ResolveGameObject(gameObjectRef, paramName);
            var surface = go.GetComponent<NavMeshSurface>();
            if (surface == null)
                throw new Exception(Error.NavMeshSurfaceNotFound());
            return surface;
        }

        /// <summary>Resolve a required GameObjectRef to a NavMeshAgent (throws on failure).</summary>
        static NavMeshAgent ResolveNavMeshAgent(GameObjectRef? gameObjectRef, string paramName)
        {
            var go = ResolveGameObject(gameObjectRef, paramName);
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent == null)
                throw new Exception(Error.NavMeshAgentNotFound());
            return agent;
        }

        /// <summary>Mark a scene object dirty and repaint the editor after a mutation.</summary>
        static void MarkDirtyAndRepaint(UnityEngine.Object target, UnityEngine.SceneManagement.Scene scene)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();
        }
    }
}
