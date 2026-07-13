using System.IO;
using UnityEditor;
using UnityEngine;

// 외부 후보에서 선별된 에셋을 Cinderkeep용 경량 프리팹으로 변환합니다.
// 원본 패키지 전체가 아니라 이미 임포트된 모델과 프로젝트 재질만 연결합니다.
public static class CinderkeepExternalAssetApplicator
{
    private const string ArrowModelPath =
        "Assets/ThirdParty/Free/CinderkeepExternalAssets/PolygonParticles/Models/FX_Arrow_01.fbx";
    private const string ArrowMaterialPath = "Assets/Materials/Generated/Projectile_Arrow.mat";
    private const string TowerArrowPrefabFolder = "Assets/Resources/Cinderkeep/prefabs/vfx";
    private const string TowerArrowPrefabPath = TowerArrowPrefabFolder + "/PF_VFX_TowerArrow.prefab";
    private const string TowerArrowPreviewPath = "Temp/Cinderkeep/PF_VFX_TowerArrow.png";

    [MenuItem("Cinderkeep/Assets/Apply Tower Arrow")]
    public static void ApplyTowerArrow()
    {
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(ArrowModelPath);
        Material arrowMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArrowMaterialPath);
        if (sourceModel == null || arrowMaterial == null)
        {
            Debug.LogError("[CinderkeepExternalAssetApplicator] 화살 모델 또는 재질을 찾지 못했습니다.");
            return;
        }

        EnsureAssetFolder(TowerArrowPrefabFolder);

        GameObject prefabRoot = new GameObject("PF_VFX_TowerArrow");
        prefabRoot.AddComponent<TowerProjectileView>();

        GameObject visual = PrefabUtility.InstantiatePrefab(sourceModel) as GameObject;
        if (visual == null)
        {
            Object.DestroyImmediate(prefabRoot);
            Debug.LogError("[CinderkeepExternalAssetApplicator] 화살 모델 인스턴스를 만들지 못했습니다.");
            return;
        }

        visual.name = "Visual_Arrow";
        visual.transform.SetParent(prefabRoot.transform, false);
        AlignLongestAxisToForward(visual);
        FitVisualToLongestAxis(visual, 0.8f);
        CenterVisualAtRoot(visual, prefabRoot.transform.position);
        ApplyMaterialToRenderers(visual, arrowMaterial);
        DestroyCollidersInChildren(visual);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, TowerArrowPrefabPath);
        Object.DestroyImmediate(prefabRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (savedPrefab == null)
        {
            Debug.LogError("[CinderkeepExternalAssetApplicator] 타워 화살 프리팹 저장에 실패했습니다.");
            return;
        }

        RenderPrefabPreview(savedPrefab, TowerArrowPreviewPath);
        Debug.Log("[CinderkeepExternalAssetApplicator] 타워 화살 에셋 적용 완료: " + TowerArrowPrefabPath);
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        string[] folders = folderPath.Split('/');
        string currentPath = folders[0];

        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = currentPath + "/" + folders[i];
            if (AssetDatabase.IsValidFolder(nextPath) == false)
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }

            currentPath = nextPath;
        }
    }

    private static void AlignLongestAxisToForward(GameObject visual)
    {
        Bounds bounds = GetRendererBounds(visual);
        if (bounds.size.x >= bounds.size.y && bounds.size.x >= bounds.size.z)
        {
            visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            return;
        }

        if (bounds.size.y >= bounds.size.x && bounds.size.y >= bounds.size.z)
        {
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    private static void FitVisualToLongestAxis(GameObject visual, float targetSize)
    {
        Bounds bounds = GetRendererBounds(visual);
        float longestAxis = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (longestAxis <= 0.01f)
        {
            return;
        }

        float scale = targetSize / longestAxis;
        visual.transform.localScale *= scale;
    }

    private static void CenterVisualAtRoot(GameObject visual, Vector3 rootPosition)
    {
        Bounds bounds = GetRendererBounds(visual);
        Vector3 centerOffset = bounds.center - rootPosition;
        visual.transform.position -= centerOffset;
    }

    private static Bounds GetRendererBounds(GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        if (renderers.Length <= 0)
        {
            return new Bounds(visual.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void ApplyMaterialToRenderers(GameObject visual, Material material)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    private static void DestroyCollidersInChildren(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>();
        for (int i = colliders.Length - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static void RenderPrefabPreview(GameObject prefab, string outputPath)
    {
        GameObject previewObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (previewObject == null)
        {
            return;
        }

        Bounds bounds = GetRendererBounds(previewObject);
        GameObject cameraObject = new GameObject("PreviewCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        camera.transform.position = bounds.center + new Vector3(
            objectSize * 2.5f,
            objectSize * 1.2f,
            objectSize * -0.35f);
        camera.transform.LookAt(bounds.center);
        camera.fieldOfView = 35f;
        camera.nearClipPlane = 0.01f;

        GameObject lightObject = new GameObject("PreviewLight");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2f;
        light.transform.rotation = Quaternion.Euler(35f, -35f, 0f);

        RenderTexture renderTexture = new RenderTexture(512, 512, 24);
        camera.targetTexture = renderTexture;
        camera.Render();

        RenderTexture.active = renderTexture;
        Texture2D previewTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        previewTexture.ReadPixels(new Rect(0f, 0f, 512f, 512f), 0, 0);
        previewTexture.Apply();

        string directoryPath = Path.GetDirectoryName(outputPath);
        if (string.IsNullOrEmpty(directoryPath) == false)
        {
            Directory.CreateDirectory(directoryPath);
        }

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
