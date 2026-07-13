using System.IO;
using UnityEditor;
using UnityEngine;

// 기존 광석 프리팹의 채집 컴포넌트와 팀원 모델을 보존하면서 등급별 얼음 결정 외형을 추가합니다.
// 광석 데이터와 채집 규칙은 수정하지 않고 시각물의 크기, 위치, 재질만 관리합니다.
public static class CinderkeepOreAssetApplicator
{
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const string HarvestableFolder = "Assets/Prefabs/Harvestables";
    private const string PolygonIceFolder =
        "Assets/ThirdParty/Free/CinderkeepExternalAssets/PolygonIce";
    private const string ModifiedAssetFolder = "Assets/ThirdParty/Free/Modify";

    private const string IronOrePrefabPath =
        HarvestableFolder + "/PF_4_0_Resource_Rock_IronOre.prefab";
    private const string GoldOrePrefabPath =
        HarvestableFolder + "/PF_4_0_Resource_Rock_GoldOre.prefab";
    private const string AdamantiumOrePrefabPath =
        HarvestableFolder + "/PF_4_0_Resource_Rock_AdamantiumOre.prefab";

    private const string IronCrystalPath = PolygonIceFolder + "/FX_Crystal_02.fbx";
    private const string GoldCrystalPath = PolygonIceFolder + "/FX_Crystal_03.fbx";
    private const string AdamantiumCrystalPath = PolygonIceFolder + "/FX_Crystal_04.fbx";

    private const string IronMaterialPath = ModifiedAssetFolder + "/Iron02.mat";
    private const string GoldMaterialPath = ModifiedAssetFolder + "/Gold02.mat";
    private const string AdamantiumMaterialPath = ModifiedAssetFolder + "/Adamantium02.mat";
    private const string CrystalVisualName = "Visual_OreCrystal";

    [MenuItem("Cinderkeep/Assets/Ores/Preview Ore Candidates")]
    public static void PreviewOreCandidates()
    {
        RenderAssetPreview(IronOrePrefabPath, "Ore_Iron_Current.png", 1f);
        RenderAssetPreview(GoldOrePrefabPath, "Ore_Gold_Current.png", 1f);
        RenderAssetPreview(AdamantiumOrePrefabPath, "Ore_Adamantium_Current.png", 1f);
        RenderAssetPreview(IronCrystalPath, "Ore_Crystal_02.png", 1f);
        RenderAssetPreview(GoldCrystalPath, "Ore_Crystal_03.png", 1f);
        RenderAssetPreview(AdamantiumCrystalPath, "Ore_Crystal_04.png", 1f);
        Debug.Log("[CinderkeepOreAssetApplicator] 광석과 결정 후보 프리뷰를 생성했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Ores/Apply Iron Crystal")]
    public static void ApplyIronCrystal()
    {
        ApplyCrystal(
            IronOrePrefabPath,
            IronCrystalPath,
            IronMaterialPath,
            2.1f,
            new Vector3(1.55f, 0f, -1.15f),
            new Vector3(0f, 18f, -8f),
            "PF_4_0_Resource_Rock_IronOre_Crystal.png");
    }

    [MenuItem("Cinderkeep/Assets/Ores/Apply Gold Crystal")]
    public static void ApplyGoldCrystal()
    {
        ApplyCrystal(
            GoldOrePrefabPath,
            GoldCrystalPath,
            GoldMaterialPath,
            1.02f,
            new Vector3(0.06f, 0f, -0.02f),
            new Vector3(-4f, -22f, 7f),
            "PF_4_0_Resource_Rock_GoldOre_Crystal.png");
    }

    [MenuItem("Cinderkeep/Assets/Ores/Apply Adamantium Crystal")]
    public static void ApplyAdamantiumCrystal()
    {
        ApplyCrystal(
            AdamantiumOrePrefabPath,
            AdamantiumCrystalPath,
            AdamantiumMaterialPath,
            1.15f,
            new Vector3(0f, 0f, 0f),
            new Vector3(6f, 30f, -5f),
            "PF_4_0_Resource_Rock_AdamantiumOre_Crystal.png");
    }

    private static void ApplyCrystal(
        string orePrefabPath,
        string crystalModelPath,
        string materialPath,
        float targetHeight,
        Vector3 localGroundOffset,
        Vector3 localEulerAngles,
        string previewFileName)
    {
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(crystalModelPath);
        Material crystalMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (sourceModel == null || crystalMaterial == null)
        {
            Debug.LogError("[CinderkeepOreAssetApplicator] 결정 모델 또는 광석 재질을 찾지 못했습니다.");
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(orePrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("[CinderkeepOreAssetApplicator] 광석 프리팹을 열지 못했습니다: " + orePrefabPath);
            return;
        }

        try
        {
            DestroyExistingCrystal(prefabRoot.transform);

            GameObject crystalVisual = InstantiateAssetCanBeNull(sourceModel);
            if (crystalVisual == null)
            {
                return;
            }

            crystalVisual.name = CrystalVisualName;
            crystalVisual.transform.SetParent(prefabRoot.transform, false);
            crystalVisual.transform.localRotation = Quaternion.Euler(localEulerAngles);
            FitVisualToHeight(crystalVisual, targetHeight);
            PlaceVisualOnLocalGround(crystalVisual, prefabRoot.transform, localGroundOffset);
            ApplyMaterialToRenderers(crystalVisual, crystalMaterial);
            DestroyCollidersInChildren(crystalVisual);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, orePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RenderAssetPreview(orePrefabPath, previewFileName, 1f);
        Debug.Log("[CinderkeepOreAssetApplicator] 광석 결정 외형 적용 완료: " + orePrefabPath);
    }

    private static void DestroyExistingCrystal(Transform parent)
    {
        Transform existingCrystal = parent.Find(CrystalVisualName);
        if (existingCrystal == null)
        {
            return;
        }

        Object.DestroyImmediate(existingCrystal.gameObject);
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

    private static void PlaceVisualOnLocalGround(
        GameObject visual,
        Transform rootTransform,
        Vector3 localGroundOffset)
    {
        Bounds bounds = GetRendererBounds(visual);
        Vector3 visualBase = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Vector3 worldGroundPosition = rootTransform.TransformPoint(localGroundOffset);
        visual.transform.position += worldGroundPosition - visualBase;
    }

    private static void ApplyMaterialToRenderers(GameObject visual, Material material)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>();
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Material[] materials = renderers[rendererIndex].sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                materials[materialIndex] = material;
            }

            renderers[rendererIndex].sharedMaterials = materials;
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

    private static void RenderAssetPreview(string assetPath, string fileName, float targetHeight)
    {
        GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject previewObject = InstantiateAssetCanBeNull(sourceAsset);
        if (previewObject == null)
        {
            Debug.LogWarning("[CinderkeepOreAssetApplicator] 프리뷰 대상을 찾지 못했습니다: " + assetPath);
            return;
        }

        if (assetPath.EndsWith(".fbx"))
        {
            FitVisualToHeight(previewObject, targetHeight);
            PlaceVisualOnLocalGround(previewObject, previewObject.transform, Vector3.zero);
        }

        RenderGameObjectPreview(previewObject, GetPreviewOutputPath(fileName));
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

    private static void RenderGameObjectPreview(GameObject previewObject, string outputPath)
    {
        Bounds bounds = GetRendererBounds(previewObject);
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        GameObject cameraObject = new GameObject("PreviewCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.06f, 0.08f, 0.12f, 1f);
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
