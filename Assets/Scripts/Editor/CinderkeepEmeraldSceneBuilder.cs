using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CinderkeepEmeraldSceneBuilder
{
    private const string BaseScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string EmeraldSceneFolderPath = "Assets/Scenes/EmeraldQuest";
    private const string EmeraldScenePath = "Assets/Scenes/EmeraldQuest/Cinderkeep_EmeraldQuest.unity";
    private const string PrefabFolderPath = "Assets/Prefabs/Emerald";
    private const string MaterialFolderPath = "Assets/Materials/Generated";
    private const string AddressableGroupName = "Cinderkeep_Emerald";
    private const string AddressableLabel = "emerald_quest";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF3.asset";
    private const string EnemyPrefabPath = "Assets/Prefabs/Enemy/PF_Enemy_LowPoly_FrostMage.prefab";

    [MenuItem("Cinderkeep/Emerald/Apply Platinum To Emerald")]
    public static void ApplyPlatinumToEmerald()
    {
        EnsureFolders();
        OpenTargetScene(BaseScenePath);
        ApplyToOpenedScene();
    }

    [MenuItem("Cinderkeep/Emerald/Create Emerald Quest Scene")]
    public static void CreateEmeraldQuestScene()
    {
        EnsureFolders();
        CopyBaseSceneToEmeraldScene();
        OpenTargetScene(EmeraldScenePath);
        ApplyToOpenedScene();
        AddSceneToBuildSettings(EmeraldScenePath);

        Debug.Log("Cinderkeep Emerald quest scene created: " + EmeraldScenePath);
    }

    private static void ApplyToOpenedScene()
    {
        EnsureFolders();

        Material pickupMaterial = GetOrCreateMaterial("MAT_Emerald_HealingPickup", new Color(0.16f, 0.9f, 0.58f, 1f));
        Material zoneMaterial = GetOrCreateMaterial("MAT_Emerald_DamageZone", new Color(0.9f, 0.16f, 0.18f, 0.45f));
        Material enemyMaterial = GetOrCreateMaterial("MAT_Emerald_CombatTarget", new Color(0.2f, 0.92f, 0.78f, 1f));

        GameObject healingPickupPrefab = CreateHealingPickupPrefab(pickupMaterial);
        GameObject damageZonePrefab = CreateDamageZonePrefab(zoneMaterial);

        GameObject player = GetSceneObjectByName("Player");
        GameObject canvasObject = GetSceneObjectByName("Canvas_GameHUD");
        GameObject runtimeRoot = GetOrCreateRootObject("MainGame_RuntimeObjects");

        if (player == null || canvasObject == null)
        {
            Debug.LogWarning("CinderkeepEmeraldSceneBuilder: Player or Canvas_GameHUD was not found. Run the main game loop setup first.");
            return;
        }

        SetupPlayer(player);
        GameObject combatTarget = SetupCombatTarget(runtimeRoot.transform, player.transform, enemyMaterial);
        SetupPickup(runtimeRoot.transform, player.transform, healingPickupPrefab);
        SetupDamageZone(runtimeRoot.transform, player.transform, damageZonePrefab);
        SetupEnvironment(runtimeRoot.transform, player.transform);
        SetupChecklistUI(canvasObject.transform);
        SetupAddressables(healingPickupPrefab, damageZonePrefab, combatTarget);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Cinderkeep Emerald quest setup completed.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder(EmeraldSceneFolderPath);
        EnsureFolder(PrefabFolderPath);
        EnsureFolder(MaterialFolderPath);
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath) == true)
        {
            return;
        }

        string parentPath = System.IO.Path.GetDirectoryName(folderPath);
        string folderName = System.IO.Path.GetFileName(folderPath);
        parentPath = parentPath.Replace("\\", "/");
        AssetDatabase.CreateFolder(parentPath, folderName);
    }

    private static void CopyBaseSceneToEmeraldScene()
    {
        SceneAsset baseSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(BaseScenePath);
        if (baseSceneAsset == null)
        {
            Debug.LogWarning("CinderkeepEmeraldSceneBuilder: base scene was not found: " + BaseScenePath);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(EmeraldScenePath) != null)
        {
            AssetDatabase.DeleteAsset(EmeraldScenePath);
        }

        bool isCopied = AssetDatabase.CopyAsset(BaseScenePath, EmeraldScenePath);
        if (isCopied == false)
        {
            Debug.LogWarning("CinderkeepEmeraldSceneBuilder: emerald scene copy failed: " + EmeraldScenePath);
            return;
        }

        AssetDatabase.ImportAsset(EmeraldScenePath);
    }

    private static void OpenTargetScene(string scenePath)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path == scenePath)
        {
            return;
        }

        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < buildScenes.Count; i++)
        {
            if (buildScenes[i].path != scenePath)
            {
                continue;
            }

            buildScenes[i].enabled = true;
            EditorBuildSettings.scenes = buildScenes.ToArray();
            return;
        }

        buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        string materialPath = MaterialFolderPath + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            material.name = materialName;
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }

        ApplyMaterialColor(material, color);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ApplyMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        material.color = color;
        if (material.HasProperty("_BaseColor") == true)
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color") == true)
        {
            material.SetColor("_Color", color);
        }
    }

    private static GameObject CreateHealingPickupPrefab(Material material)
    {
        string prefabPath = PrefabFolderPath + "/PF_Emerald_HealingPickup.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        root.name = "PF_Emerald_HealingPickup";
        root.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        ApplyMaterial(root, material);

        SphereCollider sphereCollider = root.GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;

        Rigidbody rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        PlayerHealingPickup healingPickup = root.AddComponent<PlayerHealingPickup>();
        SetFloat(healingPickup, "_healAmount", 35f);
        SetBool(healingPickup, "_disableAfterPickup", true);

        prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateDamageZonePrefab(Material material)
    {
        string prefabPath = PrefabFolderPath + "/PF_Emerald_DamageZone.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        root.name = "PF_Emerald_DamageZone";
        root.transform.localScale = new Vector3(2.4f, 0.08f, 2.4f);
        ApplyMaterial(root, material);

        Collider collider = root.GetComponent<Collider>();
        collider.isTrigger = true;

        Rigidbody rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        AreaStatusEffectZone zone = root.AddComponent<AreaStatusEffectZone>();
        SetEnum(zone, "_effectType", 0);
        SetFloat(zone, "_effectPerSecond", 8f);
        SetFloat(zone, "_tickInterval", 0.5f);

        prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void SetupPlayer(GameObject player)
    {
        EnsureComponent<PlayerStatus>(player);
        EnsureComponent<Damageable>(player);
        PlayerAttack playerAttack = EnsureComponent<PlayerAttack>(player);

        SetFloat(playerAttack, "_attackDamage", 18f);
        SetFloat(playerAttack, "_attackDistance", 3.2f);
        SetFloat(playerAttack, "_attackRadius", 0.55f);
        SetFloat(playerAttack, "_attackInterval", 0.45f);
    }

    private static GameObject SetupCombatTarget(Transform runtimeRoot, Transform playerTransform, Material enemyMaterial)
    {
        GameObject combatTarget = GetSceneObjectByName("EmeraldQuest_CombatTarget");
        if (combatTarget == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (prefab != null)
            {
                combatTarget = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }

            if (combatTarget == null)
            {
                combatTarget = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            }

            combatTarget.name = "EmeraldQuest_CombatTarget";
            combatTarget.transform.SetParent(runtimeRoot);
        }

        Vector3 forward = playerTransform.forward;
        if (forward.sqrMagnitude <= 0.01f)
        {
            forward = Vector3.forward;
        }

        combatTarget.transform.position = playerTransform.position + forward.normalized * 2.4f + Vector3.up * 0.2f;
        combatTarget.transform.rotation = Quaternion.LookRotation(-forward.normalized, Vector3.up);
        combatTarget.transform.localScale = Vector3.one;

        ApplyMaterialToChildren(combatTarget, enemyMaterial);
        CapsuleCollider capsuleCollider = EnsureComponent<CapsuleCollider>(combatTarget);
        capsuleCollider.isTrigger = false;
        capsuleCollider.center = new Vector3(0f, 1f, 0f);
        capsuleCollider.height = 2f;
        capsuleCollider.radius = 0.45f;

        EnemyStatus enemyStatus = EnsureComponent<EnemyStatus>(combatTarget);
        Damageable damageable = EnsureComponent<Damageable>(combatTarget);
        EnsureComponent<EnemyAttack>(combatTarget);

        SetFloat(enemyStatus, "_maxHealth", 54f);
        SetBool(enemyStatus, "_deactivateOnDeath", true);
        SetFloat(damageable, "_maxHealth", 54f);

        EditorUtility.SetDirty(combatTarget);
        return combatTarget;
    }

    private static void SetupPickup(Transform runtimeRoot, Transform playerTransform, GameObject pickupPrefab)
    {
        GameObject pickupObject = GetOrCreatePrefabSceneObject("EmeraldQuest_HealingPickup", pickupPrefab, runtimeRoot);
        pickupObject.transform.position = playerTransform.position + playerTransform.right * 2f + Vector3.up * 1f;
        pickupObject.transform.rotation = Quaternion.identity;
        pickupObject.transform.localScale = Vector3.one;

        PlayerHealingPickup pickup = EnsureComponent<PlayerHealingPickup>(pickupObject);
        SetFloat(pickup, "_healAmount", 35f);
        SetBool(pickup, "_disableAfterPickup", true);
        pickupObject.SetActive(true);
        EditorUtility.SetDirty(pickupObject);
    }

    private static void SetupDamageZone(Transform runtimeRoot, Transform playerTransform, GameObject zonePrefab)
    {
        GameObject zoneObject = GetOrCreatePrefabSceneObject("EmeraldQuest_DamageZone", zonePrefab, runtimeRoot);
        zoneObject.transform.position = playerTransform.position - playerTransform.right * 2.8f;
        zoneObject.transform.rotation = Quaternion.identity;
        zoneObject.transform.localScale = Vector3.one;

        AreaStatusEffectZone zone = EnsureComponent<AreaStatusEffectZone>(zoneObject);
        SetEnum(zone, "_effectType", 0);
        SetFloat(zone, "_effectPerSecond", 8f);
        SetFloat(zone, "_tickInterval", 0.5f);
        EditorUtility.SetDirty(zoneObject);
    }

    private static void SetupEnvironment(Transform runtimeRoot, Transform playerTransform)
    {
        GameObject environmentRoot = GetOrCreateChildObject(runtimeRoot, "EmeraldQuest_AssetSet");
        ClearChildren(environmentRoot.transform);

        Material fallbackMaterial = GetOrCreateMaterial("MAT_Emerald_EnvironmentFallback", new Color(0.42f, 0.72f, 0.78f, 1f));
        Material ruinMaterial = GetOrCreateMaterial("MAT_Emerald_RuinTint", new Color(0.24f, 0.32f, 0.72f, 1f));
        Material crystalMaterial = GetOrCreateMaterial("MAT_Emerald_CrystalTint", new Color(0.12f, 0.9f, 1f, 1f));
        Material fireMaterial = GetOrCreateMaterial("MAT_Emerald_FireTint", new Color(1f, 0.46f, 0.12f, 1f));
        Vector3 origin = playerTransform.position;
        Vector3 forward = playerTransform.forward;
        if (forward.sqrMagnitude <= 0.01f)
        {
            forward = Vector3.forward;
        }

        forward = forward.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Gate_A.fbx", "EmeraldAsset_RuinGate", GetOffsetPosition(origin, right, forward, 0f, 5.7f, 0f), Quaternion.LookRotation(-forward, Vector3.up), Vector3.one * 0.72f, ruinMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Tower_A.fbx", "EmeraldAsset_RuinTowerLeft", GetOffsetPosition(origin, right, forward, -6.25f, 5.15f, 0f), Quaternion.LookRotation(-forward, Vector3.up) * Quaternion.Euler(0f, 28f, 0f), Vector3.one * 0.7f, ruinMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Tower_E.fbx", "EmeraldAsset_RuinTowerRight", GetOffsetPosition(origin, right, forward, 6.25f, 5.25f, 0f), Quaternion.LookRotation(-forward, Vector3.up) * Quaternion.Euler(0f, -28f, 0f), Vector3.one * 0.68f, ruinMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Statue_A.fbx", "EmeraldAsset_RuinStatue", GetOffsetPosition(origin, right, forward, -3.35f, -3.15f, 0f), Quaternion.LookRotation(forward, Vector3.up), Vector3.one * 0.55f, ruinMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Torch_A.fbx", "EmeraldAsset_RuinTorchLeft", GetOffsetPosition(origin, right, forward, -3.15f, 2.6f, 0f), Quaternion.identity, Vector3.one * 0.68f, fireMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FrozenRuins/KB3D_DKF_Torch_A.fbx", "EmeraldAsset_RuinTorchRight", GetOffsetPosition(origin, right, forward, 3.15f, 2.6f, 0f), Quaternion.identity, Vector3.one * 0.68f, fireMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/FX_Crystal_01.fbx", "EmeraldAsset_CrystalLeft", GetOffsetPosition(origin, right, forward, -5.1f, -1.1f, 0f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 1.25f, crystalMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/PolygonIce/FX_Crystal_04.fbx", "EmeraldAsset_CrystalRight", GetOffsetPosition(origin, right, forward, 5.1f, 0.15f, 0f), Quaternion.Euler(0f, -24f, 0f), Vector3.one * 1.15f, crystalMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/FireBowl/KB3D_AOE_PropFireBowlON_A.fbx", "EmeraldAsset_FireBowl", GetOffsetPosition(origin, right, forward, 0f, -3.2f, 0f), Quaternion.identity, Vector3.one * 1.05f, fireMaterial, true);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/CarnivorousPlant/Prefabs/Carnivorous Plant-Green.prefab", "EmeraldAsset_CarnivorousGreen", GetOffsetPosition(origin, right, forward, 5.4f, 2.1f, 0f), Quaternion.Euler(0f, -45f, 0f), Vector3.one * 0.68f, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/CarnivorousPlant/Prefabs/Carnivorous Plant-Yellow.prefab", "EmeraldAsset_CarnivorousYellow", GetOffsetPosition(origin, right, forward, -5.4f, 1.7f, 0f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.66f, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/_Sandbox/4_0_CandidateAssets/Prefabs/PF_4_0_Resource_Tree_Cinderwood.prefab", "EmeraldAsset_CinderwoodTree", GetOffsetPosition(origin, right, forward, -5.7f, -0.2f, 0f), Quaternion.Euler(0f, 18f, 0f), Vector3.one, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/_Sandbox/4_0_CandidateAssets/Prefabs/PF_4_0_Resource_Tree_Heartwood.prefab", "EmeraldAsset_HeartwoodTree", GetOffsetPosition(origin, right, forward, 5.7f, -0.95f, 0f), Quaternion.Euler(0f, -18f, 0f), Vector3.one, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/_Sandbox/4_0_CandidateAssets/Prefabs/PF_4_0_Resource_Rock_GoldOre.prefab", "EmeraldAsset_GoldOre", GetOffsetPosition(origin, right, forward, -1.9f, -3.3f, 0f), Quaternion.Euler(0f, 28f, 0f), Vector3.one, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/_Sandbox/4_0_CandidateAssets/Prefabs/PF_4_0_Resource_Rock_IronOre.prefab", "EmeraldAsset_IronOre", GetOffsetPosition(origin, right, forward, 2f, -3.2f, 0f), Quaternion.Euler(0f, -34f, 0f), Vector3.one, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/Mages/MageBlue.prefab", "EmeraldAsset_BackgroundMageBlue", GetOffsetPosition(origin, right, forward, -3.85f, 1.15f, 0f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * 0.58f, fallbackMaterial);
        AddEnvironmentAsset(environmentRoot.transform, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets/Mages/MageOrange.prefab", "EmeraldAsset_BackgroundMageOrange", GetOffsetPosition(origin, right, forward, 3.85f, 1.25f, 0f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * 0.58f, fallbackMaterial);

        EditorUtility.SetDirty(environmentRoot);
    }

    private static Vector3 GetOffsetPosition(Vector3 origin, Vector3 right, Vector3 forward, float rightOffset, float forwardOffset, float upOffset)
    {
        return origin + right * rightOffset + forward * forwardOffset + Vector3.up * upOffset;
    }

    private static void AddEnvironmentAsset(
        Transform parent,
        string assetPath,
        string objectName,
        Vector3 position,
        Quaternion rotation,
        Vector3 scale,
        Material fallbackMaterial,
        bool shouldApplyMaterial = false)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null)
        {
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (instance == null)
        {
            return;
        }

        instance.name = objectName;
        instance.transform.SetParent(parent);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = scale;
        if (shouldApplyMaterial == true)
        {
            ApplyMaterialToChildren(instance, fallbackMaterial);
            return;
        }

        EnsureRenderableMaterials(instance, fallbackMaterial);
    }

    private static GameObject GetOrCreatePrefabSceneObject(string objectName, GameObject prefab, Transform parent)
    {
        GameObject sceneObject = GetSceneObjectByName(objectName);
        if (sceneObject != null)
        {
            return sceneObject;
        }

        if (prefab != null)
        {
            sceneObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        if (sceneObject == null)
        {
            sceneObject = new GameObject(objectName);
        }

        sceneObject.name = objectName;
        sceneObject.transform.SetParent(parent);
        return sceneObject;
    }

    private static void SetupChecklistUI(Transform canvasTransform)
    {
        bool isPanelCreated;
        RectTransform panel = GetOrCreateRectTransform(canvasTransform, "Panel_EmeraldQuestChecklist", out isPanelCreated);
        if (isPanelCreated == true)
        {
            panel.anchorMin = new Vector2(1f, 1f);
            panel.anchorMax = new Vector2(1f, 1f);
            panel.pivot = new Vector2(1f, 1f);
            panel.anchoredPosition = new Vector2(-18f, -18f);
            panel.sizeDelta = new Vector2(390f, 298f);
            panel.localScale = Vector3.one;
        }

        Image panelImage = EnsureComponent<Image>(panel.gameObject);
        panelImage.color = new Color(0.02f, 0.04f, 0.05f, 0.82f);
        panelImage.raycastTarget = false;

        TMP_Text titleText = CreateOrGetText(panel, "Text_EmeraldQuestTitle", "Emerald Quest Check", 20f);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(-28f, 28f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.66f, 1f, 0.82f, 1f);

        TMP_Text[] objectiveTexts = new TMP_Text[9];
        for (int i = 0; i < objectiveTexts.Length; i++)
        {
            TMP_Text rowText = CreateOrGetText(panel, "Text_EmeraldObjective_" + (i + 1).ToString("00"), "", 15f);
            RectTransform rowRect = rowText.rectTransform;
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, -48f - (i * 26f));
            rowRect.sizeDelta = new Vector2(-28f, 24f);
            rowText.alignment = TextAlignmentOptions.Left;
            rowText.textWrappingMode = TextWrappingModes.NoWrap;
            rowText.overflowMode = TextOverflowModes.Ellipsis;
            objectiveTexts[i] = rowText;
        }

        EmeraldQuestChecklistUI checklistUI = EnsureComponent<EmeraldQuestChecklistUI>(panel.gameObject);
        SerializedObject serializedObject = new SerializedObject(checklistUI);
        SerializedProperty objectiveTextProperty = serializedObject.FindProperty("_objectiveTexts");
        objectiveTextProperty.arraySize = objectiveTexts.Length;
        for (int i = 0; i < objectiveTexts.Length; i++)
        {
            objectiveTextProperty.GetArrayElementAtIndex(i).objectReferenceValue = objectiveTexts[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(panel.gameObject);
    }

    private static TMP_Text CreateOrGetText(Transform parent, string objectName, string text, float fontSize)
    {
        bool isCreated;
        RectTransform textTransform = GetOrCreateRectTransform(parent, objectName, out isCreated);
        TextMeshProUGUI textComponent = EnsureComponent<TextMeshProUGUI>(textTransform.gameObject);
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.raycastTarget = false;
        ApplyFont(textComponent);
        return textComponent;
    }

    private static void ApplyFont(TMP_Text text)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            return;
        }

        text.font = fontAsset;
    }

    private static void SetupAddressables(GameObject healingPickupPrefab, GameObject damageZonePrefab, GameObject combatTarget)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        if (settings == null)
        {
            Debug.LogWarning("CinderkeepEmeraldSceneBuilder: Addressable settings could not be created.");
            return;
        }

        settings.AddLabel(AddressableLabel);
        AddressableAssetGroup group = GetOrCreateAddressableGroup(settings);

        RegisterAddressableAssetsInFolder(settings, group, "Assets/Prefabs/Enemy");
        RegisterAddressableAssetsInFolder(settings, group, "Assets/Prefabs/Map");
        RegisterAddressableAssetsInFolder(settings, group, "Assets/_Sandbox/4_0_CandidateAssets/Prefabs");
        RegisterAddressableAssetsInFolder(settings, group, "Assets/ThirdParty/AssetStore/Free/CinderkeepExternalAssets");
        RegisterAddressableAssetsInFolder(settings, group, "Assets/Audio");
        RegisterAddressableAsset(settings, group, AssetDatabase.GetAssetPath(healingPickupPrefab), "PF_Emerald_HealingPickup");
        RegisterAddressableAsset(settings, group, AssetDatabase.GetAssetPath(damageZonePrefab), "PF_Emerald_DamageZone");
        RegisterAddressableAsset(settings, group, EnemyPrefabPath, "PF_Emerald_CombatTargetSource");
        RegisterAddressableAsset(settings, group, "Assets/Prefabs/Map/PF_Map_GameMapGroup.prefab", "PF_Map_GameMapGroup");

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true);
        AssetDatabase.SaveAssets();
    }

    private static AddressableAssetGroup GetOrCreateAddressableGroup(AddressableAssetSettings settings)
    {
        AddressableAssetGroup group = settings.FindGroup(AddressableGroupName);
        if (group != null)
        {
            return group;
        }

        return settings.CreateGroup(
            AddressableGroupName,
            false,
            false,
            true,
            null,
            typeof(BundledAssetGroupSchema),
            typeof(ContentUpdateGroupSchema));
    }

    private static void RegisterAddressableAssetsInFolder(AddressableAssetSettings settings, AddressableAssetGroup group, string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath) == false)
        {
            return;
        }

        string[] filters = { "t:Prefab", "t:Material", "t:AudioClip", "t:Texture2D" };
        HashSet<string> registeredGuids = new HashSet<string>();
        for (int filterIndex = 0; filterIndex < filters.Length; filterIndex++)
        {
            string[] guids = AssetDatabase.FindAssets(filters[filterIndex], new[] { folderPath });
            for (int i = 0; i < guids.Length; i++)
            {
                if (registeredGuids.Add(guids[i]) == false)
                {
                    continue;
                }

                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string address = GetAddressFromAssetPath(assetPath);
                RegisterAddressableAsset(settings, group, assetPath, address);
            }
        }
    }

    private static string GetAddressFromAssetPath(string assetPath)
    {
        string address = assetPath.Replace("\\", "/");
        if (address.StartsWith("Assets/") == true)
        {
            address = address.Substring("Assets/".Length);
        }

        string extension = System.IO.Path.GetExtension(address);
        if (string.IsNullOrEmpty(extension) == false)
        {
            address = address.Substring(0, address.Length - extension.Length);
        }

        address = address.Replace(" ", "_");
        return "Emerald/" + address;
    }

    private static void RegisterAddressableAsset(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string address)
    {
        if (settings == null || group == null || string.IsNullOrEmpty(assetPath))
        {
            return;
        }

        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrEmpty(guid))
        {
            return;
        }

        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
        if (entry == null)
        {
            return;
        }

        entry.address = address;
        entry.SetLabel(AddressableLabel, true, true);
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

    private static GameObject GetOrCreateChildObject(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject childObject = new GameObject(objectName);
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
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

    private static RectTransform GetOrCreateRectTransform(Transform parent, string objectName, out bool isCreated)
    {
        Transform child = parent.Find(objectName);
        if (child != null)
        {
            isCreated = false;
            RectTransform existingRectTransform = child.GetComponent<RectTransform>();
            if (existingRectTransform != null)
            {
                return existingRectTransform;
            }

            return child.gameObject.AddComponent<RectTransform>();
        }

        GameObject childObject = new GameObject(objectName);
        childObject.transform.SetParent(parent);
        RectTransform rectTransform = childObject.AddComponent<RectTransform>();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        isCreated = true;
        return rectTransform;
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

    private static void ApplyMaterial(GameObject targetObject, Material material)
    {
        Renderer renderer = targetObject.GetComponent<Renderer>();
        if (renderer == null || material == null)
        {
            return;
        }

        renderer.sharedMaterial = material;
    }

    private static void ApplyMaterialToChildren(GameObject targetObject, Material material)
    {
        if (targetObject == null || material == null)
        {
            return;
        }

        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }

    private static void EnsureRenderableMaterials(GameObject targetObject, Material fallbackMaterial)
    {
        if (targetObject == null || fallbackMaterial == null)
        {
            return;
        }

        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].sharedMaterial != null)
            {
                continue;
            }

            renderers[i].sharedMaterial = fallbackMaterial;
        }
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
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(targetObject);
    }

    private static void SetFloat(Object targetObject, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(targetObject);
    }
}
