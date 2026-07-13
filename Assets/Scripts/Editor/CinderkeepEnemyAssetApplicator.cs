using System.IO;
using UnityEditor;
using UnityEngine;

// 적 프리팹의 AI와 전투 컴포넌트를 보존하면서 설원 테마의 저폴리 외형만 교체합니다.
// 일반 적 변형과 Frozen Golem 조립 외형의 크기, 재질, 프리뷰를 관리합니다.
public static class CinderkeepEnemyAssetApplicator
{
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const string EnemyPrefabFolder = "Assets/Prefabs/Enemy";
    private const string EnemyMaterialFolder = "Assets/Materials/Enemy";
    private const string ExternalAssetFolder = "Assets/ThirdParty/Free/CinderkeepExternalAssets";
    private const string MageFolder = ExternalAssetFolder + "/Mages";
    private const string PlantFolder = ExternalAssetFolder + "/CarnivorousPlant";
    private const string PolygonIceFolder = ExternalAssetFolder + "/PolygonIce";

    private const string FrostMagePrefabPath =
        EnemyPrefabFolder + "/PF_Enemy_LowPoly_FrostMage.prefab";
    private const string IcePlantPrefabPath =
        EnemyPrefabFolder + "/PF_Enemy_LowPoly_IcePlant.prefab";
    private const string FrozenGolemPrefabPath =
        EnemyPrefabFolder + "/PF_Boss_FrozenGolem.prefab";

    private const string MageBluePrefabPath = MageFolder + "/MageBlue.prefab";
    private const string PlantYellowPrefabPath =
        PlantFolder + "/Prefabs/Carnivorous Plant-Yellow.prefab";
    private const string RockSmallModelPath = PolygonIceFolder + "/SM_Rock_01.fbx";
    private const string RockLargeModelPath = PolygonIceFolder + "/SM_Rock_04.fbx";
    private const string CrystalCoreModelPath = PolygonIceFolder + "/FX_Crystal_01.fbx";
    private const string CrystalSpikeModelPath = PolygonIceFolder + "/FX_Crystal_04.fbx";
    private const string CrystalShardModelPath = PolygonIceFolder + "/FX_CrystalShard_01.fbx";

    private const string IcePlantMaterialPath = EnemyMaterialFolder + "/IcePlant_Frost.mat";
    private const string GolemBodyMaterialPath = EnemyMaterialFolder + "/FrozenGolem_Body.mat";
    private const string GolemCoreMaterialPath = EnemyMaterialFolder + "/FrozenGolem_Core.mat";
    private const string EnemyVisualPrefix = "Visual_";

