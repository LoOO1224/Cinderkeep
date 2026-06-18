using Cinderkeep.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// 4.00 게임 루프를 Cinderkeep_Game 씬에 연결하는 에디터 도구입니다.
// 팀원이 반복해서 수동 배치해야 하는 작업을 줄이기 위한 준비용 코드입니다.
public static class Cinderkeep_4_0_GameSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string SandboxRootPath = "Assets/_Sandbox/4_0_CandidateAssets";
    private const string SandboxPrefabPath = SandboxRootPath + "/Prefabs";
    private const string SandboxMaterialPath = SandboxRootPath + "/Materials";
    private const string SandboxModelPath = SandboxRootPath + "/Models";
    private const string SandboxReadMePath = SandboxRootPath + "/ReadMe";
    private const string MaterialTemplatePath = "Assets/Materials/Generated/FlameHeart_Core.mat";
    private const string CinderHeartTag = "CinderHeart";
    private const string PlayerTag = "Player";
    private const string EnemyTag = "Enemy";
    private const string BuildTag = "Build";

    private const string HeartModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/FX_Heart_01.fbx";
    private const string FlameModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/SM_Flame_FX.fbx";
    private const string CrystalModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/FX_Crystal_01.fbx";
    private const string RockModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/SM_Rock_01.fbx";
    private const string OreRockModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/SM_Rock_04.fbx";
    private const string TreeModelPath = "Assets/Arts/3DModels/Harvestables/SM_Resource_Tree.fbx";
    private const string StoneModelPath = "Assets/Arts/3DModels/Harvestables/SM_Resource_Stone.fbx";
    private const string AxePrefabPath = "Assets/Prefabs/Equipment/PF_Equipment_Axe.prefab";
    private const string MageBlackPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/Mages/MageBlack.prefab";
    private const string TowerModelPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Tower_A.fbx";
    private const string FireBowlPath = "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FireBowl/KB3D_AOE_PropFireBowlON_A.fbx";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF3.asset";

    [MenuItem("Cinderkeep/Main Game/Apply 4.00 GameLoop")]
    public static void ApplyGameLoop()
    {
        EnsureProjectFolders();
        EnsureTags();
        OpenTargetScene();

        Material cinderHeartMaterial = GetOrCreateMaterial("MAT_4_0_CinderHeart_Red", new Color(1f, 0.08f, 0.02f, 1f));
        Material iceMaterial = GetOrCreateMaterial("MAT_4_0_Ice_Blue", new Color(0.35f, 0.72f, 1f, 1f));
        Material woodMaterial = GetOrCreateMaterial("MAT_4_0_Wood", new Color(0.46f, 0.26f, 0.1f, 1f));
        Material stoneMaterial = GetOrCreateMaterial("MAT_4_0_Stone", new Color(0.42f, 0.46f, 0.5f, 1f));
        Material ironMaterial = GetOrCreateMaterial("MAT_4_0_IronOre", new Color(0.55f, 0.65f, 0.72f, 1f));
        Material goldMaterial = GetOrCreateMaterial("MAT_4_0_GoldOre", new Color(1f, 0.72f, 0.18f, 1f));
        Material enemyMaterial = GetOrCreateMaterial("MAT_4_0_IceZombie", new Color(0.55f, 0.8f, 0.95f, 1f));

        CreateSandboxPrefabs(cinderHeartMaterial, iceMaterial, woodMaterial, stoneMaterial, ironMaterial, goldMaterial, enemyMaterial);

        GameObject cinderHeart = SetupCinderHeart(cinderHeartMaterial);
        GameObject player = SetupPlayer();
        GameObject runtimeRoot = GetOrCreateRootObject("MainGame_RuntimeObjects");
        SetupResources(runtimeRoot.transform, woodMaterial, stoneMaterial, ironMaterial, goldMaterial);
        GameObject enemy = SetupEnemy(runtimeRoot.transform, enemyMaterial);
        SetupBuildPreviewObjects(runtimeRoot.transform, woodMaterial);
        SetupHud(player, cinderHeart, enemy);
        SetupGameLoopConnector(player, cinderHeart, enemy);
        SetupManagersForGameLoop();
        LogImportantRendererMaterials();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cinderkeep 4.00 game loop setup completed.");
    }

    [MenuItem("Cinderkeep/Main Game/Fix 4.00 Visual Only")]
    public static void FixVisualOnly()
    {
        EnsureProjectFolders();
        EnsureTags();
        OpenTargetScene();

        Material cinderHeartMaterial = GetOrCreateMaterial("MAT_4_0_CinderHeart_Red", new Color(1f, 0.08f, 0.02f, 1f));
        GetOrCreateMaterial("MAT_4_0_Ice_Blue", new Color(0.35f, 0.72f, 1f, 1f));
        GetOrCreateMaterial("MAT_4_0_Wood", new Color(0.46f, 0.26f, 0.1f, 1f));
        GetOrCreateMaterial("MAT_4_0_Stone", new Color(0.42f, 0.46f, 0.5f, 1f));
        GetOrCreateMaterial("MAT_4_0_IronOre", new Color(0.55f, 0.65f, 0.72f, 1f));
        GetOrCreateMaterial("MAT_4_0_GoldOre", new Color(1f, 0.72f, 0.18f, 1f));
        GetOrCreateMaterial("MAT_4_0_IceZombie", new Color(0.55f, 0.8f, 0.95f, 1f));
        GetOrCreateMaterial("MAT_4_0_PlayerHand", new Color(0.82f, 0.58f, 0.42f, 1f));
        GetOrCreateMaterial("MAT_4_0_Tool_DarkWood", new Color(0.38f, 0.22f, 0.1f, 1f));
        GetOrCreateMaterial("MAT_4_0_Tool_Metal", new Color(0.48f, 0.52f, 0.56f, 1f));

        SetupCinderHeart(cinderHeartMaterial);
        AdjustSceneLight();
        LogImportantRendererMaterials();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cinderkeep 4.00 visual only setup completed.");
    }

    private static void AdjustSceneLight()
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            Light[] lights = rootObjects[i].GetComponentsInChildren<Light>(true);
            AdjustLights(lights);
        }
    }

    private static void AdjustLights(Light[] lights)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light.type != LightType.Directional)
            {
                continue;
            }

            light.color = new Color(0.72f, 0.82f, 0.95f, 1f);
            light.intensity = 0.65f;
            EditorUtility.SetDirty(light);
        }
    }

    private static void EnsureProjectFolders()
    {
        EnsureFolder("Assets/_Sandbox");
        EnsureFolder(SandboxRootPath);
        EnsureFolder(SandboxPrefabPath);
        EnsureFolder(SandboxMaterialPath);
        EnsureFolder(SandboxModelPath);
        EnsureFolder(SandboxReadMePath);
        EnsureFolder("Assets/_TeamGuide");
        EnsureFolder("Assets/_TeamGuide/ReadMe");
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

    private static void EnsureTags()
    {
        EnsureTag(CinderHeartTag);
        EnsureTag(EnemyTag);
        EnsureTag(BuildTag);
    }

    private static void EnsureTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProperty.arraySize; i++)
        {
            SerializedProperty tagProperty = tagsProperty.GetArrayElementAtIndex(i);
            if (tagProperty.stringValue == tagName)
            {
                return;
            }
        }

        tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
        SerializedProperty newTagProperty = tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1);
        newTagProperty.stringValue = tagName;
        tagManager.ApplyModifiedProperties();
    }

    private static void OpenTargetScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == ScenePath)
        {
            return;
        }

        EditorSceneManager.OpenScene(ScenePath);
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        string materialPath = SandboxMaterialPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Shader shader = GetVisibleShader();
        bool useEmission = materialName == "MAT_4_0_CinderHeart_Red";

        if (material != null)
        {
            if (shader != null)
            {
                material.shader = shader;
            }

            ApplyMaterialColor(material, color, useEmission);
            EditorUtility.SetDirty(material);
            return material;
        }

        material = new Material(shader);
        material.name = materialName;
        ApplyMaterialColor(material, color, useEmission);
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static void ApplyMaterialColor(Material material, Color color, bool useEmission)
    {
        if (material == null)
        {
            return;
        }

        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            if (useEmission)
            {
                Color emissionColor = color * 0.35f;
                emissionColor.a = color.a;
                material.SetColor("_EmissionColor", emissionColor);
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    private static Shader GetVisibleShader()
    {
        Material templateMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialTemplatePath);
        if (templateMaterial != null && templateMaterial.shader != null)
        {
            return templateMaterial.shader;
        }

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Lit.shader");
        if (shader != null)
        {
            return shader;
        }

        shader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Unlit.shader");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null)
        {
            return shader;
        }

        shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader != null)
        {
            return shader;
        }

        return Shader.Find("Standard");
    }

    private static void CreateSandboxPrefabs(
        Material cinderHeartMaterial,
        Material iceMaterial,
        Material woodMaterial,
        Material stoneMaterial,
        Material ironMaterial,
        Material goldMaterial,
        Material enemyMaterial)
    {
        CreateCinderHeartPrefab(cinderHeartMaterial, iceMaterial);
        CreateResourcePrefab("PF_4_0_Resource_Tree", TreeModelPath, woodMaterial, new Vector3(1.2f, 1.2f, 1.2f));
        CreateResourcePrefab("PF_4_0_Resource_StonePickup", StoneModelPath, stoneMaterial, new Vector3(0.5f, 0.5f, 0.5f));
        CreateResourcePrefab("PF_4_0_Resource_Rock", RockModelPath, stoneMaterial, new Vector3(1.2f, 1.2f, 1.2f));
        CreateResourcePrefab("PF_4_0_Resource_IronOre", OreRockModelPath, ironMaterial, new Vector3(1.1f, 1.1f, 1.1f));
        CreateResourcePrefab("PF_4_0_Resource_GoldOre", OreRockModelPath, goldMaterial, new Vector3(1.1f, 1.1f, 1.1f));
        CreatePickaxePrefab(woodMaterial, ironMaterial);
        CreateWorkbenchPrefab(woodMaterial);
        CreateTurretPrefab(woodMaterial);
        CreateEnemyPrefab(enemyMaterial);
        CreateSecondPriorityPreviewPrefab("PF_4_0_Candidate_PlayerVisual_MageBlack", MageBlackPath);
        CreateSecondPriorityPreviewPrefab("PF_4_0_Candidate_RuinTower", TowerModelPath);
        CreateSecondPriorityPreviewPrefab("PF_4_0_Candidate_FireBowl", FireBowlPath);
    }

    private static void CreateCinderHeartPrefab(Material cinderHeartMaterial, Material iceMaterial)
    {
        string prefabPath = SandboxPrefabPath + "/PF_4_0_CinderHeart_Core.prefab";
        GameObject root = new GameObject("PF_4_0_CinderHeart_Core");

        AddModelChild(root.transform, "Heart", HeartModelPath, Vector3.zero, new Vector3(1.5f, 1.5f, 1.5f), cinderHeartMaterial);
        AddModelChild(root.transform, "Flame", FlameModelPath, new Vector3(0f, 1.1f, 0f), new Vector3(1.5f, 1.5f, 1.5f), cinderHeartMaterial);
        AddModelChild(root.transform, "IceCrystal", CrystalModelPath, new Vector3(0.8f, 0f, 0.3f), new Vector3(0.9f, 0.9f, 0.9f), iceMaterial);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreateResourcePrefab(string prefabName, string modelPath, Material material, Vector3 scale)
    {
        string prefabPath = SandboxPrefabPath + "/" + prefabName + ".prefab";
        GameObject root = new GameObject(prefabName);
        AddModelChild(root.transform, "Visual", modelPath, Vector3.zero, scale, material);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreatePickaxePrefab(Material handleMaterial, Material headMaterial)
    {
        string prefabPath = SandboxPrefabPath + "/PF_4_0_Equipment_Pickaxe.prefab";
        GameObject root = new GameObject("PF_4_0_Equipment_Pickaxe");

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Mesh_Handle";
        handle.transform.SetParent(root.transform);
        handle.transform.localPosition = Vector3.zero;
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
        handle.transform.localScale = new Vector3(0.12f, 1.2f, 0.12f);
        ApplyMaterial(handle, handleMaterial);
        RemoveCollider(handle);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Mesh_Head";
        head.transform.SetParent(root.transform);
        head.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        head.transform.localRotation = Quaternion.identity;
        head.transform.localScale = new Vector3(0.8f, 0.12f, 0.16f);
        ApplyMaterial(head, headMaterial);
        RemoveCollider(head);

        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreateWorkbenchPrefab(Material woodMaterial)
    {
        string prefabPath = SandboxPrefabPath + "/PF_4_0_Crafting_Workbench.prefab";
        GameObject root = new GameObject("PF_4_0_Crafting_Workbench");
        AddCubeChild(root.transform, "TableTop", new Vector3(0f, 0.8f, 0f), new Vector3(1.8f, 0.2f, 1f), woodMaterial);
        AddCubeChild(root.transform, "Leg_A", new Vector3(-0.7f, 0.35f, -0.35f), new Vector3(0.18f, 0.7f, 0.18f), woodMaterial);
        AddCubeChild(root.transform, "Leg_B", new Vector3(0.7f, 0.35f, -0.35f), new Vector3(0.18f, 0.7f, 0.18f), woodMaterial);
        AddCubeChild(root.transform, "Leg_C", new Vector3(-0.7f, 0.35f, 0.35f), new Vector3(0.18f, 0.7f, 0.18f), woodMaterial);
        AddCubeChild(root.transform, "Leg_D", new Vector3(0.7f, 0.35f, 0.35f), new Vector3(0.18f, 0.7f, 0.18f), woodMaterial);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreateTurretPrefab(Material woodMaterial)
    {
        string prefabPath = SandboxPrefabPath + "/PF_4_0_Building_WoodTurret.prefab";
        GameObject root = new GameObject("PF_4_0_Building_WoodTurret");
        AddCubeChild(root.transform, "Post", new Vector3(0f, 1f, 0f), new Vector3(0.35f, 2f, 0.35f), woodMaterial);
        AddCubeChild(root.transform, "Platform", new Vector3(0f, 2.1f, 0f), new Vector3(1.4f, 0.2f, 1.4f), woodMaterial);
        AddCubeChild(root.transform, "BowPreview", new Vector3(0f, 2.35f, 0.45f), new Vector3(0.15f, 0.7f, 0.15f), woodMaterial);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreateEnemyPrefab(Material enemyMaterial)
    {
        string prefabPath = SandboxPrefabPath + "/PF_4_0_Enemy_IceZombie.prefab";
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        root.name = "PF_4_0_Enemy_IceZombie";
        root.transform.localScale = new Vector3(1f, 1.2f, 1f);
        ApplyMaterial(root, enemyMaterial);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static void CreateSecondPriorityPreviewPrefab(string prefabName, string sourcePath)
    {
        string prefabPath = SandboxPrefabPath + "/" + prefabName + ".prefab";
        GameObject root = new GameObject(prefabName);
        AddModelChild(root.transform, "Preview", sourcePath, Vector3.zero, Vector3.one, null);
        SavePrefabAndDestroy(root, prefabPath);
    }

    private static GameObject SetupCinderHeart(Material cinderHeartMaterial)
    {
        GameObject cinderHeart = GetOrCreateRootObject("CinderHeart");
        cinderHeart.tag = CinderHeartTag;
        cinderHeart.transform.position = new Vector3(0f, 0.8f, 0f);
        cinderHeart.transform.rotation = Quaternion.identity;
        cinderHeart.transform.localScale = Vector3.one;
        RemoveVisualComponents(cinderHeart);
        EnsureComponent<CinderHeart>(cinderHeart);
        EnsureComponent<Damageable>(cinderHeart);

        ClearChildrenExcept(cinderHeart.transform, "Visual_CinderHeart_Core");
        Transform visualRoot = GetOrCreateChild(cinderHeart.transform, "Visual_CinderHeart_Core");
        ResetLocalTransform(visualRoot);
        ClearChildren(visualRoot);

        AddPrefabOrModelChild(visualRoot, "Model_CinderHeart_Heart", HeartModelPath, new Vector3(0f, 0.15f, 0f), new Vector3(1.4f, 1.4f, 1.4f), cinderHeartMaterial);
        AddCinderHeartFallbackShape(visualRoot, cinderHeartMaterial);

        Light light = EnsureComponent<Light>(cinderHeart);
        light.type = LightType.Point;
        light.color = new Color(1f, 0.45f, 0.25f, 1f);
        light.range = 9f;
        light.intensity = 2.2f;

        return cinderHeart;
    }

    private static GameObject SetupPlayer()
    {
        GameObject player = GetOrCreateRootObject("Player");
        player.tag = PlayerTag;
        player.transform.position = new Vector3(0f, 1.1f, -7f);
        player.transform.rotation = Quaternion.identity;

        CharacterController characterController = EnsureComponent<CharacterController>(player);
        characterController.height = 1.8f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 0.9f, 0f);

        PlayerMovement playerMovement = EnsureComponent<PlayerMovement>(player);
        PlayerJump playerJump = EnsureComponent<PlayerJump>(player);
        PlayerView playerView = EnsureComponent<PlayerView>(player);
        PlayerStatus playerStatus = EnsureComponent<PlayerStatus>(player);
        PlayerInteraction playerInteraction = EnsureComponent<PlayerInteraction>(player);
        PlayerBuild playerBuild = EnsureComponent<PlayerBuild>(player);
        PlayerToolController playerToolController = EnsureComponent<PlayerToolController>(player);
        PlayerAttack playerAttack = EnsureComponent<PlayerAttack>(player);
        Damageable damageable = EnsureComponent<Damageable>(player);

        Transform cameraRoot = GetOrCreateChild(player.transform, "Transform_CameraRoot_FirstPerson");
        cameraRoot.localPosition = new Vector3(0f, 1.55f, 0f);
        cameraRoot.localRotation = Quaternion.identity;

        GameObject cameraObject = GetOrCreateChild(cameraRoot, "Camera_FirstPerson").gameObject;
        Camera camera = EnsureComponent<Camera>(cameraObject);
        camera.tag = "MainCamera";
        camera.nearClipPlane = 0.02f;
        camera.fieldOfView = 68f;
        cameraObject.transform.localPosition = Vector3.zero;
        cameraObject.transform.localRotation = Quaternion.identity;
        EnsureComponent<AudioListener>(cameraObject);

        SetupFirstPersonTools(cameraObject.transform, playerToolController);

        SetObjectReference(playerJump, "PlayerMovement_PlayerMovement", playerMovement);
        SetObjectReference(playerView, "Transform_PlayerBody", player.transform);
        SetObjectReference(playerView, "Transform_Camera", cameraObject.transform);
        SetObjectReference(playerInteraction, "Transform_Camera", cameraObject.transform);
        SetInt(playerInteraction, "_interactionLayerMask", -1);
        SetObjectReference(playerAttack, "Transform_AttackOrigin", cameraObject.transform);
        SetObjectReference(playerAttack, "FirstPersonToolView_FirstPersonToolView", cameraObject.GetComponentInChildren<FirstPersonToolView>());

        EditorUtility.SetDirty(playerMovement);
        EditorUtility.SetDirty(playerStatus);
        EditorUtility.SetDirty(playerBuild);
        EditorUtility.SetDirty(damageable);
        return player;
    }

    private static void SetupFirstPersonTools(Transform cameraTransform, PlayerToolController playerToolController)
    {
        GameObject toolRootObject = GetOrCreateChild(cameraTransform, "FirstPerson_ToolView").gameObject;
        FirstPersonToolView toolView = EnsureComponent<FirstPersonToolView>(toolRootObject);
        toolRootObject.transform.localPosition = new Vector3(0.55f, -0.45f, 0.85f);
        toolRootObject.transform.localRotation = Quaternion.Euler(8f, -18f, 0f);
        Material handMaterial = GetOrCreateMaterial("MAT_4_0_PlayerHand", new Color(0.82f, 0.58f, 0.42f, 1f));
        Material toolMaterial = GetOrCreateMaterial("MAT_4_0_Tool_DarkWood", new Color(0.38f, 0.22f, 0.1f, 1f));
        Material metalMaterial = GetOrCreateMaterial("MAT_4_0_Tool_Metal", new Color(0.48f, 0.52f, 0.56f, 1f));

        GameObject axeView = GetOrCreateChild(toolRootObject.transform, "GameObject_AxeView").gameObject;
        ClearChildren(axeView.transform);
        AddPrefabOrModelChild(axeView.transform, "Axe_Model", AxePrefabPath, new Vector3(0f, 0f, 0f), new Vector3(0.18f, 0.18f, 0.18f), toolMaterial);
        AddAxeFallbackShape(axeView.transform, toolMaterial, metalMaterial);

        GameObject pickaxeView = GetOrCreateChild(toolRootObject.transform, "GameObject_PickaxeView").gameObject;
        ClearChildren(pickaxeView.transform);
        AddPrefabOrModelChild(pickaxeView.transform, "Pickaxe_Model", SandboxPrefabPath + "/PF_4_0_Equipment_Pickaxe.prefab", Vector3.zero, new Vector3(0.62f, 0.62f, 0.62f), metalMaterial);

        GameObject handView = GetOrCreateChild(toolRootObject.transform, "GameObject_HandView").gameObject;
        ClearChildren(handView.transform);
        AddCubeChild(handView.transform, "Cube_BlockHand", new Vector3(0f, -0.03f, 0.03f), new Vector3(0.3f, 0.3f, 0.62f), handMaterial);

        SetObjectReference(toolView, "PlayerToolController_PlayerToolController", playerToolController);
        SetObjectReference(toolView, "GameObject_AxeView", axeView);
        SetObjectReference(toolView, "GameObject_PickaxeView", pickaxeView);
        SetObjectReference(toolView, "GameObject_HandView", handView);
    }

    private static void SetupResources(Transform runtimeRoot, Material woodMaterial, Material stoneMaterial, Material ironMaterial, Material goldMaterial)
    {
        Transform resourceRoot = GetOrCreateChild(runtimeRoot, "Resources_GameLoop");
        ClearChildren(resourceRoot);

        CreateResourceNode(resourceRoot, "Resource_Tree_Axe_01", SandboxPrefabPath + "/PF_4_0_Resource_Tree.prefab", new Vector3(-30f, 0f, -30f), PlayerModel.ResourceWood, 3, GatherToolType.Axe, new Vector3(1.15f, 1.15f, 1.15f), woodMaterial);
        CreateResourceNode(resourceRoot, "Resource_Stone_Pickaxe_01", SandboxPrefabPath + "/PF_4_0_Resource_StonePickup.prefab", new Vector3(30f, 0.25f, -30f), PlayerModel.ResourceStone, 2, GatherToolType.Pickaxe, new Vector3(1.4f, 1.4f, 1.4f), stoneMaterial);
        CreateResourceNode(resourceRoot, "Resource_Rock_Pickaxe_02", SandboxPrefabPath + "/PF_4_0_Resource_Rock.prefab", new Vector3(34f, 0.35f, -26f), PlayerModel.ResourceStone, 4, GatherToolType.Pickaxe, new Vector3(1.25f, 1.25f, 1.25f), stoneMaterial);
        CreateResourceNode(resourceRoot, "Resource_IronOre_Pickaxe_01", SandboxPrefabPath + "/PF_4_0_Resource_IronOre.prefab", new Vector3(30f, 0.35f, 30f), PlayerModel.ResourceIron, 2, GatherToolType.Pickaxe, new Vector3(1.15f, 1.15f, 1.15f), ironMaterial);
        CreateResourceNode(resourceRoot, "Resource_GoldOre_Pickaxe_01", SandboxPrefabPath + "/PF_4_0_Resource_GoldOre.prefab", new Vector3(-30f, 0.35f, 30f), PlayerModel.ResourceGold, 1, GatherToolType.Pickaxe, new Vector3(1.15f, 1.15f, 1.15f), goldMaterial);
    }

    private static void AddAxeFallbackShape(Transform parent, Material handleMaterial, Material headMaterial)
    {
        Transform fallbackRoot = GetOrCreateChild(parent, "Fallback_AxeShape");
        ClearChildren(fallbackRoot);
        fallbackRoot.localPosition = new Vector3(0.05f, -0.03f, 0.04f);
        fallbackRoot.localRotation = Quaternion.Euler(0f, 0f, -32f);
        fallbackRoot.localScale = Vector3.one;

        AddCubeChild(fallbackRoot, "Cube_AxeHandle", new Vector3(0f, -0.14f, 0f), new Vector3(0.1f, 0.62f, 0.1f), handleMaterial);
        AddCubeChild(fallbackRoot, "Cube_AxeHead", new Vector3(0.17f, 0.16f, 0f), new Vector3(0.36f, 0.18f, 0.14f), headMaterial);
        AddCubeChild(fallbackRoot, "Cube_AxeBlade", new Vector3(0.31f, 0.08f, 0f), new Vector3(0.08f, 0.3f, 0.12f), headMaterial);
    }

    private static void AddCinderHeartFallbackShape(Transform parent, Material cinderHeartMaterial)
    {
        Transform fallbackRoot = GetOrCreateChild(parent, "Fallback_CinderHeartShape");
        ClearChildren(fallbackRoot);
        fallbackRoot.localPosition = new Vector3(0f, 0f, 0f);
        fallbackRoot.localRotation = Quaternion.identity;
        fallbackRoot.localScale = Vector3.one;

        AddSphereChild(fallbackRoot, "Sphere_HeartCore_Left", new Vector3(-0.35f, 0.25f, 0f), new Vector3(0.7f, 0.7f, 0.45f), cinderHeartMaterial);
        AddSphereChild(fallbackRoot, "Sphere_HeartCore_Right", new Vector3(0.35f, 0.25f, 0f), new Vector3(0.7f, 0.7f, 0.45f), cinderHeartMaterial);
        AddSphereChild(fallbackRoot, "Sphere_HeartPoint", new Vector3(0f, -0.32f, 0f), new Vector3(0.55f, 0.55f, 0.35f), cinderHeartMaterial);
    }

    private static void CreateResourceNode(
        Transform parent,
        string objectName,
        string prefabPath,
        Vector3 position,
        string resourceId,
        int amount,
        GatherToolType requiredToolType,
        Vector3 visualScale,
        Material fallbackMaterial)
    {
        GameObject resourceObject = new GameObject(objectName);
        resourceObject.transform.SetParent(parent);
        resourceObject.transform.position = position;
        resourceObject.transform.rotation = Quaternion.identity;

        AddPrefabOrModelChild(resourceObject.transform, "Candidate_Visual", prefabPath, Vector3.zero, visualScale, fallbackMaterial);

        SphereCollider collider = EnsureComponent<SphereCollider>(resourceObject);
        collider.radius = 1.25f;
        collider.center = new Vector3(0f, 0.6f, 0f);

        ResourceBase resourceBase = EnsureComponent<ResourceBase>(resourceObject);
        resourceBase.Initialize(resourceId, amount);

        ResourceNode resourceNode = EnsureComponent<ResourceNode>(resourceObject);
        SetString(resourceNode, "_resourceId", resourceId);
        SetInt(resourceNode, "_amount", amount);
        SetEnum(resourceNode, "_requiredToolType", (int)requiredToolType);
        SetBool(resourceNode, "_disableAfterGather", true);
        SetBool(resourceNode, "_canInteract", true);
    }

    private static GameObject SetupEnemy(Transform runtimeRoot, Material enemyMaterial)
    {
        Transform enemyRoot = GetOrCreateChild(runtimeRoot, "Enemies_GameLoop");
        ClearChildren(enemyRoot);

        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SandboxPrefabPath + "/PF_4_0_Enemy_IceZombie.prefab");
        GameObject enemy = InstantiatePrefabOrPrimitive(enemyPrefab, PrimitiveType.Capsule, enemyMaterial);
        enemy.name = "Enemy_IceZombie_01";
        enemy.tag = EnemyTag;
        enemy.transform.SetParent(enemyRoot);
        enemy.transform.position = new Vector3(0f, 1.1f, 36f);
        enemy.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        enemy.transform.localScale = new Vector3(1f, 1.2f, 1f);

        EnsureComponent<Damageable>(enemy);
        EnemyStatus enemyStatus = EnsureComponent<EnemyStatus>(enemy);
        EnemyDetector enemyDetector = EnsureComponent<EnemyDetector>(enemy);
        EnemyAttack enemyAttack = EnsureComponent<EnemyAttack>(enemy);
        EnemyMovement enemyMovement = EnsureComponent<EnemyMovement>(enemy);
        EnemyBrain enemyBrain = EnsureComponent<EnemyBrain>(enemy);
        NavMeshAgent navMeshAgent = EnsureComponent<NavMeshAgent>(enemy);
        navMeshAgent.speed = 1.8f;
        navMeshAgent.stoppingDistance = 1.3f;

        GameObject hudRoot = GetOrCreateChild(enemy.transform, "Canvas_EnemyHUD").gameObject;
        Canvas hudCanvas = EnsureComponent<Canvas>(hudRoot);
        hudCanvas.renderMode = RenderMode.WorldSpace;
        hudRoot.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        hudRoot.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);

        RectTransform hudRect = EnsureComponent<RectTransform>(hudRoot);
        hudRect.sizeDelta = new Vector2(160f, 36f);

        Image hpBack = CreateUiImage(hudRoot.transform, "Image_HpBack", new Color(0.05f, 0.05f, 0.06f, 0.9f));
        SetRect(hpBack.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(150f, 18f), Vector2.zero);

        Image hpFill = CreateUiImage(hpBack.transform, "Image_HpFill", new Color(0.82f, 0.08f, 0.08f, 1f));
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        SetRect(hpFill.rectTransform, new Vector2(0f, 0.5f), new Vector2(150f, 18f), Vector2.zero);
        hpFill.rectTransform.pivot = new Vector2(0f, 0.5f);

        Text hpText = CreateLegacyText(hudRoot.transform, "Text_Hp", "30 / 30", 14, Color.white);
        SetRect(hpText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(150f, 22f), Vector2.zero);

        EnemyHud enemyHud = EnsureComponent<EnemyHud>(hudRoot);
        SetObjectReference(enemyHud, "GameObject_HudRoot", hudRoot);
        SetObjectReference(enemyHud, "Image_HpFill", hpFill);
        SetObjectReference(enemyHud, "Text_Hp", hpText);

        SetObjectReference(enemyStatus, "EnemyHud_EnemyHud", enemyHud);
        SetObjectReference(enemyBrain, "EnemyDetector_EnemyDetector", enemyDetector);
        SetObjectReference(enemyBrain, "EnemyAttack_EnemyAttack", enemyAttack);
        SetObjectReference(enemyMovement, "EnemyDetector_EnemyDetector", enemyDetector);

        return enemy;
    }

    private static void SetupBuildPreviewObjects(Transform runtimeRoot, Material woodMaterial)
    {
        Transform buildRoot = GetOrCreateChild(runtimeRoot, "BuildingPreview_GameLoop");
        ClearChildren(buildRoot);

        GameObject workbenchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SandboxPrefabPath + "/PF_4_0_Crafting_Workbench.prefab");
        GameObject workbench = InstantiatePrefabOrPrimitive(workbenchPrefab, PrimitiveType.Cube, woodMaterial);
        workbench.name = "BuildPreview_CraftingWorkbench";
        workbench.tag = BuildTag;
        workbench.transform.SetParent(buildRoot);
        workbench.transform.position = new Vector3(4f, 0f, 4.5f);

        GameObject turretPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SandboxPrefabPath + "/PF_4_0_Building_WoodTurret.prefab");
        GameObject turret = InstantiatePrefabOrPrimitive(turretPrefab, PrimitiveType.Cube, woodMaterial);
        turret.name = "BuildPreview_WoodTurret";
        turret.tag = BuildTag;
        turret.transform.SetParent(buildRoot);
        turret.transform.position = new Vector3(-4.5f, 0f, 5.5f);
    }

    private static void SetupHud(GameObject player, GameObject cinderHeart, GameObject enemy)
    {
        RemoveLegacyGameObject("PlayerHUD");

        GameObject canvasObject = GetOrCreateRootObject("Canvas_GameHUD");
        Canvas canvas = EnsureComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        EnsureComponent<CanvasScaler>(canvasObject);
        EnsureComponent<GraphicRaycaster>(canvasObject);

        Transform hudRoot = GetOrCreateChild(canvasObject.transform, "Panel_HUDRoot");
        GameObject hudRootObject = hudRoot.gameObject;
        hudRootObject.SetActive(true);

        PlayerHUD playerHud = SetupPlayerHud(hudRoot, player.GetComponent<PlayerStatus>());
        ResourceUI resourceUi = SetupResourceUi(hudRoot);

        bool guideTextAlreadyExists = hudRoot.Find("Text_GameGuide") != null;
        GameObject guideTextObject = GetOrCreateChild(hudRoot, "Text_GameGuide").gameObject;
        TMP_Text guideText = EnsureComponent<TextMeshProUGUI>(guideTextObject);
        if (guideTextAlreadyExists == false)
        {
            guideText.text = "|  1:도끼  2:곡괭이  3:맨손  E:채집  좌클릭:공격  |";
            guideText.fontSize = 18f;
            guideText.alignment = TextAlignmentOptions.Left;
            ApplyTmpFont(guideText);
            SetRect(guideText.rectTransform, new Vector2(0f, 0f), new Vector2(760f, 40f), new Vector2(24f, 24f));
        }

        SetObjectReference(playerHud, "PlayerStatus_Target", player.GetComponent<PlayerStatus>());
        EditorUtility.SetDirty(resourceUi);
        EditorUtility.SetDirty(cinderHeart);
        EditorUtility.SetDirty(enemy);
    }

    private static PlayerHUD SetupPlayerHud(Transform hudRoot, PlayerStatus playerStatus)
    {
        bool playerHudAlreadyExists = hudRoot.Find("Panel_PlayerHUD") != null;
        GameObject playerHudObject = GetOrCreateChild(hudRoot, "Panel_PlayerHUD").gameObject;
        Image background = EnsureComponent<Image>(playerHudObject);
        if (playerHudAlreadyExists == false)
        {
            background.color = new Color(0.02f, 0.03f, 0.04f, 0.75f);
            SetRect(playerHudObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(380f, 96f), new Vector2(24f, -70f));
        }

        PlayerHUD playerHud = EnsureComponent<PlayerHUD>(playerHudObject);
        Slider healthSlider = CreateOrGetSlider(playerHudObject.transform, "Slider_Health", new Vector2(82f, -28f), new Color(0.8f, 0.08f, 0.08f, 1f), playerHudAlreadyExists);
        TMP_Text healthCurrent = CreateOrGetTmpText(playerHudObject.transform, "Text_HealthCurrent", "100", new Vector2(260f, -28f), playerHudAlreadyExists);
        TMP_Text healthMax = CreateOrGetTmpText(playerHudObject.transform, "Text_HealthMax", "100", new Vector2(306f, -28f), playerHudAlreadyExists);
        Slider staminaSlider = CreateOrGetSlider(playerHudObject.transform, "Slider_Stamina", new Vector2(82f, -66f), new Color(0.15f, 0.55f, 1f, 1f), playerHudAlreadyExists);
        TMP_Text staminaCurrent = CreateOrGetTmpText(playerHudObject.transform, "Text_StaminaCurrent", "150", new Vector2(260f, -66f), playerHudAlreadyExists);
        TMP_Text staminaMax = CreateOrGetTmpText(playerHudObject.transform, "Text_StaminaMax", "150", new Vector2(306f, -66f), playerHudAlreadyExists);

        TMP_Text hpLabel = CreateOrGetTmpText(playerHudObject.transform, "Text_HealthLabel", "HP", new Vector2(24f, -28f), playerHudAlreadyExists);
        TMP_Text staminaLabel = CreateOrGetTmpText(playerHudObject.transform, "Text_StaminaLabel", "ST", new Vector2(24f, -66f), playerHudAlreadyExists);
        if (playerHudAlreadyExists == false)
        {
            hpLabel.color = Color.white;
            staminaLabel.color = Color.white;
        }

        SetObjectReference(playerHud, "PlayerStatus_Target", playerStatus);
        SetObjectReference(playerHud, "Slider_Health", healthSlider);
        SetObjectReference(playerHud, "Text_HealthCurrent", healthCurrent);
        SetObjectReference(playerHud, "Text_HealthMax", healthMax);
        SetObjectReference(playerHud, "Slider_Stamina", staminaSlider);
        SetObjectReference(playerHud, "Text_StaminaCurrent", staminaCurrent);
        SetObjectReference(playerHud, "Text_StaminaMax", staminaMax);
        return playerHud;
    }

    private static ResourceUI SetupResourceUi(Transform hudRoot)
    {
        bool resourceUiAlreadyExists = hudRoot.Find("Panel_ResourceUI") != null;
        GameObject resourceUiObject = GetOrCreateChild(hudRoot, "Panel_ResourceUI").gameObject;
        Image background = EnsureComponent<Image>(resourceUiObject);
        if (resourceUiAlreadyExists == false)
        {
            background.color = new Color(0.02f, 0.03f, 0.04f, 0.72f);
            SetRect(resourceUiObject.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(360f, 180f), new Vector2(-24f, 24f));
        }

        ResourceUI resourceUi = EnsureComponent<ResourceUI>(resourceUiObject);
        TMP_Text woodText = CreateOrGetResourceRow(resourceUiObject.transform, "Wood", "Wood", 0, resourceUiAlreadyExists);
        TMP_Text stoneText = CreateOrGetResourceRow(resourceUiObject.transform, "Stone", "Stone", 1, resourceUiAlreadyExists);
        TMP_Text ironText = CreateOrGetResourceRow(resourceUiObject.transform, "Iron", "Iron", 2, resourceUiAlreadyExists);
        TMP_Text goldText = CreateOrGetResourceRow(resourceUiObject.transform, "Gold", "Gold", 3, resourceUiAlreadyExists);
        TMP_Text mithrilText = CreateOrGetResourceRow(resourceUiObject.transform, "Mithril", "Mithril", 4, resourceUiAlreadyExists);
        TMP_Text adamantiumText = CreateOrGetResourceRow(resourceUiObject.transform, "Adamantium", "Adamantium", 5, resourceUiAlreadyExists);

        SetObjectReference(resourceUi, "Text_Wood", woodText);
        SetObjectReference(resourceUi, "Text_Stone", stoneText);
        SetObjectReference(resourceUi, "Text_Iron", ironText);
        SetObjectReference(resourceUi, "Text_Gold", goldText);
        SetObjectReference(resourceUi, "Text_Mithril", mithrilText);
        SetObjectReference(resourceUi, "Text_Adamantium", adamantiumText);
        return resourceUi;
    }

    private static TMP_Text CreateOrGetResourceRow(Transform parent, string resourceName, string labelText, int rowIndex, bool preserveLayout)
    {
        TMP_Text label = CreateOrGetTmpText(parent, "Text_" + resourceName + "_Label", labelText, new Vector2(24f, -28f - rowIndex * 24f), preserveLayout);
        if (preserveLayout == false)
        {
            label.alignment = TextAlignmentOptions.Left;
            SetRect(label.rectTransform, new Vector2(0f, 1f), new Vector2(190f, 24f), new Vector2(24f, -28f - rowIndex * 24f));
        }

        TMP_Text amount = CreateOrGetTmpText(parent, "Text_" + resourceName, "0", new Vector2(276f, -28f - rowIndex * 24f), preserveLayout);
        if (preserveLayout == false)
        {
            amount.alignment = TextAlignmentOptions.Right;
            SetRect(amount.rectTransform, new Vector2(0f, 1f), new Vector2(60f, 24f), new Vector2(276f, -28f - rowIndex * 24f));
        }
        return amount;
    }

    private static void SetupGameLoopConnector(GameObject player, GameObject cinderHeart, GameObject enemy)
    {
        GameObject connectorObject = GetOrCreateRootObject("MainGame_LoopConnector");
        CinderkeepGameLoopConnector connector = EnsureComponent<CinderkeepGameLoopConnector>(connectorObject);

        GameObject managerRoot = GetOrCreateRootObject("MainGame_Managers");
        GameManager gameManager = GetComponentInChildrenByName<GameManager>(managerRoot.transform, "GameManager");
        GameDataManager gameDataManager = GetComponentInChildrenByName<GameDataManager>(managerRoot.transform, "GameDataManager");

        if (gameManager == null)
        {
            gameManager = EnsureComponent<GameManager>(GetOrCreateChild(managerRoot.transform, "GameManager").gameObject);
        }

        if (gameDataManager == null)
        {
            gameDataManager = EnsureComponent<GameDataManager>(GetOrCreateChild(managerRoot.transform, "GameDataManager").gameObject);
        }

        SetObjectReference(connector, "GameManager_GameManager", gameManager);
        SetObjectReference(connector, "GameDataManager_GameDataManager", gameDataManager);
        SetObjectReference(connector, "PlayerStatus_PlayerStatus", player.GetComponent<PlayerStatus>());
        SetObjectReference(connector, "PlayerHUD_PlayerHUD", GetSceneComponentByName<PlayerHUD>("Panel_PlayerHUD"));
        SetObjectReference(connector, "ResourceUI_ResourceUI", GetSceneComponentByName<ResourceUI>("Panel_ResourceUI"));
        SetObjectReference(connector, "Transform_CinderHeartTarget", cinderHeart.transform);
        SetObjectReference(connector, "Camera_GameCamera", player.GetComponentInChildren<Camera>());
        SetEnemyRuntimeSet(connector, enemy);
    }

    private static void SetupManagersForGameLoop()
    {
        GameObject managerRoot = GetOrCreateRootObject("MainGame_Managers");
        GameManager gameManager = EnsureComponent<GameManager>(GetOrCreateChild(managerRoot.transform, "GameManager").gameObject);
        GameDataManager gameDataManager = EnsureComponent<GameDataManager>(GetOrCreateChild(managerRoot.transform, "GameDataManager").gameObject);
        GameObjectManager gameObjectManager = EnsureComponent<GameObjectManager>(GetOrCreateChild(managerRoot.transform, "GameObjectManager").gameObject);
        UIManager uiManager = EnsureComponent<UIManager>(GetOrCreateChild(managerRoot.transform, "UIManager").gameObject);
        SoundManager soundManager = EnsureComponent<SoundManager>(GetOrCreateChild(managerRoot.transform, "SoundManager").gameObject);
        MapManager mapManager = EnsureComponent<MapManager>(GetOrCreateChild(managerRoot.transform, "MapManager").gameObject);

        Transform objectRoot = GetOrCreateChild(managerRoot.transform, "Transform_RuntimeObjectRoot");
        SetObjectReference(gameObjectManager, "Transform_ObjectRoot", objectRoot);

        SetObjectReference(gameManager, "GameDataManager_GameDataManager", gameDataManager);
        SetObjectReference(gameManager, "GameObjectManager_GameObjectManager", gameObjectManager);
        SetObjectReference(gameManager, "UIManager_UIManager", uiManager);
        SetObjectReference(gameManager, "SoundManager_SoundManager", soundManager);
        SetObjectReference(gameManager, "MapManager_MapManager", mapManager);

        GameObject hudRoot = GetSceneObjectByName("Panel_HUDRoot");
        SetObjectReference(uiManager, "GameObject_HudRoot", hudRoot);
    }

    private static void SetEnemyRuntimeSet(CinderkeepGameLoopConnector connector, GameObject enemy)
    {
        SerializedObject serializedObject = new SerializedObject(connector);
        SerializedProperty enemySets = serializedObject.FindProperty("_enemyRuntimeSets");
        enemySets.arraySize = 1;

        SerializedProperty enemySet = enemySets.GetArrayElementAtIndex(0);
        enemySet.FindPropertyRelative("_enemyDataId").stringValue = "ice_zombie";
        enemySet.FindPropertyRelative("EnemyStatus_EnemyStatus").objectReferenceValue = enemy.GetComponent<EnemyStatus>();
        enemySet.FindPropertyRelative("EnemyAttack_EnemyAttack").objectReferenceValue = enemy.GetComponent<EnemyAttack>();
        enemySet.FindPropertyRelative("EnemyDetector_EnemyDetector").objectReferenceValue = enemy.GetComponent<EnemyDetector>();
        enemySet.FindPropertyRelative("EnemyMovement_EnemyMovement").objectReferenceValue = enemy.GetComponent<EnemyMovement>();
        enemySet.FindPropertyRelative("EnemyHud_EnemyHud").objectReferenceValue = enemy.GetComponentInChildren<EnemyHud>();

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(connector);
    }

    private static Slider CreateOrGetSlider(Transform parent, string objectName, Vector2 anchoredPosition, Color fillColor, bool preserveLayout)
    {
        bool sliderAlreadyExists = parent.Find(objectName) != null;
        GameObject sliderObject = GetOrCreateChild(parent, objectName).gameObject;
        Slider slider = EnsureComponent<Slider>(sliderObject);
        if (preserveLayout == false || sliderAlreadyExists == false)
        {
            SetRect(sliderObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(170f, 16f), anchoredPosition);
        }

        Image background = EnsureComponent<Image>(sliderObject);
        if (preserveLayout == false || sliderAlreadyExists == false)
        {
            background.color = new Color(0.12f, 0.12f, 0.14f, 1f);
        }

        bool fillAlreadyExists = sliderObject.transform.Find("Image_Fill") != null;
        GameObject fillObject = GetOrCreateChild(sliderObject.transform, "Image_Fill").gameObject;
        Image fill = EnsureComponent<Image>(fillObject);
        if (preserveLayout == false || fillAlreadyExists == false)
        {
            fill.color = fillColor;
            SetRect(fill.rectTransform, new Vector2(0f, 0.5f), new Vector2(170f, 16f), Vector2.zero);
            fill.rectTransform.pivot = new Vector2(0f, 0.5f);
        }

        slider.targetGraphic = fill;
        slider.fillRect = fill.rectTransform;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private static TMP_Text CreateOrGetTmpText(Transform parent, string objectName, string text, Vector2 anchoredPosition, bool preserveLayout)
    {
        bool textAlreadyExists = parent.Find(objectName) != null;
        GameObject textObject = GetOrCreateChild(parent, objectName).gameObject;
        TMP_Text tmpText = EnsureComponent<TextMeshProUGUI>(textObject);
        if (preserveLayout == false || textAlreadyExists == false)
        {
            tmpText.text = text;
            tmpText.fontSize = 18f;
            tmpText.color = Color.white;
            tmpText.alignment = TextAlignmentOptions.Center;
            ApplyTmpFont(tmpText);
            SetRect(tmpText.rectTransform, new Vector2(0f, 1f), new Vector2(80f, 22f), anchoredPosition);
        }
        return tmpText;
    }

    private static Image CreateUiImage(Transform parent, string objectName, Color color)
    {
        GameObject imageObject = GetOrCreateChild(parent, objectName).gameObject;
        Image image = EnsureComponent<Image>(imageObject);
        image.color = color;
        return image;
    }

    private static Text CreateLegacyText(Transform parent, string objectName, string text, int fontSize, Color color)
    {
        GameObject textObject = GetOrCreateChild(parent, objectName).gameObject;
        Text textComponent = EnsureComponent<Text>(textObject);
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAnchor.MiddleCenter;
        return textComponent;
    }

    private static void ApplyTmpFont(TMP_Text text)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            return;
        }

        text.font = fontAsset;
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = anchor;
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
    }

    private static void AddModelChild(Transform parent, string childName, string modelPath, Vector3 localPosition, Vector3 localScale, Material material)
    {
        AddPrefabOrModelChild(parent, childName, modelPath, localPosition, localScale, material);
    }

    private static void AddPrefabOrModelChild(Transform parent, string childName, string assetPath, Vector3 localPosition, Vector3 localScale, Material material, bool applyMaterialToLoadedAsset = true)
    {
        GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        GameObject childObject;
        bool isFallbackObject = false;

        if (assetObject == null)
        {
            childObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            RemoveCollider(childObject);
            isFallbackObject = true;
        }
        else
        {
            childObject = PrefabUtility.InstantiatePrefab(assetObject) as GameObject;
        }

        if (childObject == null)
        {
            return;
        }

        childObject.name = childName;
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = localPosition;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = localScale;

        if (material != null && (isFallbackObject || applyMaterialToLoadedAsset))
        {
            ApplyMaterialToChildren(childObject, material);
        }

        RemoveCollidersInChildren(childObject);
    }

    private static GameObject InstantiatePrefabOrPrimitive(GameObject prefabObject, PrimitiveType fallbackPrimitiveType, Material fallbackMaterial)
    {
        if (prefabObject == null)
        {
            return CreateFallbackPrimitive(fallbackPrimitiveType, fallbackMaterial);
        }

        GameObject instanceObject = PrefabUtility.InstantiatePrefab(prefabObject) as GameObject;
        if (instanceObject != null)
        {
            return instanceObject;
        }

        return CreateFallbackPrimitive(fallbackPrimitiveType, fallbackMaterial);
    }

    private static GameObject CreateFallbackPrimitive(PrimitiveType primitiveType, Material material)
    {
        GameObject fallbackObject = GameObject.CreatePrimitive(primitiveType);
        ApplyMaterial(fallbackObject, material);
        return fallbackObject;
    }

    private static void AddCubeChild(Transform parent, string childName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = childName;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = localScale;
        ApplyMaterial(cube, material);
        RemoveCollider(cube);
    }

    private static void AddSphereChild(Transform parent, string childName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = childName;
        sphere.transform.SetParent(parent);
        sphere.transform.localPosition = localPosition;
        sphere.transform.localRotation = Quaternion.identity;
        sphere.transform.localScale = localScale;
        ApplyMaterial(sphere, material);
        RemoveCollider(sphere);
    }

    private static void ApplyMaterial(GameObject targetObject, Material material)
    {
        if (targetObject == null || material == null)
        {
            return;
        }

        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        renderer.sharedMaterial = material;
    }

    private static void ApplyMaterialToChildren(GameObject targetObject, Material material)
    {
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
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

    private static void RemoveCollidersInChildren(GameObject targetObject)
    {
        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    private static void RemoveVisualComponents(GameObject targetObject)
    {
        MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Object.DestroyImmediate(meshRenderer);
        }

        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Object.DestroyImmediate(meshFilter);
        }
    }

    private static void SavePrefabAndDestroy(GameObject root, string prefabPath)
    {
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static GameObject GetOrCreateRootObject(string objectName)
    {
        GameObject sceneObject = GetSceneObjectByName(objectName);
        if (sceneObject != null)
        {
            return sceneObject;
        }

        return new GameObject(objectName);
    }

    private static Transform GetOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject.transform;
    }

    private static GameObject GetSceneObjectByName(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject result = GetChildByName(rootObjects[i].transform, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static TComponent GetSceneComponentByName<TComponent>(string objectName)
        where TComponent : Component
    {
        GameObject sceneObject = GetSceneObjectByName(objectName);
        if (sceneObject == null)
        {
            return null;
        }

        return sceneObject.GetComponent<TComponent>();
    }

    private static TComponent GetComponentInChildrenByName<TComponent>(Transform rootTransform, string objectName)
        where TComponent : Component
    {
        if (rootTransform == null)
        {
            return null;
        }

        GameObject sceneObject = GetChildByName(rootTransform, objectName);
        if (sceneObject == null)
        {
            return null;
        }

        return sceneObject.GetComponent<TComponent>();
    }

    private static GameObject GetChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform.name == objectName)
        {
            return rootTransform.gameObject;
        }

        for (int i = 0; i < rootTransform.childCount; i++)
        {
            GameObject result = GetChildByName(rootTransform.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void ClearChildrenExcept(Transform parent, string keepChildName)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name == keepChildName)
            {
                continue;
            }

            Object.DestroyImmediate(child.gameObject);
        }
    }

    private static void ResetLocalTransform(Transform targetTransform)
    {
        targetTransform.localPosition = Vector3.zero;
        targetTransform.localRotation = Quaternion.identity;
        targetTransform.localScale = Vector3.one;
    }

    private static void LogImportantRendererMaterials()
    {
        LogRendererMaterialsInRoot("CinderHeart");
        LogRendererMaterialsInRoot("Player");
        LogRendererMaterialsInRoot("MainGame_RuntimeObjects");
    }

    private static void LogRendererMaterialsInRoot(string rootObjectName)
    {
        GameObject rootObject = GetSceneObjectByName(rootObjectName);
        if (rootObject == null)
        {
            Debug.LogWarning("4.00 material check skipped. Missing root object: " + rootObjectName);
            return;
        }

        Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            Material material = renderer.sharedMaterial;
            if (material == null)
            {
                Debug.LogWarning("4.00 material missing: " + GetHierarchyPath(renderer.transform));
                continue;
            }

            Shader shader = material.shader;
            if (shader == null)
            {
                Debug.LogWarning("4.00 shader missing: " + GetHierarchyPath(renderer.transform) + " / " + material.name);
                continue;
            }

            if (shader.name == "Hidden/InternalErrorShader")
            {
                Debug.LogWarning("4.00 shader error: " + GetHierarchyPath(renderer.transform) + " / " + material.name);
                continue;
            }

            Debug.Log("4.00 material ok: " + GetHierarchyPath(renderer.transform) + " / " + material.name + " / " + shader.name);
        }
    }

    private static string GetHierarchyPath(Transform targetTransform)
    {
        string path = targetTransform.name;
        Transform parent = targetTransform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private static void RemoveLegacyGameObject(string objectName)
    {
        GameObject legacyObject = GetSceneObjectByName(objectName);
        if (legacyObject == null)
        {
            return;
        }

        Object.DestroyImmediate(legacyObject);
    }

    private static TComponent EnsureComponent<TComponent>(GameObject targetObject)
        where TComponent : Component
    {
        TComponent component = targetObject.GetComponent<TComponent>();
        if (component != null)
        {
            return component;
        }

        return targetObject.AddComponent<TComponent>();
    }

    private static void SetObjectReference(Object targetObject, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetObject);
    }

    private static void SetString(Object targetObject, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.stringValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetObject);
    }

    private static void SetInt(Object targetObject, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.intValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetObject);
    }

    private static void SetBool(Object targetObject, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.boolValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetObject);
    }

    private static void SetEnum(Object targetObject, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.enumValueIndex = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(targetObject);
    }
}
