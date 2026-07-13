using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// 실제 장비 프리팹을 1인칭 카메라의 표시 오브젝트로 연결합니다.
// 채집과 공격 판정은 변경하지 않고 폴백 도형을 교체하는 에디터 도구입니다.
public static class CinderkeepFirstPersonEquipmentApplicator
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string HandStonePrefabPath =
        "Assets/Prefabs/Equipment/PF_Equipment_HandStone.prefab";
    private const string StoneAxePrefabPath =
        "Assets/Prefabs/Equipment/PF_Equipment_Axe_Stone.prefab";
    private const string StonePickaxePrefabPath =
        "Assets/Prefabs/Equipment/PF_Equipment_Pickaxe_Stone.prefab";
    private const string HandStoneViewName = "GameObject_HandStoneView";
    private const string AxeViewName = "GameObject_AxeView";
    private const string PickaxeViewName = "GameObject_PickaxeView";
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const int PreviewLayer = 31;

    [MenuItem("Cinderkeep/Assets/First Person Equipment/Apply Hand Stone")]
    public static void ApplyHandStone()
    {
        ApplyEquipmentView(
            HandStonePrefabPath,
            HandStoneViewName,
            "_handStoneView",
            "Visual_HandStone",
            Vector3.zero,
            new Vector3(10f, -18f, 8f),
            0.34f,
            "FirstPerson_HandStone.png",
            "손돌");
    }

    [MenuItem("Cinderkeep/Assets/First Person Equipment/Apply Stone Axe")]
    public static void ApplyStoneAxe()
    {
        ApplyEquipmentView(
            StoneAxePrefabPath,
            AxeViewName,
            "_axeView",
            "Visual_StoneAxe",
            Vector3.zero,
            new Vector3(8f, -12f, -18f),
            1.05f,
            "FirstPerson_StoneAxe.png",
            "돌도끼");
    }

    [MenuItem("Cinderkeep/Assets/First Person Equipment/Apply Stone Pickaxe")]
    public static void ApplyStonePickaxe()
    {
        ApplyEquipmentView(
            StonePickaxePrefabPath,
            PickaxeViewName,
            "_pickaxeView",
            "Visual_StonePickaxe",
            Vector3.zero,
            new Vector3(8f, -12f, -18f),
            1.05f,
            "FirstPerson_StonePickaxe.png",
            "돌곡괭이");
    }

    private static void ApplyEquipmentView(
        string prefabPath,
        string viewName,
        string serializedPropertyName,
        string visualName,
        Vector3 visualEulerAngles,
        Vector3 viewEulerAngles,
        float targetSize,
        string previewFileName,
        string displayName)
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        FirstPersonToolView toolView = Object.FindFirstObjectByType<FirstPersonToolView>(
            FindObjectsInactive.Include);
        GameObject equipmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (toolView == null || equipmentPrefab == null)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] " + displayName + " View 연결 대상을 찾지 못했습니다.");
            return;
        }

        SerializedObject serializedToolView = new SerializedObject(toolView);
        SerializedProperty viewProperty = serializedToolView.FindProperty(serializedPropertyName);
        if (viewProperty == null)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] " + serializedPropertyName + " 필드를 찾지 못했습니다.");
            return;
        }

        GameObject viewObject = GetOrCreateView(toolView.transform, viewName);
        ClearChildren(viewObject.transform);

        GameObject visual = PrefabUtility.InstantiatePrefab(equipmentPrefab, scene) as GameObject;
        if (visual == null)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] " + displayName + " 프리팹을 생성하지 못했습니다.");
            return;
        }

        visual.name = visualName;
        visual.transform.SetParent(viewObject.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(visualEulerAngles);
        visual.transform.localScale = Vector3.one;
        RemoveColliders(visual);
        RemoveGameplayComponents(visual);
        SetLayerRecursively(visual.transform, 0);
        FitVisualToSize(visual, viewObject.transform, targetSize);

        viewObject.transform.localPosition = Vector3.zero;
        viewObject.transform.localRotation = Quaternion.Euler(viewEulerAngles);
        viewObject.SetActive(false);

        viewProperty.objectReferenceValue = viewObject;
        serializedToolView.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(toolView);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RenderPrefabPreview(equipmentPrefab, previewFileName);
        Debug.Log("[CinderkeepFirstPersonEquipmentApplicator] " + displayName + " 1인칭 View를 연결했습니다.");
    }

    private static GameObject GetOrCreateView(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject viewObject = new GameObject(objectName);
        viewObject.transform.SetParent(parent, false);
        return viewObject;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void RemoveColliders(GameObject rootObject)
    {
        Collider[] colliders = rootObject.GetComponentsInChildren<Collider>(true);
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static void RemoveGameplayComponents(GameObject rootObject)
    {
        MonoBehaviour[] behaviours = rootObject.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = behaviours.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(behaviours[i]);
        }

        Rigidbody[] rigidbodies = rootObject.GetComponentsInChildren<Rigidbody>(true);
        for (int i = rigidbodies.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(rigidbodies[i]);
        }

        rootObject.tag = "Untagged";
    }

    private static void SetLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layer);
        }
    }

    private static void FitVisualToSize(GameObject visual, Transform viewRoot, float targetSize)
    {
        Bounds bounds = GetRendererBounds(visual);
        float largestSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (largestSize <= 0.001f)
        {
            return;
        }

        visual.transform.localScale *= targetSize / largestSize;
        bounds = GetRendererBounds(visual);
        visual.transform.position += viewRoot.position - bounds.center;
    }

    private static Bounds GetRendererBounds(GameObject rootObject)
    {
        Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length <= 0)
        {
            return new Bounds(rootObject.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void RenderPrefabPreview(GameObject prefab, string fileName)
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            Debug.LogWarning("[CinderkeepFirstPersonEquipmentApplicator] 그래픽 장치가 없어 프리뷰 생성을 건너뜁니다.");
            return;
        }

        GameObject previewObject = Object.Instantiate(prefab);
        SetLayerRecursively(previewObject.transform, PreviewLayer);

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length <= 0)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] " + prefab.name + " 프리팹에 Renderer가 없습니다.");
            Object.DestroyImmediate(previewObject);
            return;
        }

        Bounds bounds = GetRendererBounds(previewObject);
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (objectSize <= 0.001f)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] " + prefab.name + " 프리팹의 표시 크기가 0입니다.");
            Object.DestroyImmediate(previewObject);
            return;
        }

        Debug.Log(
            "[CinderkeepFirstPersonEquipmentApplicator] Preview " + prefab.name
            + " / Renderers=" + renderers.Length
            + " / Bounds=" + bounds);

        GameObject cameraObject = new GameObject("PreviewCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
        camera.cullingMask = 1 << PreviewLayer;
        camera.transform.position = bounds.center + new Vector3(
            objectSize * 1.35f,
            objectSize * 1.05f,
            objectSize * -2.1f);
        camera.transform.LookAt(bounds.center);
        camera.fieldOfView = 35f;
        camera.nearClipPlane = 0.01f;

        GameObject lightObject = new GameObject("PreviewLight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2f;
        light.cullingMask = 1 << PreviewLayer;
        light.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        camera.targetTexture = renderTexture;
        camera.Render();
        RenderTexture.active = renderTexture;

        Texture2D previewTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        previewTexture.ReadPixels(new Rect(0f, 0f, 512f, 512f), 0, 0);
        previewTexture.Apply();

        string outputPath = Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "Library",
            PreviewFolderName,
            fileName));
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        File.WriteAllBytes(outputPath, previewTexture.EncodeToPNG());

        RenderTexture.active = null;
        camera.targetTexture = null;
        Object.DestroyImmediate(previewTexture);
        renderTexture.Release();
        Object.DestroyImmediate(renderTexture);
        Object.DestroyImmediate(lightObject);
        Object.DestroyImmediate(cameraObject);
        Object.DestroyImmediate(previewObject);
    }
}
