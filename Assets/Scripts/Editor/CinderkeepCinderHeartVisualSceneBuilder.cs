using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
public static class CinderkeepCinderHeartVisualSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string MaterialFolderPath = "Assets/Materials/Generated";
    private const string CinderHeartName = "CinderHeart";

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

        Material cinderHeartMaterial = GetOrCreateMaterial("MAT_CinderHeart_Visible_Red", new Color(1f, 0.05f, 0.02f, 1f));
        Transform visualRoot = GetOrCreateChild(cinderHeart.transform, "Visual_CinderHeart_Core");

        CreateOrUpdatePrimitive(visualRoot, "Beacon_CinderHeart_Core", PrimitiveType.Sphere, new Vector3(0f, 1.9f, 0f), new Vector3(2.0f, 2.0f, 2.0f), cinderHeartMaterial);
        CreateOrUpdatePrimitive(visualRoot, "Beacon_CinderHeart_Pillar", PrimitiveType.Cylinder, new Vector3(0f, 2.5f, 0f), new Vector3(0.36f, 3.0f, 0.36f), cinderHeartMaterial);
        CreateOrUpdatePrimitive(visualRoot, "Beacon_CinderHeart_Base", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(2.2f, 0.24f, 2.2f), cinderHeartMaterial);
        CreateOrUpdateLight(cinderHeart.transform);
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

    private static void CreateOrUpdatePrimitive(
        Transform parent,
        string objectName,
        PrimitiveType primitiveType,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        Transform childTransform = parent.Find(objectName);
        GameObject childObject;

        if (childTransform == null)
        {
            childObject = GameObject.CreatePrimitive(primitiveType);
            childObject.name = objectName;
            childObject.transform.SetParent(parent);
            RemoveCollider(childObject);
        }
        else
        {
            childObject = childTransform.gameObject;
        }

        childObject.transform.localPosition = localPosition;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = localScale;
        ApplyMaterial(childObject, material);
    }

    private static void RemoveCollider(GameObject targetObject)
    {
        Collider collider = targetObject.GetComponent<Collider>();
        if (collider == null)
        {
            return;
        }

        Object.DestroyImmediate(collider);
    }

    private static void ApplyMaterial(GameObject targetObject, Material material)
    {
        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = material;
    }

    private static void CreateOrUpdateLight(Transform cinderHeartTransform)
    {
        Transform lightTransform = cinderHeartTransform.Find("Light_CinderHeart_Beacon");
        GameObject lightObject;

        if (lightTransform == null)
        {
            lightObject = new GameObject("Light_CinderHeart_Beacon");
            lightObject.transform.SetParent(cinderHeartTransform);
        }
        else
        {
            lightObject = lightTransform.gameObject;
        }

        lightObject.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        lightObject.transform.localRotation = Quaternion.identity;
        lightObject.transform.localScale = Vector3.one;

        Light light = lightObject.GetComponent<Light>();
        if (light == null)
        {
            light = lightObject.AddComponent<Light>();
        }

        light.type = LightType.Point;
        light.color = new Color(1f, 0.2f, 0.06f, 1f);
        light.range = 14f;
        light.intensity = 3.5f;
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

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        EnsureFolder("Assets/Materials");
        EnsureFolder(MaterialFolderPath);

        string materialPath = MaterialFolderPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material != null)
        {
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = materialName;
        material.color = color;
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parentPath = System.IO.Path.GetDirectoryName(folderPath);
        string folderName = System.IO.Path.GetFileName(folderPath);
        parentPath = parentPath.Replace("\\", "/");
        AssetDatabase.CreateFolder(parentPath, folderName);
    }
}
