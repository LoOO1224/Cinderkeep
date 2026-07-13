using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// 적 프리팹의 AI와 전투 컴포넌트를 보존하면서 설원 테마의 저폴리 외형만 교체합니다.
// 일반 적 변형과 Frozen Golem 조립 외형의 크기, 재질, 프리뷰를 관리합니다.
public static class CinderkeepEnemyAssetApplicator
{
    private const string PreviewFolderName = "CinderkeepAssetPreviews";
    private const string EnemyPrefabFolder = "Assets/Prefabs/Enemy";
    private const string EnemyMaterialFolder = "Assets/Materials/Enemy";
    private const string EnemyAnimationFolder = "Assets/Animations/Enemy";
    private const string ExternalAssetFolder = "Assets/ThirdParty/Free/CinderkeepExternalAssets";
    private const string MageFolder = ExternalAssetFolder + "/Mages";
    private const string PlantFolder = ExternalAssetFolder + "/CarnivorousPlant";
    private const string GolemFolder = ExternalAssetFolder + "/GolemCollection";

    private const string RedMagePrefabPath =
        EnemyPrefabFolder + "/PF_Enemy_LowPoly_RedMage.prefab";
    private const string FrostMagePrefabPath =
        EnemyPrefabFolder + "/PF_Enemy_LowPoly_FrostMage.prefab";
    private const string IcePlantPrefabPath =
        EnemyPrefabFolder + "/PF_Enemy_LowPoly_IcePlant.prefab";
    private const string FrozenGolemPrefabPath =
        EnemyPrefabFolder + "/PF_Boss_FrozenGolem.prefab";

    private const string MageBluePrefabPath = MageFolder + "/MageBlue.prefab";
    private const string MageTexturePath = MageFolder + "/Textures/MagesTexture.png";
    private const string PlantYellowPrefabPath =
        PlantFolder + "/Prefabs/Carnivorous Plant-Yellow.prefab";
    private const string CrystalGolemModelPath = GolemFolder + "/CrystalGolem.FBX";
    private const string CrystalGolemIdlePath = GolemFolder + "/GolemIdle.FBX";
    private const string CrystalGolemDiffusePath = GolemFolder + "/CrG_Diffuse.png";
    private const string CrystalGolemNormalPath = GolemFolder + "/CrGNormals.png";

    private const string RedMageMaterialPath = EnemyMaterialFolder + "/FireMage_Red.mat";
    private const string IcePlantMaterialPath = EnemyMaterialFolder + "/IcePlant_Frost.mat";
    private const string FrostMageMaterialPath = EnemyMaterialFolder + "/FrostMage_Blue.mat";
    private const string FrozenGolemMaterialPath = EnemyMaterialFolder + "/FrozenGolem_Crystal.mat";
    private const string FrozenGolemControllerPath =
        EnemyAnimationFolder + "/FrozenGolem_Idle.controller";
    private const string EnemyVisualPrefix = "Visual_";

    [MenuItem("Cinderkeep/Assets/Enemies/Preview Current Enemies")]
    public static void PreviewCurrentEnemies()
    {
        RenderAssetPreview(RedMagePrefabPath, "Enemy_RedMage_Current.png");
        RenderAssetPreview(FrostMagePrefabPath, "Enemy_FrostMage_Current.png");
        RenderAssetPreview(IcePlantPrefabPath, "Enemy_IcePlant_Current.png");
        RenderAssetPreview(FrozenGolemPrefabPath, "Boss_FrozenGolem_Current.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] 현재 적 프리뷰를 생성했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Enemies/Apply Red Fire Mage")]
    public static void ApplyRedFireMage()
    {
        Material redMageMaterial = GetOrCreateMaterial(
            RedMageMaterialPath,
            Color.white,
            0f,
            0.28f);
        Texture2D mageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MageTexturePath);
        SetMaterialTexture(redMageMaterial, mageTexture);

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(RedMagePrefabPath);
        if (prefabRoot == null)
        {
            return;
        }

        try
        {
            ApplyMaterialToRenderers(prefabRoot, redMageMaterial);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, RedMagePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RenderAssetPreview(RedMagePrefabPath, "Enemy_RedMage_Fire.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] RedMage 원본 아틀라스를 적용했습니다.");
    }