    [MenuItem("Cinderkeep/Assets/Enemies/Preview Current Enemies")]
    public static void PreviewCurrentEnemies()
    {
        RenderAssetPreview(FrostMagePrefabPath, "Enemy_FrostMage_Current.png");
        RenderAssetPreview(IcePlantPrefabPath, "Enemy_IcePlant_Current.png");
        RenderAssetPreview(FrozenGolemPrefabPath, "Boss_FrozenGolem_Current.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] 현재 적 프리뷰를 생성했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Enemies/Apply Blue Frost Mage")]
    public static void ApplyBlueFrostMage()
    {
        bool didApply = ReplaceEnemyVisual(
            FrostMagePrefabPath,
            MageBluePrefabPath,
            "Visual_PF_Enemy_LowPoly_FrostMage",
            1.2f,
            null);

        if (didApply == false)
        {
            return;
        }

        RenderAssetPreview(FrostMagePrefabPath, "Enemy_FrostMage_Blue.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] FrostMage Blue 외형을 적용했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Enemies/Apply Frost Ice Plant")]
    public static void ApplyFrostIcePlant()
    {
        Material frostMaterial = GetOrCreateMaterial(
            IcePlantMaterialPath,
            new Color(0.18f, 0.68f, 0.82f, 1f),
            0.05f,
            0.28f);

        bool didApply = ReplaceEnemyVisual(
            IcePlantPrefabPath,
            PlantYellowPrefabPath,
            "Visual_PF_Enemy_LowPoly_IcePlant",
            1.4f,
            frostMaterial);

        if (didApply == false)
        {
            return;
        }

        RenderAssetPreview(IcePlantPrefabPath, "Enemy_IcePlant_Frost.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] IcePlant 서리 외형을 적용했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Enemies/Apply Frozen Golem")]
    public static void ApplyFrozenGolem()
    {
        Material bodyMaterial = GetOrCreateMaterial(
            GolemBodyMaterialPath,
            new Color(0.22f, 0.34f, 0.48f, 1f),
            0.12f,
            0.42f);
        Material coreMaterial = GetOrCreateMaterial(
            GolemCoreMaterialPath,
            new Color(0.18f, 0.78f, 1f, 1f),
            0.08f,
            0.58f);
        ConfigureEmission(coreMaterial, new Color(0.08f, 0.9f, 1.6f, 1f));

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(FrozenGolemPrefabPath);
        if (prefabRoot == null)
        {
            return;
        }

        try
        {
            DestroyExistingVisuals(prefabRoot.transform);

            GameObject visualRoot = new GameObject("Visual_PF_Boss_FrozenGolem");
            visualRoot.transform.SetParent(prefabRoot.transform, false);

            AddVisualPiece(
                visualRoot.transform,
                RockLargeModelPath,
                "Body_Torso",
                1.35f,
                new Vector3(0f, 1.15f, 0f),
                new Vector3(0f, 15f, 0f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                RockSmallModelPath,
                "Body_Head",
                0.55f,
                new Vector3(0f, 1.95f, -0.02f),
                new Vector3(0f, -20f, 8f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                RockSmallModelPath,
                "Body_Arm_Left",
                1.15f,
                new Vector3(-0.85f, 1.2f, 0f),
                new Vector3(0f, 5f, -52f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                RockSmallModelPath,
                "Body_Arm_Right",
                1.15f,
                new Vector3(0.85f, 1.2f, 0f),
                new Vector3(0f, -5f, 52f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                RockSmallModelPath,
                "Body_Leg_Left",
                0.9f,
                new Vector3(-0.38f, 0.42f, 0f),
                new Vector3(0f, 12f, -8f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                RockSmallModelPath,
                "Body_Leg_Right",
                0.9f,
                new Vector3(0.38f, 0.42f, 0f),
                new Vector3(0f, -12f, 8f),
                bodyMaterial);
            AddVisualPiece(
                visualRoot.transform,
                CrystalCoreModelPath,
                "Crystal_ChestCore",
                0.42f,
                new Vector3(0f, 1.25f, -0.58f),
                new Vector3(90f, 0f, 0f),
                coreMaterial);
            AddVisualPiece(
                visualRoot.transform,
                CrystalSpikeModelPath,
                "Crystal_Shoulder_Left",
                0.52f,
                new Vector3(-0.55f, 1.75f, 0f),
                new Vector3(0f, 0f, -24f),
                coreMaterial);
            AddVisualPiece(
                visualRoot.transform,
                CrystalSpikeModelPath,
                "Crystal_Shoulder_Right",
                0.52f,
                new Vector3(0.55f, 1.75f, 0f),
                new Vector3(0f, 0f, 24f),
                coreMaterial);
            AddVisualPiece(
                visualRoot.transform,
                CrystalShardModelPath,
                "Crystal_BackSpikes",
                0.75f,
                new Vector3(0f, 1.45f, 0.45f),
                new Vector3(-25f, 0f, 0f),
                coreMaterial);

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, FrozenGolemPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        RenderAssetPreview(FrozenGolemPrefabPath, "Boss_FrozenGolem_LowPoly.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] Frozen Golem 저폴리 외형을 적용했습니다.");
    }

    private static bool ReplaceEnemyVisual(
        string enemyPrefabPath,
        string sourceVisualPath,
        string visualName,
        float localScale,
        Material overrideMaterial)
    {
        GameObject sourceVisual = AssetDatabase.LoadAssetAtPath<GameObject>(sourceVisualPath);
        if (sourceVisual == null)
        {
            Debug.LogError("[CinderkeepEnemyAssetApplicator] 적 외형 에셋을 찾지 못했습니다: " + sourceVisualPath);
            return false;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
        if (prefabRoot == null)
        {
            return false;
        }

        bool didSave = false;
        try
        {
            DestroyExistingVisuals(prefabRoot.transform);

            GameObject visual = InstantiateAssetCanBeNull(sourceVisual);
            if (visual == null)
            {
                return false;
            }

            visual.name = visualName;
            visual.transform.SetParent(prefabRoot.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * localScale;
            DestroyCollidersInChildren(visual);

            if (overrideMaterial != null)
            {
                ApplyMaterialToRenderers(visual, overrideMaterial);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, enemyPrefabPath);
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

    private static void DestroyExistingVisuals(Transform rootTransform)
    {
        for (int i = rootTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = rootTransform.GetChild(i);
            if (child.name.StartsWith(EnemyVisualPrefix) == false)
            {
                continue;
            }

            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void AddVisualPiece(
        Transform visualRoot,
        string sourceModelPath,
        string pieceName,
        float targetHeight,
        Vector3 localCenter,
        Vector3 localEulerAngles,
        Material material)
    {
        GameObject sourceModel = AssetDatabase.LoadAssetAtPath<GameObject>(sourceModelPath);
        GameObject piece = InstantiateAssetCanBeNull(sourceModel);
        if (piece == null)
        {
            Debug.LogError("[CinderkeepEnemyAssetApplicator] 골렘 파츠를 만들지 못했습니다: " + sourceModelPath);
            return;
        }

        piece.name = pieceName;
        piece.transform.SetParent(visualRoot, false);
        piece.transform.localRotation = Quaternion.Euler(localEulerAngles);
        FitVisualToHeight(piece, targetHeight);
        CenterVisualAtLocalPosition(piece, visualRoot, localCenter);
        ApplyMaterialToRenderers(piece, material);
        DestroyCollidersInChildren(piece);
    }

    private static Material GetOrCreateMaterial(
        string materialPath,
        Color color,
        float metallic,
        float smoothness)
    {
        EnsureAssetFolder(EnemyMaterialFolder);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else
        {
            material.shader = shader;
        }

        SetMaterialColor(material, color);
        SetMaterialFloat(material, "_Metallic", metallic);
        SetMaterialFloat(material, "_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void ConfigureEmission(Material material, Color emissionColor)
    {
        if (material == null || material.HasProperty("_EmissionColor") == false)
        {
            return;
        }

        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);
        EditorUtility.SetDirty(material);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName) == false)
        {
            return;
        }

        material.SetFloat(propertyName, value);
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
        if (sourceAsset == null)
        {
            return null;
        }

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

    private static void CenterVisualAtLocalPosition(
        GameObject visual,
        Transform visualRoot,
        Vector3 localCenter)
    {
        Bounds bounds = GetRendererBounds(visual);
        Vector3 targetWorldCenter = visualRoot.TransformPoint(localCenter);
        visual.transform.position += targetWorldCenter - bounds.center;
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

    private static void RenderAssetPreview(string assetPath, string fileName)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject previewObject = InstantiateAssetCanBeNull(asset);
        if (previewObject == null)
        {
            return;
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
