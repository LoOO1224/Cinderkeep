using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
public static class CinderkeepCinderHeartVisualSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string CinderHeartName = "CinderHeart";
    private const string VisualRootName = "Visual_CinderHeart_Core";
    private const string ShrineVisualName = "Model_CinderHeart_Shrine";

    [MenuItem("Cinderkeep/Setup Visible CinderHeart")]
    public static void SetupVisibleCinderHeart()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        GameObject cinderHeart = GetRootObject(CinderHeartName);

        if (cinderHeart == null)
        {
            Debug.LogWarning("CinderkeepCinderHeartVisualSceneBuilder: CinderHeart를 찾지 못했습니다.");
            return;
        }

        GameObject shrineVisualPrefab =
            CinderkeepExternalAssetApplicator.CreateOrUpdateCinderHeartShrineVisualPrefab();
        if (shrineVisualPrefab == null)
        {
            return;
        }

        Transform visualRoot = GetOrCreateChild(cinderHeart.transform, VisualRootName);
        ReplaceVisualWithPrefab(visualRoot, shrineVisualPrefab, scene);
        CreateOrUpdateLight(cinderHeart);
        SetupCinderHeartStatus(cinderHeart);
        SetupPlayerDeathView(cinderHeart.transform);

        EditorUtility.SetDirty(cinderHeart);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CinderkeepCinderHeartVisualSceneBuilder: CinderHeart visual setup finished.");
    }

    private static GameObject GetRootObject(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].name == objectName)
            {
                return rootObjects[i];
            }
        }

        return null;
    }

    private static Transform GetOrCreateChild(Transform parent, string objectName)
    {
        Transform childTransform = parent.Find(objectName);
        if (childTransform != null)
        {
            return childTransform;
        }

        GameObject childObject = new GameObject(objectName);
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject.transform;
    }

    private static void ReplaceVisualWithPrefab(
        Transform visualRoot,
        GameObject visualPrefab,
        Scene scene)
    {
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(visualRoot.GetChild(i).gameObject);
        }

        GameObject visualInstance = PrefabUtility.InstantiatePrefab(visualPrefab, scene) as GameObject;
        if (visualInstance == null)
        {
            Debug.LogError("CinderkeepCinderHeartVisualSceneBuilder: CinderHeart 제단 프리팹 생성에 실패했습니다.");
            return;
        }

        visualInstance.name = ShrineVisualName;
        visualInstance.transform.SetParent(visualRoot, false);
        visualInstance.transform.localPosition = new Vector3(0f, -0.8f, 0f);
        visualInstance.transform.localRotation = Quaternion.identity;
        visualInstance.transform.localScale = Vector3.one;
    }

    private static void CreateOrUpdateLight(GameObject cinderHeart)
    {
        Transform previousLight = cinderHeart.transform.Find("Light_CinderHeart_Beacon");
        if (previousLight != null)
        {
            Object.DestroyImmediate(previousLight.gameObject);
        }

        Light light = cinderHeart.GetComponent<Light>();
        if (light == null)
        {
            light = cinderHeart.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = new Color(1f, 0.22f, 0.045f, 1f);
        light.range = 15f;
        light.intensity = 4f;
        light.shadows = LightShadows.None;
    }

    private static void SetupCinderHeartStatus(GameObject cinderHeart)
    {
        CinderHeart cinderHeartComponent = cinderHeart.GetComponent<CinderHeart>();
        if (cinderHeartComponent == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(cinderHeartComponent);
        serializedObject.FindProperty("_testDamage").floatValue = 10f;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetupPlayerDeathView(Transform cinderHeartTransform)
    {
        GameObject player = GetRootObject("Player");
        if (player == null)
        {
            return;
        }

        DeathCinderHeartView deathView = player.GetComponent<DeathCinderHeartView>();
        if (deathView == null)
        {
            deathView = player.AddComponent<DeathCinderHeartView>();
        }

        Camera playerCamera = GetComponentInChildrenByName<Camera>(player, "Camera_FirstPerson");

        SerializedObject serializedObject = new SerializedObject(deathView);
        serializedObject.FindProperty("_targetCamera").objectReferenceValue = playerCamera;
        serializedObject.FindProperty("_cinderHeartTarget").objectReferenceValue = cinderHeartTransform;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static T GetComponentInChildrenByName<T>(GameObject rootObject, string objectName)
        where T : Component
    {
        T[] components = rootObject.GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].gameObject.name == objectName)
            {
                return components[i];
            }
        }

        return null;
    }

}