    [MenuItem("Cinderkeep/Assets/Enemies/Apply Blue Frost Mage")]
    public static void ApplyBlueFrostMage()
    {
        Material frostMageMaterial = GetOrCreateMaterial(
            FrostMageMaterialPath,
            Color.white,
            0f,
            0.28f);
        Texture2D mageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(MageTexturePath);
        SetMaterialTexture(frostMageMaterial, mageTexture);

        bool didApply = ReplaceEnemyVisual(
            FrostMagePrefabPath,
            MageBluePrefabPath,
            "Visual_PF_Enemy_LowPoly_FrostMage",
            1.2f,
            frostMageMaterial);

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
        Material frozenGolemMaterial = GetOrCreateMaterial(
            FrozenGolemMaterialPath,
            Color.white,
            0.08f,
            0.48f);
        Texture2D diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(CrystalGolemDiffusePath);
        Texture2D normalTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(CrystalGolemNormalPath);
        SetMaterialTexture(frozenGolemMaterial, diffuseTexture);
        SetMaterialNormalTexture(frozenGolemMaterial, normalTexture);
        AnimatorController idleController = GetOrCreateIdleAnimatorController(
            FrozenGolemControllerPath,
            CrystalGolemIdlePath);

        bool didApply = ReplaceEnemyVisual(
            FrozenGolemPrefabPath,
            CrystalGolemModelPath,
            "Visual_PF_Boss_FrozenGolem",
            1f,
            frozenGolemMaterial,
            2f,
            idleController);

        if (didApply == false)
        {
            return;
        }

        RenderAssetPreview(FrozenGolemPrefabPath, "Boss_FrozenGolem_LowPoly.png");
        Debug.Log("[CinderkeepEnemyAssetApplicator] Frozen Golem 저폴리 외형을 적용했습니다.");
    }

    private static bool ReplaceEnemyVisual(
        string enemyPrefabPath,
        string sourceVisualPath,
        string visualName,
        float localScale,
        Material overrideMaterial,
        float targetHeight = 0f,
        RuntimeAnimatorController animatorController = null)
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

            if (targetHeight > 0f)
            {
                FitVisualToHeight(visual, targetHeight);
                CenterVisualAtLocalPosition(
                    visual,
                    prefabRoot.transform,
                    new Vector3(0f, targetHeight * 0.5f, 0f));
            }

            DestroyCollidersInChildren(visual);

            if (overrideMaterial != null)
            {
                ApplyMaterialToRenderers(visual, overrideMaterial);
            }

            if (animatorController != null)
            {
                AssignAnimatorController(visual, animatorController);
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

    private static AnimatorController GetOrCreateIdleAnimatorController(
        string controllerPath,
        string animationAssetPath)
    {
        AnimationClip idleClip = FindAnimationClipCanBeNull(animationAssetPath, "Idle");
        if (idleClip == null)
        {
            Debug.LogError("[CinderkeepEnemyAssetApplicator] Frozen Golem Idle 클립을 찾지 못했습니다.");
            return null;
        }

        EnsureAssetFolder(EnemyAnimationFolder);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = null;
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state.name == "Idle")
            {
                idleState = states[i].state;
                break;
            }
        }

        if (idleState == null)
        {
            idleState = stateMachine.AddState("Idle");
        }

        idleState.motion = idleClip;
        stateMachine.defaultState = idleState;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip FindAnimationClipCanBeNull(string assetPath, string clipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip animationClip = assets[i] as AnimationClip;
            if (animationClip == null)
            {
                continue;
            }

            if (animationClip.name == clipName)
            {
                return animationClip;
            }
        }

        return null;
    }

    private static void AssignAnimatorController(
        GameObject visual,
        RuntimeAnimatorController animatorController)
    {
        Animator animator = visual.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("[CinderkeepEnemyAssetApplicator] Frozen Golem Animator를 찾지 못했습니다.");
            return;
        }

        animator.runtimeAnimatorController = animatorController;
        animator.applyRootMotion = false;
        EditorUtility.SetDirty(animator);
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

    private static void SetMaterialTexture(Material material, Texture texture)
    {
        if (material == null || texture == null)
        {
            return;
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }

        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
    }

    private static void SetMaterialNormalTexture(Material material, Texture texture)
    {
        if (material == null || texture == null)
        {
            return;
        }

        if (material.HasProperty("_BumpMap") == false)
        {
            return;
        }

        material.EnableKeyword("_NORMALMAP");
        material.SetTexture("_BumpMap", texture);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
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

        SampleAnimatorForPreview(previewObject);
        RenderGameObjectPreview(previewObject, GetPreviewOutputPath(fileName));
    }

    private static void SampleAnimatorForPreview(GameObject previewObject)
    {
        Animator animator = previewObject.GetComponentInChildren<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        AnimationClip[] animationClips = animator.runtimeAnimatorController.animationClips;
        if (animationClips.Length <= 0)
        {
            return;
        }

        AnimationClip idleClip = animationClips[0];
        Transform animatorTransform = animator.transform;
        Vector3 localPosition = animatorTransform.localPosition;
        Quaternion localRotation = animatorTransform.localRotation;
        Vector3 localScale = animatorTransform.localScale;
        idleClip.SampleAnimation(animator.gameObject, idleClip.length * 0.25f);
        animatorTransform.localPosition = localPosition;
        animatorTransform.localRotation = localRotation;
        animatorTransform.localScale = localScale;
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
