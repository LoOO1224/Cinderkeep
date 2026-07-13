using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 실제 장비 프리팹을 1인칭 카메라의 표시 오브젝트로 연결합니다.
// 채집과 공격 판정은 변경하지 않고 폴백 도형을 교체하는 에디터 도구입니다.
public static class CinderkeepFirstPersonEquipmentApplicator
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string HandStonePrefabPath =
        "Assets/Prefabs/Equipment/PF_Equipment_HandStone.prefab";
    private const string HandStoneViewName = "GameObject_HandStoneView";
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const int PreviewLayer = 31;

    [MenuItem("Cinderkeep/Assets/First Person Equipment/Apply Hand Stone")]
    public static void ApplyHandStone()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        FirstPersonToolView toolView = Object.FindFirstObjectByType<FirstPersonToolView>(
            FindObjectsInactive.Include);
        GameObject handStonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandStonePrefabPath);
        if (toolView == null || handStonePrefab == null)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] 손돌 View 연결 대상을 찾지 못했습니다.");
            return;
        }

        GameObject handStoneView = GetOrCreateView(toolView.transform, HandStoneViewName);
        ClearChildren(handStoneView.transform);

        GameObject visual = PrefabUtility.InstantiatePrefab(handStonePrefab, scene) as GameObject;
        if (visual == null)
        {
            Debug.LogError("[CinderkeepFirstPersonEquipmentApplicator] 손돌 프리팹을 생성하지 못했습니다.");
            return;
        }

        visual.name = "Visual_HandStone";
        visual.transform.SetParent(handStoneView.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        RemoveColliders(visual);
        RemoveGameplayComponents(visual);
        SetLayerRecursively(visual.transform, 0);
        FitVisualToSize(visual, handStoneView.transform, 0.34f);

        handStoneView.transform.localPosition = Vector3.zero;
        handStoneView.transform.localRotation = Quaternion.Euler(10f, -18f, 8f);
        handStoneView.SetActive(false);

        SerializedObject serializedToolView = new SerializedObject(toolView);
        serializedToolView.FindProperty("_handStoneView").objectReferenceValue = handStoneView;
        serializedToolView.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(toolView);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RenderPrefabPreview(handStonePrefab, "FirstPerson_HandStone.png");
        Debug.Log("[CinderkeepFirstPersonEquipmentApplicator] 손돌 1인칭 View를 연결했습니다.");
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
        GameObject previewObject = Object.Instantiate(prefab);
        SetLayerRecursively(previewObject.transform, PreviewLayer);
        Bounds bounds = GetRendererBounds(previewObject);
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

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
