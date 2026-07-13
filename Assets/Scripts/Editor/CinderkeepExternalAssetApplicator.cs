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
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const string FrozenTowerFolder =
        "Assets/ThirdParty/Free/CinderkeepExternalAssets/FrozenRuins";
    private const string WoodTowerModelPath = FrozenTowerFolder + "/KB3D_DKF_Tower_J.fbx";
    private const string IronTowerModelPath = FrozenTowerFolder + "/KB3D_DKF_Tower_A.fbx";
    private const string GoldTowerModelPath = FrozenTowerFolder + "/KB3D_DKF_Tower_E.fbx";
    private const string WoodTowerPrefabPath = "Assets/Prefabs/Building/PF_Building_Tower_Wood.prefab";
    private const string IronTowerPrefabPath = "Assets/Prefabs/Building/PF_Building_Tower_Iron.prefab";
    private const string GoldTowerPrefabPath = "Assets/Prefabs/Building/PF_Building_Tower_Gold.prefab";
    private const string BuildingMaterialFolder = "Assets/Materials/Building";
    private const string WoodTowerMaterialPath = BuildingMaterialFolder + "/Tower_Wood.mat";
    private const string IronTowerMaterialPath = BuildingMaterialFolder + "/Tower_Iron.mat";
    private const string GoldTowerMaterialPath = BuildingMaterialFolder + "/Tower_Gold.mat";
    private const string FrozenTowerSnowMaterialPath = BuildingMaterialFolder + "/Tower_Snow.mat";

    private static readonly string[] FrozenTowerCandidatePaths =
    {
        FrozenTowerFolder + "/KB3D_DKF_Tower_A.fbx",
        FrozenTowerFolder + "/KB3D_DKF_Tower_E.fbx",
        FrozenTowerFolder + "/KB3D_DKF_Tower_J.fbx"
    };

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

        GameObject visual = InstantiateAssetCanBeNull(sourceModel);
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

        RenderPrefabPreview(savedPrefab, GetPreviewOutputPath("PF_VFX_TowerArrow.png"));
        Debug.Log("[CinderkeepExternalAssetApplicator] 타워 화살 에셋 적용 완료: " + TowerArrowPrefabPath);
    }

    [MenuItem("Cinderkeep/Assets/Preview Frozen Tower Candidates")]
    public static void PreviewFrozenTowerCandidates()
    {
        for (int i = 0; i < FrozenTowerCandidatePaths.Length; i++)
        {
            string candidatePath = FrozenTowerCandidatePaths[i];
            GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
            if (sourceModel == null)
            {
                Debug.LogWarning("[CinderkeepExternalAssetApplicator] 타워 후보를 찾지 못했습니다: " + candidatePath);
                continue;
            }

            GameObject previewRoot = new GameObject("Preview_FrozenTower");
            GameObject visual = InstantiateAssetCanBeNull(sourceModel);
            if (visual == null)
            {
                Object.DestroyImmediate(previewRoot);
                Debug.LogWarning("[CinderkeepExternalAssetApplicator] 타워 후보 인스턴스 생성에 실패했습니다: " + candidatePath);
                continue;
            }

            visual.transform.SetParent(previewRoot.transform, false);
            FitVisualToHeight(visual, 4f);
            PlaceVisualOnGround(visual, Vector3.zero);

            string candidateName = Path.GetFileNameWithoutExtension(candidatePath);
            string outputPath = GetPreviewOutputPath(candidateName + ".png");
            RenderGameObjectPreview(previewRoot, outputPath);
        }

        Debug.Log("[CinderkeepExternalAssetApplicator] FrozenRuins 타워 후보 프리뷰 생성을 완료했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Apply Wood Tower Visual")]
    public static void ApplyWoodTowerVisual()
    {
        Material towerMaterial = GetOrCreateProjectMaterial(
            WoodTowerMaterialPath,
            new Color(0.3f, 0.17f, 0.08f, 1f),
            0f,
            0.22f);
        Material snowMaterial = GetOrCreateProjectMaterial(
            FrozenTowerSnowMaterialPath,
            new Color(0.52f, 0.64f, 0.72f, 1f),
            0.05f,
            0.32f);

        bool didApply = ApplyTowerVisual(
            WoodTowerPrefabPath,
            WoodTowerModelPath,
            towerMaterial,
            snowMaterial,
            "Visual_FrozenTower_Wood",
            2.4f);

        if (didApply == false)
        {
            return;
        }

        GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WoodTowerPrefabPath);
        RenderPrefabPreview(towerPrefab, GetPreviewOutputPath("PF_Building_Tower_Wood.png"));
        Debug.Log("[CinderkeepExternalAssetApplicator] 목재 타워 외형 적용 완료: " + WoodTowerPrefabPath);
    }

    [MenuItem("Cinderkeep/Assets/Apply Iron Tower Visual")]
    public static void ApplyIronTowerVisual()
    {
        Material towerMaterial = GetOrCreateProjectMaterial(
            IronTowerMaterialPath,
            new Color(0.28f, 0.34f, 0.4f, 1f),
            0.72f,
            0.4f);
        Material snowMaterial = GetOrCreateProjectMaterial(
            FrozenTowerSnowMaterialPath,
            new Color(0.52f, 0.64f, 0.72f, 1f),
            0.05f,
            0.32f);

        bool didApply = ApplyTowerVisual(
            IronTowerPrefabPath,
            IronTowerModelPath,
            towerMaterial,
            snowMaterial,
            "Visual_FrozenTower_Iron",
            2.5f);

        if (didApply == false)
        {
            return;
        }

        GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(IronTowerPrefabPath);
        RenderPrefabPreview(towerPrefab, GetPreviewOutputPath("PF_Building_Tower_Iron.png"));
        Debug.Log("[CinderkeepExternalAssetApplicator] 철제 타워 외형 적용 완료: " + IronTowerPrefabPath);
    }

    [MenuItem("Cinderkeep/Assets/Apply Gold Tower Visual")]
    public static void ApplyGoldTowerVisual()
    {
        Material towerMaterial = GetOrCreateProjectMaterial(
            GoldTowerMaterialPath,
            new Color(0.72f, 0.45f, 0.08f, 1f),
            0.65f,
            0.45f);
        Material snowMaterial = GetOrCreateProjectMaterial(
            FrozenTowerSnowMaterialPath,
            new Color(0.52f, 0.64f, 0.72f, 1f),
            0.05f,
            0.32f);

        bool didApply = ApplyTowerVisual(
            GoldTowerPrefabPath,
            GoldTowerModelPath,
            towerMaterial,
            snowMaterial,
            "Visual_FrozenTower_Gold",
            2.7f);

        if (didApply == false)
        {
            return;
        }

        GameObject towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GoldTowerPrefabPath);
        RenderPrefabPreview(towerPrefab, GetPreviewOutputPath("PF_Building_Tower_Gold.png"));
        Debug.Log("[CinderkeepExternalAssetApplicator] 금 타워 외형 적용 완료: " + GoldTowerPrefabPath);
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

    private static GameObject InstantiateAssetCanBeNull(GameObject sourceAsset)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(sourceAsset) as GameObject;
        if (instance != null)
        {
            return instance;
        }

        return Object.Instantiate(sourceAsset);
    }

    private static string GetPreviewOutputPath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "..",
            "Library",
            PreviewFolderName,
            fileName));
    }

    private static Material GetOrCreateProjectMaterial(
        string materialPath,
        Color color,
        float metallic,
        float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            EnsureAssetFolder(Path.GetDirectoryName(materialPath).Replace('\\', '/'));
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static bool ApplyTowerVisual(
        string towerPrefabPath,
        string towerModelPath,
        Material primaryMaterial,
        Material secondaryMaterial,
        string visualName,
        float targetHeight)
    {
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(towerModelPath);
        if (sourceModel == null)
        {
            Debug.LogError("[CinderkeepExternalAssetApplicator] 타워 모델을 찾지 못했습니다: " + towerModelPath);
            return false;
        }

        if (primaryMaterial == null || secondaryMaterial == null)
        {
            Debug.LogError("[CinderkeepExternalAssetApplicator] 타워 머티리얼을 찾지 못했습니다.");
            return false;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(towerPrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("[CinderkeepExternalAssetApplicator] 타워 프리팹을 열지 못했습니다: " + towerPrefabPath);
            return false;
        }

        bool didSave = false;
        try
        {
            Transform previousVisual = prefabRoot.transform.Find(visualName);
            if (previousVisual != null)
            {
                Object.DestroyImmediate(previousVisual.gameObject);
            }

            Renderer rootRenderer = prefabRoot.GetComponent<Renderer>();
            if (rootRenderer != null)
            {
                rootRenderer.enabled = false;
            }

            GameObject visual = InstantiateAssetCanBeNull(sourceModel);
            if (visual == null)
            {
                return false;
            }

            visual.name = visualName;
            visual.transform.SetParent(prefabRoot.transform, false);
            FitVisualToHeight(visual, targetHeight);

            Vector3 groundPosition = prefabRoot.transform.position + Vector3.down;
            PlaceVisualOnGround(visual, groundPosition);
            ApplyMaterialPaletteToRenderers(visual, primaryMaterial, secondaryMaterial);
            DestroyCollidersInChildren(visual);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, towerPrefabPath);
            didSave = true;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return didSave;
    }

    private static void ApplyMaterialPaletteToRenderers(
        GameObject visual,
        Material primaryMaterial,
        Material secondaryMaterial)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Material[] materials = renderers[rendererIndex].sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = materialIndex == 0
                    ? primaryMaterial
                    : secondaryMaterial;
            }

            renderers[rendererIndex].sharedMaterials = materials;
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

    private static void FitVisualToHeight(GameObject visual, float targetHeight)
    {
        Bounds bounds = GetRendererBounds(visual);
        if (bounds.size.y <= 0.01f)
        {
            return;
        }

        float scale = targetHeight / bounds.size.y;
        visual.transform.localScale *= scale;
    }

    private static void PlaceVisualOnGround(GameObject visual, Vector3 groundPosition)
    {
        Bounds bounds = GetRendererBounds(visual);
        Vector3 visualBasePosition = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        visual.transform.position += groundPosition - visualBasePosition;
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
            Material[] materials = renderers[i].sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = material;
            }

            renderers[i].sharedMaterials = materials;
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
        GameObject previewObject = InstantiateAssetCanBeNull(prefab);
        if (previewObject == null)
        {
            return;
        }

        RenderGameObjectPreview(previewObject, outputPath);
    }

    private static void RenderGameObjectPreview(GameObject previewObject, string outputPath)
    {

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
