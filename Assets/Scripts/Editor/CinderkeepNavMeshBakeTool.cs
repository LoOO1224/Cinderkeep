using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
namespace Cinderkeep.EditorTools
{
    // 맵 청크 프리팹들의 NavMeshData를 다시 굽는 Editor 전용 도구입니다.
    // 맵 지형, 벽, 호수, 폐허 배치가 바뀐 뒤 이 도구를 다시 실행하면 됩니다.
    public static class CinderkeepNavMeshBakeTool
    {
        private const float _chunkBakeSize = 120f;
        private const float _chunkBakeHeight = 30f;
        private const string _commandLineBakeArgument = "-cinderkeepBakeMapNavMeshes";

        [InitializeOnLoadMethod]
        private static void RegisterCommandLineBake()
        {
            if (Application.isBatchMode == false)
            {
                return;
            }

            if (CheckHasCommandLineBakeArgument() == false)
            {
                return;
            }

            EditorApplication.delayCall += BakeMapNavMeshesFromCommandLine;
        }

        [MenuItem("Cinderkeep/Map/Bake Map NavMeshes")]
        public static void BakeMapNavMeshes()
        {
            List<string> prefabPaths = GetMapPrefabPaths();

            for (int i = 0; i < prefabPaths.Count; i++)
            {
                BakePrefabNavMesh(prefabPaths[i]);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CinderkeepNavMeshBakeTool: Map NavMesh bake finished.");
        }

        public static void BakeMapNavMeshesFromCommandLine()
        {
            try
            {
                BakeMapNavMeshes();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("CinderkeepNavMeshBakeTool: Bake failed. " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static bool CheckHasCommandLineBakeArgument()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == _commandLineBakeArgument)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> GetMapPrefabPaths()
        {
            List<string> prefabPaths = new List<string>();

            prefabPaths.Add("Assets/Prefabs/Map/PF_Map_GameMapGroup.prefab");

            for (int index = 1; index <= 8; index++)
            {
                string chunkNumber = index.ToString("00");
                prefabPaths.Add("Assets/Prefabs/Map/PF_Map_Chunk_" + chunkNumber + ".prefab");
            }

            return prefabPaths;
        }

        private static void BakePrefabNavMesh(string prefabPath)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                NavMeshSurface navMeshSurface = prefabRoot.GetComponent<NavMeshSurface>();

                if (navMeshSurface == null)
                {
                    Debug.LogWarning("CinderkeepNavMeshBakeTool: NavMeshSurface is missing. " + prefabPath);
                    return;
                }

                PrepareSurface(navMeshSurface);
                BakeSurface(navMeshSurface, prefabPath);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log("CinderkeepNavMeshBakeTool: Baked " + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void PrepareSurface(NavMeshSurface navMeshSurface)
        {
            // Children 방식은 해당 청크 프리팹 내부 오브젝트만 Bake에 사용합니다.
            // 다른 씬 오브젝트가 섞이지 않아서 모듈형 맵 청크에 적합합니다.
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.size = new Vector3(_chunkBakeSize, _chunkBakeHeight, _chunkBakeSize);
            navMeshSurface.center = new Vector3(0f, _chunkBakeHeight * 0.5f, 0f);
        }

        private static void BakeSurface(NavMeshSurface navMeshSurface, string prefabPath)
        {
            NavMeshData savedNavMeshData = navMeshSurface.navMeshData;

            if (savedNavMeshData == null)
            {
                Debug.LogWarning("CinderkeepNavMeshBakeTool: NavMeshData is missing. " + prefabPath);
                return;
            }

            navMeshSurface.BuildNavMesh();
            NavMeshData bakedNavMeshData = navMeshSurface.navMeshData;

            if (bakedNavMeshData == null)
            {
                Debug.LogWarning("CinderkeepNavMeshBakeTool: Bake result is empty. " + prefabPath);
                return;
            }

            if (bakedNavMeshData != savedNavMeshData)
            {
                EditorUtility.CopySerialized(bakedNavMeshData, savedNavMeshData);
                navMeshSurface.navMeshData = savedNavMeshData;
                UnityEngine.Object.DestroyImmediate(bakedNavMeshData);
            }

            EditorUtility.SetDirty(savedNavMeshData);
            EditorUtility.SetDirty(navMeshSurface);
        }
    }
}
