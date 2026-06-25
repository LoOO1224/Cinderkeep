using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 5.26 Check 메뉴는 3일차 닫힌 루프에 필요한 씬, 데이터, UI, 보스 연결 상태를 한 번에 보고합니다.
// 자동 수정은 하지 않고, 팀장이 main 푸쉬 전 위험 지점을 빠르게 확인하는 읽기 전용 도구입니다.
public static class Cinderkeep526CheckReportPanel
{
    private const string MenuRoot = "Cinderkeep/5.26 Check Report/";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string DataFolderPath = "Assets/Resources/Cinderkeep/data";
    private const string ReportPath = "Temp/Cinderkeep_5_26_CheckReport.txt";
    private const int RequiredQuickSlotCount = 7;
    private const int RequiredRewardOptionCount = 3;
    private const int TargetCinderHeartSkillCount = 50;
    private const int MinimumImplementedRewardEffectTypeCount = 10;
    private const int MinimumReviveRewardCount = 1;
    private const int MinimumRunResultStatCount = 28;
    private static readonly string[] RequiredRunResultStatKeys =
    {
        "result_status",
        "reached_day",
        "survival_time",
        "failure_reason",
        "monster_kills",
        "boss_defeated",
        "enemy_damage_dealt",
        "tower_damage_dealt",
        "trap_damage_dealt",
        "player_damage_taken",
        "player_down_count",
        "cinderheart_damage_taken",
        "wood_gained",
        "stone_gained",
        "iron_gained",
        "gold_gained",
        "mithril_gained",
        "adamantium_gained",
        "crafted_item_count",
        "placed_building_count",
        "destroyed_building_count",
        "upgraded_building_count",
        "trap_crowd_control_score",
        "raw_meat_picked_up",
        "cooked_meat_created",
        "food_eaten_count",
        "satiety_restored",
        "selected_cinderheart_skills"
    };

    [MenuItem(MenuRoot + "Run Full 5.26 Check Report")]
    public static void RunFullCheckReport()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunFullCheckReport(scene, reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.26] check report passed: " + ReportPath + "\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.26] check report found issues: " + ReportPath + "\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Generate Report Only")]
    public static void GenerateReportOnly()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        RunFullCheckReport(scene, reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.26] check report generated: " + ReportPath);
    }

    private static bool RunFullCheckReport(Scene scene, StringBuilder reportBuilder)
    {
        bool isOk = true;
        reportBuilder.AppendLine("[Cinderkeep 5.26 Check Report]");
        reportBuilder.AppendLine("Scene: " + scene.path);
        reportBuilder.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        reportBuilder.AppendLine();

        isOk &= RunSceneWiringReport(scene, reportBuilder);
        isOk &= RunRequiredUiReport(scene, reportBuilder);
        isOk &= RunDataCheckReport(reportBuilder);
        isOk &= RunBuildingUpgradeReport(reportBuilder);
        isOk &= RunJsonShapeReport(reportBuilder);
        isOk &= RunPrefabKeyReport(reportBuilder);
        isOk &= RunPrefabComponentReport(reportBuilder);
        isOk &= RunMaterialKeyReport(reportBuilder);
        isOk &= RunBossClearFlowReport(scene, reportBuilder);
        isOk &= RunRunResultReport(scene, reportBuilder);

        reportBuilder.AppendLine();
        reportBuilder.AppendLine(isOk ? "[PASS] 5.26 Check 기준 통과" : "[CHECK] 5.26 Check 기준에서 확인 필요");
        return isOk;
    }

    private static bool RunSceneWiringReport(Scene scene, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Scene Wiring]");
        bool isOk = true;

        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "MainGame_Managers");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "MainGame_LoopConnector");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "MainGame_RuntimeManagers");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "Player");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "CinderHeart");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "EnemySpawnPoints");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "MainGame_RuntimeObjects");
        isOk &= AppendSceneObjectCheck(scene, reportBuilder, "Canvas_GameHUD");

        GameManager gameManager = FindComponentByName<GameManager>(scene, "GameManager");
        GameFlowController gameFlowController = FindComponentByName<GameFlowController>(scene, "GameFlowController");
        GameFlowEnemySpawnDirector enemySpawnDirector = FindComponentByName<GameFlowEnemySpawnDirector>(scene, "GameFlowEnemySpawnDirector");
        UIManager uiManager = FindComponentByName<UIManager>(scene, "UIManager");

        isOk &= AppendComponentCheck(reportBuilder, "GameManager", gameManager);
        isOk &= AppendComponentCheck(reportBuilder, "GameDataManager", FindComponentByName<GameDataManager>(scene, "GameDataManager"));
        isOk &= AppendComponentCheck(reportBuilder, "ResourceManager", FindComponentByName<ResourceManager>(scene, "ResourceManager"));
        isOk &= AppendComponentCheck(reportBuilder, "GameObjectManager", FindComponentByName<GameObjectManager>(scene, "GameObjectManager"));
        isOk &= AppendComponentCheck(reportBuilder, "BuildingManager", FindComponentByName<BuildingManager>(scene, "BuildingManager"));
        isOk &= AppendComponentCheck(reportBuilder, "SoundManager", FindComponentByName<SoundManager>(scene, "SoundManager"));
        isOk &= AppendComponentCheck(reportBuilder, "MapManager", FindComponentByName<MapManager>(scene, "MapManager"));
        isOk &= AppendComponentCheck(reportBuilder, "UIManager", uiManager);
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowController", gameFlowController);
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowEnemySpawnDirector", enemySpawnDirector);
        isOk &= AppendComponentCheck(reportBuilder, "PlayerStatus", FindComponentInScene<PlayerStatus>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerMovement", FindComponentInScene<PlayerMovement>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerToolController", FindComponentInScene<PlayerToolController>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerAttack", FindComponentInScene<PlayerAttack>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerBuild", FindComponentInScene<PlayerBuild>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerInteraction", FindComponentInScene<PlayerInteraction>(scene));

        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_gameDataManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_resourceManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_gameObjectManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_buildingManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_uiManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_soundManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_mapManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameManager, "_gameFlowController");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, gameFlowController, "_enemySpawnDirector");
        isOk &= AppendSerializedArrayCheck(reportBuilder, enemySpawnDirector, "_enemySpawnPoints", 1);

        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool RunRequiredUiReport(Scene scene, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Required UI]");
        bool isOk = true;

        UIManager uiManager = FindComponentByName<UIManager>(scene, "UIManager");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_hudRoot");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_inventoryRoot");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_gameOverPanel");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_inventoryUI");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_craftingUI");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_cinderHeartSkillSelectionUI");

        isOk &= AppendComponentCheck(reportBuilder, "PlayerHUD", FindComponentInScene<PlayerHUD>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "ResourceUI", FindComponentInScene<ResourceUI>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "InventoryUI", FindComponentInScene<InventoryUI>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "CraftingUI", FindComponentInScene<CraftingUI>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowTimerHUD", FindComponentInScene<GameFlowTimerHUD>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "HudTutorialGuide", FindComponentInScene<HudTutorialGuide>(scene));
        isOk &= AppendComponentCheck(reportBuilder, "CinderHeartSkillSelectionUI", FindComponentInScene<CinderHeartSkillSelectionUI>(scene));

        int quickSlotViewCount = FindComponentsInScene<QuickSlotView>(scene).Count;
        int rewardOptionCount = FindComponentsInScene<CinderHeartSkillOptionView>(scene).Count;
        isOk &= AppendCountCheck(reportBuilder, "QuickSlotView count", quickSlotViewCount, RequiredQuickSlotCount);
        isOk &= AppendCountCheck(reportBuilder, "CinderHeart reward option count", rewardOptionCount, RequiredRewardOptionCount);

        AppendRuntimeGeneratedCheck(reportBuilder, "BuildProgressUI", "PlayerBuild가 없으면 런타임에 생성합니다.");
        AppendRuntimeGeneratedCheck(reportBuilder, "RunResultUI", "UIManager가 결과창 오픈 시 없으면 보정합니다.");
        AppendRuntimeGeneratedCheck(reportBuilder, "PlayerEquipmentStatApplier", "GameManager가 시작/재시작 시 Player에 자동 보정합니다.");
        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool RunDataCheckReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Data Check]");
        GameObject tempObject = new GameObject("Temp_5_26_GameDataManager_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;
            isOk &= AppendCountCheck(reportBuilder, "resources", gameDataManager.ResourceDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "harvest_nodes", gameDataManager.HarvestNodeDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "tools", gameDataManager.ToolDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "weapons", gameDataManager.WeaponDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "armors", gameDataManager.ArmorDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "buildings", gameDataManager.BuildingDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "crafting_recipes", gameDataManager.CraftingRecipeDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "enemy_spawn_rules", gameDataManager.EnemySpawnRuleDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "game_flow_phases", gameDataManager.GameFlowPhaseDataList.Count, 1);
            isOk &= AppendCountCheck(reportBuilder, "cinderheart_skills", gameDataManager.CinderHeartSkillDataList.Count, TargetCinderHeartSkillCount);
            isOk &= AppendCountCheck(reportBuilder, "bosses", gameDataManager.BossDataList.Count, 1);

            isOk &= AppendRecipeExposureReport(gameDataManager, reportBuilder);
            isOk &= AppendCinderHeartSkillEffectReport(gameDataManager, reportBuilder);
            isOk &= AppendStarterLoopDataReport(gameDataManager, reportBuilder);
            reportBuilder.AppendLine();
            return isOk;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempObject);
        }
    }

    private static bool AppendRecipeExposureReport(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        int liveCount = 0;
        int hiddenCount = 0;
        bool isOk = true;

        foreach (KeyValuePair<string, CraftingRecipeData> pair in gameDataManager.CraftingRecipeDataList)
        {
            CraftingRecipeData recipeData = pair.Value;
            if (recipeData == null)
            {
                isOk &= AppendCheck(reportBuilder, "recipe: " + pair.Key, false, "null");
                continue;
            }

            if (GameDataCheckRules.IsImplementedCraftingRecipeResultType(recipeData.ResultDataType))
            {
                liveCount++;
            }
            else
            {
                hiddenCount++;
            }

            isOk &= AppendCheck(
                reportBuilder,
                "recipe type: " + recipeData.Id,
                GameDataCheckRules.IsSupportedCraftingRecipeResultType(recipeData.ResultDataType),
                recipeData.ResultDataType);
            isOk &= AppendRecipeResultReferenceCheck(gameDataManager, recipeData, reportBuilder);
            isOk &= AppendRecipeCostReferenceCheck(gameDataManager, recipeData, reportBuilder);
        }

        isOk &= AppendCountCheck(reportBuilder, "live crafting recipe count", liveCount, 1);
        AppendInfo(reportBuilder, "hidden roadmap recipe count", hiddenCount.ToString());
        return isOk;
    }

    private static bool AppendRecipeResultReferenceCheck(
        GameDataManager gameDataManager,
        CraftingRecipeData recipeData,
        StringBuilder reportBuilder)
    {
        if (recipeData == null)
        {
            return false;
        }

        bool hasResultReference = false;
        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeResource, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetResource(recipeData.ResultItemId) != null;
        }
        else if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeTool, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetTool(recipeData.ResultItemId) != null;
        }
        else if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeWeapon, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetWeapon(recipeData.ResultItemId) != null;
        }
        else if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeArmor, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetArmor(recipeData.ResultItemId) != null;
        }
        else if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeBuilding, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetBuilding(recipeData.ResultItemId) != null;
        }
        else if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeCinderHeartUpgrade, StringComparison.OrdinalIgnoreCase))
        {
            hasResultReference = gameDataManager.GetCinderHeartUpgrade(recipeData.ResultItemId) != null;
        }

        return AppendCheck(
            reportBuilder,
            "recipe result reference: " + recipeData.Id,
            hasResultReference,
            recipeData.ResultDataType + "/" + recipeData.ResultItemId);
    }

    private static bool AppendRecipeCostReferenceCheck(
        GameDataManager gameDataManager,
        CraftingRecipeData recipeData,
        StringBuilder reportBuilder)
    {
        if (recipeData == null || recipeData.Costs == null)
        {
            return false;
        }

        bool isOk = true;
        for (int i = 0; i < recipeData.Costs.Count; i++)
        {
            CraftingCostData costData = recipeData.Costs[i];
            bool hasCostReference = costData != null
                && costData.Amount > 0
                && gameDataManager.GetResource(costData.ResourceId) != null;

            isOk &= AppendCheck(
                reportBuilder,
                "recipe cost reference: " + recipeData.Id + "[" + i + "]",
                hasCostReference,
                costData == null ? "null" : costData.ResourceId + " x" + costData.Amount);
        }

        return isOk;
    }

    private static bool AppendCinderHeartSkillEffectReport(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        int implementedCount = 0;
        int roadmapCount = 0;
        int reviveRewardCount = 0;
        int dayOneNormalRewardCount = 0;
        int dayThreeNormalRewardCount = 0;
        HashSet<string> implementedEffectTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool isOk = true;

        foreach (KeyValuePair<string, CinderHeartSkillData> pair in gameDataManager.CinderHeartSkillDataList)
        {
            CinderHeartSkillData skillData = pair.Value;
            if (skillData == null)
            {
                isOk &= AppendCheck(reportBuilder, "cinderheart skill: " + pair.Key, false, "null");
                continue;
            }

            if (GameDataCheckRules.IsImplementedCinderHeartRewardEffect(skillData.EffectType))
            {
                implementedCount++;
                implementedEffectTypes.Add(skillData.EffectType);

                bool isReviveReward = string.Equals(
                    skillData.EffectType,
                    GameDataCheckRules.RewardEffectPlayerReviveRate,
                    StringComparison.OrdinalIgnoreCase);

                if (isReviveReward)
                {
                    reviveRewardCount++;
                }
                else
                {
                    if (skillData.RequiredDay <= GameRunModel.FirstDay)
                    {
                        dayOneNormalRewardCount++;
                    }

                    if (skillData.RequiredDay <= 3)
                    {
                        dayThreeNormalRewardCount++;
                    }
                }
            }
            else
            {
                roadmapCount++;
            }

            isOk &= AppendCheck(
                reportBuilder,
                "cinderheart skill weight: " + skillData.Id,
                skillData.Weight > 0,
                skillData.Weight.ToString());
            isOk &= AppendCheck(
                reportBuilder,
                "cinderheart skill required day: " + skillData.Id,
                skillData.RequiredDay >= GameRunModel.FirstDay,
                skillData.RequiredDay.ToString());
        }

        isOk &= AppendCountCheck(reportBuilder, "implemented CinderHeart reward effect count", implementedCount, TargetCinderHeartSkillCount);
        isOk &= AppendCountCheck(reportBuilder, "implemented CinderHeart effect type variety", implementedEffectTypes.Count, MinimumImplementedRewardEffectTypeCount);
        isOk &= AppendCountCheck(reportBuilder, "revive-only reward count", reviveRewardCount, MinimumReviveRewardCount);
        isOk &= AppendCountCheck(reportBuilder, "day 1 normal reward candidate count", dayOneNormalRewardCount, RequiredRewardOptionCount);
        isOk &= AppendCountCheck(reportBuilder, "day 3 normal reward candidate count", dayThreeNormalRewardCount, RequiredRewardOptionCount);
        isOk &= AppendCheck(reportBuilder, "roadmap CinderHeart reward effect count", roadmapCount == 0, roadmapCount.ToString());
        AppendInfo(reportBuilder, "roadmap CinderHeart reward effect count", roadmapCount.ToString());
        return isOk;
    }

    private static bool AppendStarterLoopDataReport(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Starter Loop Data]");
        bool isOk = true;

        isOk &= AppendCheck(reportBuilder, "resource: Wood", gameDataManager.GetResource(PlayerModel.ResourceWood) != null, PlayerModel.ResourceWood);
        isOk &= AppendCheck(reportBuilder, "resource: Stone", gameDataManager.GetResource(PlayerModel.ResourceStone) != null, PlayerModel.ResourceStone);
        isOk &= AppendCheck(reportBuilder, "tool: hand_stone", gameDataManager.GetTool(PlayerToolController.HandStoneToolDataId) != null, PlayerToolController.HandStoneToolDataId);
        isOk &= AppendStarterHandStoneCheck(gameDataManager, reportBuilder);
        isOk &= AppendCheck(reportBuilder, "recipe: stone_pickaxe", gameDataManager.GetCraftingRecipe("recipe_stone_pickaxe") != null, "recipe_stone_pickaxe");
        isOk &= AppendCheck(reportBuilder, "recipe: stone_axe", gameDataManager.GetCraftingRecipe("recipe_stone_axe") != null, "recipe_stone_axe");
        isOk &= AppendCheck(reportBuilder, "recipe: wood_wall", gameDataManager.GetCraftingRecipe("recipe_wood_wall") != null, "recipe_wood_wall");
        isOk &= AppendCheck(reportBuilder, "recipe: wood_tower", gameDataManager.GetCraftingRecipe("recipe_wood_tower") != null, "recipe_wood_tower");
        isOk &= AppendCheck(reportBuilder, "recipe: wood_slow_trap", gameDataManager.GetCraftingRecipe("recipe_wood_slow_trap") != null, "recipe_wood_slow_trap");
        isOk &= AppendCheck(reportBuilder, "building: wood_wall", gameDataManager.GetBuilding("wood_wall") != null, "wood_wall");
        isOk &= AppendCheck(reportBuilder, "building: wood_tower", gameDataManager.GetBuilding("wood_tower") != null, "wood_tower");
        isOk &= AppendCheck(reportBuilder, "building: wood_slow_trap", gameDataManager.GetBuilding("wood_slow_trap") != null, "wood_slow_trap");
        isOk &= AppendCheck(reportBuilder, "boss: frost_colossus", gameDataManager.GetBoss("frost_colossus") != null, "frost_colossus");
        isOk &= AppendCheck(reportBuilder, "food: raw_meat satiety", FoodItemIds.GetSatietyRestoreAmount(FoodItemIds.RawMeat) > 0f, FoodItemIds.RawMeat);
        isOk &= AppendCheck(reportBuilder, "food: cooked_meat satiety", FoodItemIds.GetSatietyRestoreAmount(FoodItemIds.CookedMeat) > FoodItemIds.GetSatietyRestoreAmount(FoodItemIds.RawMeat), FoodItemIds.CookedMeat);
        isOk &= AppendArmorProgressionCheck(gameDataManager, reportBuilder);

        return isOk;
    }

    private static bool AppendStarterHandStoneCheck(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        bool isOk = true;
        ToolData handStone = gameDataManager.GetTool(PlayerToolController.HandStoneToolDataId);
        HarvestNodeData woodTree = gameDataManager.GetHarvestNode("tree_wood_tier1");
        HarvestNodeData stoneRock = gameDataManager.GetHarvestNode("rock_stone");

        isOk &= AppendCheck(reportBuilder, "hand_stone gathers Wood", handStone != null && handStone.WoodGatherMultiplier > 0f, "wood multiplier");
        isOk &= AppendCheck(reportBuilder, "hand_stone gathers Stone", handStone != null && handStone.StoneGatherMultiplier > 0f, "stone multiplier");
        isOk &= AppendCheck(reportBuilder, "hand_stone blocks Iron+", handStone != null && handStone.IronGatherMultiplier <= 0f && handStone.GoldGatherMultiplier <= 0f && handStone.AdamantiumGatherMultiplier <= 0f, "high-tier multipliers");
        isOk &= AppendCheck(reportBuilder, "starter harvest node: tree_wood_tier1", woodTree != null && woodTree.RequiredToolTier <= 1, "tree_wood_tier1");
        isOk &= AppendCheck(reportBuilder, "starter harvest node: rock_stone", stoneRock != null && stoneRock.RequiredToolTier <= 1, "rock_stone");
        return isOk;
    }

    private static bool AppendArmorProgressionCheck(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        bool isOk = true;
        isOk &= AppendArmorStatCheck(gameDataManager, reportBuilder, "iron_helmet");
        isOk &= AppendArmorStatCheck(gameDataManager, reportBuilder, "iron_chest_armor");
        isOk &= AppendArmorStatCheck(gameDataManager, reportBuilder, "iron_boots");
        isOk &= AppendArmorStatCheck(gameDataManager, reportBuilder, "adamantium_chest_armor");
        return isOk;
    }

    private static bool AppendArmorStatCheck(GameDataManager gameDataManager, StringBuilder reportBuilder, string armorId)
    {
        ArmorData armorData = gameDataManager.GetArmor(armorId);
        bool hasUsefulStats = armorData != null
            && armorData.Defense > 0f
            && string.IsNullOrEmpty(armorData.PrefabKey) == false
            && string.IsNullOrEmpty(armorData.CraftingRecipeId) == false;

        return AppendCheck(reportBuilder, "armor stat: " + armorId, hasUsefulStats, armorId);
    }

    private static bool RunJsonShapeReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[JSON Shape Check]");
        bool isOk = true;
        string[] jsonPaths = Directory.GetFiles(DataFolderPath, "*.json", SearchOption.TopDirectoryOnly);

        isOk &= AppendCountCheck(reportBuilder, "json file count", jsonPaths.Length, 1);
        for (int i = 0; i < jsonPaths.Length; i++)
        {
            string assetPath = NormalizeAssetPath(jsonPaths[i]);
            TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            bool hasValidShape = false;
            string detail = "missing";

            if (jsonAsset != null)
            {
                try
                {
                    if (IsGameModeSettingsJson(assetPath))
                    {
                        GameModeSettingsData settingsData = JsonUtility.FromJson<GameModeSettingsData>(jsonAsset.text);
                        hasValidShape = IsValidGameModeSettings(settingsData);
                        detail = hasValidShape ? "mode durations ok" : "invalid mode durations";
                    }
                    else
                    {
                        RawCatalogProbe catalogProbe = JsonUtility.FromJson<RawCatalogProbe>(jsonAsset.text);
                        hasValidShape = catalogProbe != null && catalogProbe.Items != null && catalogProbe.Items.Count > 0;
                        detail = hasValidShape ? catalogProbe.Items.Count.ToString() : "Items missing or empty";
                    }
                }
                catch (Exception exception)
                {
                    detail = exception.GetType().Name;
                }
            }

            isOk &= AppendCheck(reportBuilder, Path.GetFileName(assetPath), hasValidShape, detail);
        }

        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool IsGameModeSettingsJson(string assetPath)
    {
        return string.Equals(
            Path.GetFileName(assetPath),
            "game_mode_settings.json",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidGameModeSettings(GameModeSettingsData settingsData)
    {
        return settingsData != null
            && settingsData.NormalDayDuration > 0f
            && settingsData.NormalNightDuration > 0f
            && settingsData.TestFastDayDuration > 0f
            && settingsData.TestFastNightDuration > 0f;
    }

    private static bool RunBuildingUpgradeReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Building Upgrade Check]");
        GameObject tempObject = new GameObject("Temp_5_26_BuildingUpgrade_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;
            isOk &= AppendCountCheck(reportBuilder, "building_upgrades", gameDataManager.BuildingUpgradeDataList.Count, 1);
            isOk &= AppendRequiredBuildingUpgradeCheck(gameDataManager, reportBuilder, "upgrade_wall_wood_to_iron");
            isOk &= AppendRequiredBuildingUpgradeCheck(gameDataManager, reportBuilder, "upgrade_tower_wood_to_iron");
            isOk &= AppendRequiredBuildingUpgradeCheck(gameDataManager, reportBuilder, "upgrade_trap_wood_to_iron");
            isOk &= AppendRequiredBuildingUpgradeCheck(gameDataManager, reportBuilder, "upgrade_trap_iron_to_gold");
            isOk &= AppendRequiredBuildingUpgradeCheck(gameDataManager, reportBuilder, "upgrade_trap_gold_to_adamantium");

            foreach (KeyValuePair<string, BuildingUpgradeData> pair in gameDataManager.BuildingUpgradeDataList)
            {
                BuildingUpgradeData upgradeData = pair.Value;
                if (upgradeData == null)
                {
                    isOk &= AppendCheck(reportBuilder, "building upgrade: " + pair.Key, false, "null");
                    continue;
                }

                isOk &= AppendCheck(
                    reportBuilder,
                    "upgrade from: " + upgradeData.Id,
                    gameDataManager.GetBuilding(upgradeData.FromBuildingId) != null,
                    upgradeData.FromBuildingId);
                isOk &= AppendCheck(
                    reportBuilder,
                    "upgrade to: " + upgradeData.Id,
                    gameDataManager.GetBuilding(upgradeData.ToBuildingId) != null,
                    upgradeData.ToBuildingId);
                isOk &= AppendCheck(
                    reportBuilder,
                    "upgrade recipe: " + upgradeData.Id,
                    gameDataManager.GetCraftingRecipe(upgradeData.CraftingRecipeId) != null,
                    upgradeData.CraftingRecipeId);
                isOk &= AppendCheck(
                    reportBuilder,
                    "upgrade required day: " + upgradeData.Id,
                    upgradeData.RequiredDay >= GameRunModel.FirstDay,
                    upgradeData.RequiredDay.ToString());
            }

            reportBuilder.AppendLine();
            return isOk;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempObject);
        }
    }

    private static bool AppendRequiredBuildingUpgradeCheck(
        GameDataManager gameDataManager,
        StringBuilder reportBuilder,
        string upgradeId)
    {
        bool hasUpgrade = gameDataManager != null
            && gameDataManager.BuildingUpgradeDataList != null
            && gameDataManager.BuildingUpgradeDataList.ContainsKey(upgradeId);
        return AppendCheck(reportBuilder, "required upgrade: " + upgradeId, hasUpgrade, upgradeId);
    }

    private static bool RunPrefabKeyReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Missing Prefab Key Report]");
        GameObject tempObject = new GameObject("Temp_5_26_PrefabKey_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;

            foreach (KeyValuePair<string, ResourceData> pair in gameDataManager.ResourceDataList)
            {
                ResourceData resourceData = pair.Value;
                if (resourceData == null || string.IsNullOrEmpty(resourceData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "resource prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "resource prefab: " + resourceData.Id, resourceData.PrefabKey);
            }

            foreach (KeyValuePair<string, ToolData> pair in gameDataManager.ToolDataList)
            {
                ToolData toolData = pair.Value;
                if (toolData == null || string.IsNullOrEmpty(toolData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "tool prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "tool prefab: " + toolData.Id, toolData.PrefabKey);
            }

            foreach (KeyValuePair<string, WeaponData> pair in gameDataManager.WeaponDataList)
            {
                WeaponData weaponData = pair.Value;
                if (weaponData == null || string.IsNullOrEmpty(weaponData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "weapon prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "weapon prefab: " + weaponData.Id, weaponData.PrefabKey);
            }

            foreach (KeyValuePair<string, ArmorData> pair in gameDataManager.ArmorDataList)
            {
                ArmorData armorData = pair.Value;
                if (armorData == null || string.IsNullOrEmpty(armorData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "armor prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "armor prefab: " + armorData.Id, armorData.PrefabKey);
            }

            foreach (KeyValuePair<string, BuildingData> pair in gameDataManager.BuildingDataList)
            {
                BuildingData buildingData = pair.Value;
                if (buildingData == null || string.IsNullOrEmpty(buildingData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "building prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "building prefab: " + buildingData.Id, buildingData.PrefabKey);
            }

            foreach (KeyValuePair<string, HarvestNodeData> pair in gameDataManager.HarvestNodeDataList)
            {
                HarvestNodeData nodeData = pair.Value;
                if (nodeData == null || string.IsNullOrEmpty(nodeData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "harvest node prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "harvest node prefab: " + nodeData.Id, nodeData.PrefabKey);
            }

            foreach (KeyValuePair<string, CraftingStationData> pair in gameDataManager.CraftingStationDataList)
            {
                CraftingStationData stationData = pair.Value;
                if (stationData == null || string.IsNullOrEmpty(stationData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "crafting station prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "crafting station prefab: " + stationData.Id, stationData.PrefabKey);
            }

            foreach (KeyValuePair<string, BossData> pair in gameDataManager.BossDataList)
            {
                BossData bossData = pair.Value;
                if (bossData == null || string.IsNullOrEmpty(bossData.PrefabKey))
                {
                    isOk &= AppendCheck(reportBuilder, "boss prefab: " + pair.Key, false, "PrefabKey empty");
                    continue;
                }

                isOk &= AppendPrefabKeyCheck(reportBuilder, "boss prefab: " + bossData.Id, bossData.PrefabKey);
            }

            reportBuilder.AppendLine();
            return isOk;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempObject);
        }
    }

    private static bool RunMaterialKeyReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Missing Material Key Report]");
        GameObject tempObject = new GameObject("Temp_5_26_MaterialKey_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;

            foreach (KeyValuePair<string, BuildingData> pair in gameDataManager.BuildingDataList)
            {
                BuildingData buildingData = pair.Value;
                if (buildingData == null || string.IsNullOrEmpty(buildingData.MaterialKey))
                {
                    isOk &= AppendCheck(reportBuilder, "building material: " + pair.Key, false, "MaterialKey empty");
                    continue;
                }

                isOk &= AppendMaterialKeyCheck(reportBuilder, "building material: " + buildingData.Id, buildingData.MaterialKey);
            }

            foreach (KeyValuePair<string, HarvestNodeData> pair in gameDataManager.HarvestNodeDataList)
            {
                HarvestNodeData nodeData = pair.Value;
                if (nodeData == null || string.IsNullOrEmpty(nodeData.MaterialKey))
                {
                    isOk &= AppendCheck(reportBuilder, "harvest node material: " + pair.Key, false, "MaterialKey empty");
                    continue;
                }

                isOk &= AppendMaterialKeyCheck(reportBuilder, "harvest node material: " + nodeData.Id, nodeData.MaterialKey);
            }

            reportBuilder.AppendLine();
            return isOk;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempObject);
        }
    }

    private static bool RunPrefabComponentReport(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Prefab Component Check]");
        GameObject tempObject = new GameObject("Temp_5_26_PrefabComponent_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;

            foreach (KeyValuePair<string, BossData> pair in gameDataManager.BossDataList)
            {
                BossData bossData = pair.Value;
                if (bossData == null)
                {
                    isOk &= AppendCheck(reportBuilder, "boss prefab component: " + pair.Key, false, "null");
                    continue;
                }

                isOk &= AppendPrefabHasComponentCheck<EnemyStatus>(reportBuilder, "boss EnemyStatus: " + bossData.Id, bossData.PrefabKey);
                isOk &= AppendPrefabHasComponentCheck<EnemyMovement>(reportBuilder, "boss EnemyMovement: " + bossData.Id, bossData.PrefabKey);
                isOk &= AppendPrefabHasComponentCheck<EnemyDetector>(reportBuilder, "boss EnemyDetector: " + bossData.Id, bossData.PrefabKey);
                isOk &= AppendPrefabHasComponentCheck<EnemyAttack>(reportBuilder, "boss EnemyAttack: " + bossData.Id, bossData.PrefabKey);
                isOk &= AppendPrefabHasComponentCheck<EnemyBrain>(reportBuilder, "boss EnemyBrain: " + bossData.Id, bossData.PrefabKey);
                isOk &= AppendPrefabHasComponentCheck<Damageable>(reportBuilder, "boss Damageable: " + bossData.Id, bossData.PrefabKey);
            }

            foreach (KeyValuePair<string, BuildingData> pair in gameDataManager.BuildingDataList)
            {
                BuildingData buildingData = pair.Value;
                if (buildingData == null)
                {
                    isOk &= AppendCheck(reportBuilder, "building prefab component: " + pair.Key, false, "null");
                    continue;
                }

                isOk &= AppendPrefabHasComponentCheck<BuildingHp>(reportBuilder, "building BuildingHp: " + buildingData.Id, buildingData.PrefabKey);

                if (string.Equals(buildingData.BuildingType, "Tower", StringComparison.OrdinalIgnoreCase))
                {
                    isOk &= AppendPrefabHasComponentCheck<BuildingTower>(reportBuilder, "tower BuildingTower: " + buildingData.Id, buildingData.PrefabKey);
                    isOk &= AppendPrefabHasComponentCheck<TowerTargeting>(reportBuilder, "tower TowerTargeting: " + buildingData.Id, buildingData.PrefabKey);
                    isOk &= AppendPrefabHasComponentCheck<TowerAttack>(reportBuilder, "tower TowerAttack: " + buildingData.Id, buildingData.PrefabKey);
                    isOk &= AppendPrefabHasComponentCheck<DamageDealer>(reportBuilder, "tower DamageDealer: " + buildingData.Id, buildingData.PrefabKey);
                }
                else if (string.Equals(buildingData.BuildingType, "Trap", StringComparison.OrdinalIgnoreCase))
                {
                    isOk &= AppendPrefabHasComponentCheck<TrapCrowdControlReporter>(reportBuilder, "trap reporter: " + buildingData.Id, buildingData.PrefabKey);
                    isOk &= AppendPrefabHasComponentCheck<TrapSlowZone>(reportBuilder, "trap slow zone: " + buildingData.Id, buildingData.PrefabKey);
                    isOk &= AppendPrefabHasTriggerColliderCheck(reportBuilder, "trap trigger collider: " + buildingData.Id, buildingData.PrefabKey);
                }
            }

            foreach (KeyValuePair<string, HarvestNodeData> pair in gameDataManager.HarvestNodeDataList)
            {
                HarvestNodeData nodeData = pair.Value;
                if (nodeData == null)
                {
                    isOk &= AppendCheck(reportBuilder, "harvest prefab component: " + pair.Key, false, "null");
                    continue;
                }

                isOk &= AppendPrefabHasComponentCheck<ResourceNode>(reportBuilder, "harvest ResourceNode: " + nodeData.Id, nodeData.PrefabKey);
            }

            reportBuilder.AppendLine();
            return isOk;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(tempObject);
        }
    }

    private static bool RunBossClearFlowReport(Scene scene, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Boss / ClearFlow Check]");
        bool isOk = true;
        GameFlowController gameFlowController = FindComponentByName<GameFlowController>(scene, "GameFlowController");
        GameFlowEnemySpawnDirector enemySpawnDirector = FindComponentByName<GameFlowEnemySpawnDirector>(scene, "GameFlowEnemySpawnDirector");
        List<EnemySpawnPoint> spawnPoints = FindComponentsInScene<EnemySpawnPoint>(scene);
        int bossCandidateCount = 0;
        int dedicatedBossPrefabReferenceCount = 0;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            EnemySpawnPoint spawnPoint = spawnPoints[i];
            if (spawnPoint != null && spawnPoint.HasBossSpawnCandidate())
            {
                bossCandidateCount++;
            }

            if (HasSerializedObjectReference(spawnPoint, "_bossEnemyPrefab"))
            {
                dedicatedBossPrefabReferenceCount++;
            }
        }

        isOk &= AppendComponentCheck(reportBuilder, "GameFlowController", gameFlowController);
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowEnemySpawnDirector", enemySpawnDirector);
        isOk &= AppendCountCheck(reportBuilder, "boss spawn candidate count", bossCandidateCount, 1);
        isOk &= AppendCountCheck(reportBuilder, "dedicated boss prefab reference count", dedicatedBossPrefabReferenceCount, 1);
        isOk &= AppendCheck(reportBuilder, "Boss death -> ClearFlow", gameFlowController != null && enemySpawnDirector != null, "GameFlowController.HandleBossDefeated 연결 구조 확인");
        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool RunRunResultReport(Scene scene, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Run Result Check]");
        bool isOk = true;
        RunResultStatDataCatalog runResultCatalog = LoadRunResultStatCatalog();
        int statCount = runResultCatalog == null || runResultCatalog.Items == null ? 0 : runResultCatalog.Items.Count;

        isOk &= AppendCountCheck(reportBuilder, "run_result_stats count", statCount, MinimumRunResultStatCount);
        isOk &= AppendRunResultStatKeyCheck(reportBuilder, runResultCatalog);
        isOk &= AppendSceneRunResultUiCheck(scene, reportBuilder);
        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool AppendRunResultStatKeyCheck(StringBuilder reportBuilder, RunResultStatDataCatalog runResultCatalog)
    {
        bool isOk = true;
        HashSet<string> statKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (runResultCatalog != null && runResultCatalog.Items != null)
        {
            for (int i = 0; i < runResultCatalog.Items.Count; i++)
            {
                RunResultStatData statData = runResultCatalog.Items[i];
                if (statData != null && string.IsNullOrEmpty(statData.StatKey) == false)
                {
                    statKeys.Add(statData.StatKey);
                    isOk &= AppendCheck(
                        reportBuilder,
                        "run result UI supports stat key: " + statData.StatKey,
                        RunResultUI.IsSupportedStatKey(statData.StatKey),
                        statData.StatKey);
                }
            }
        }

        for (int i = 0; i < RequiredRunResultStatKeys.Length; i++)
        {
            string statKey = RequiredRunResultStatKeys[i];
            isOk &= AppendCheck(reportBuilder, "run result stat key: " + statKey, statKeys.Contains(statKey), statKey);
        }

        return isOk;
    }

    private static bool AppendSceneRunResultUiCheck(Scene scene, StringBuilder reportBuilder)
    {
        UIManager uiManager = FindComponentByName<UIManager>(scene, "UIManager");
        bool hasPanel = AppendSerializedReferenceCheck(reportBuilder, uiManager, "_gameOverPanel");
        bool hasText = AppendSerializedReferenceCheck(reportBuilder, uiManager, "_runResultText");
        AppendRuntimeGeneratedCheck(reportBuilder, "RunResultTracker", "GameManager가 한 판 시작 시 EnsureSceneTracker로 생성합니다.");
        return hasPanel && hasText;
    }

    private static bool AppendPrefabKeyCheck(StringBuilder reportBuilder, string label, string prefabKey)
    {
        string firstSimilarPath;
        string exactPath = FindExactPrefabPath(prefabKey, out firstSimilarPath);

        bool isOk = string.IsNullOrEmpty(exactPath) == false;
        string detail = isOk ? exactPath : string.IsNullOrEmpty(firstSimilarPath) ? prefabKey : "similar only: " + firstSimilarPath;
        return AppendCheck(reportBuilder, label, isOk, detail);
    }

    private static bool AppendPrefabHasComponentCheck<TComponent>(
        StringBuilder reportBuilder,
        string label,
        string prefabKey)
        where TComponent : Component
    {
        string assetPath = FindExactPrefabPath(prefabKey, out _);
        GameObject prefabObject = string.IsNullOrEmpty(assetPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        TComponent component = prefabObject == null ? null : prefabObject.GetComponentInChildren<TComponent>(true);
        return AppendCheck(reportBuilder, label, component != null, string.IsNullOrEmpty(assetPath) ? prefabKey : assetPath);
    }

    private static bool AppendPrefabHasTriggerColliderCheck(
        StringBuilder reportBuilder,
        string label,
        string prefabKey)
    {
        string assetPath = FindExactPrefabPath(prefabKey, out _);
        GameObject prefabObject = string.IsNullOrEmpty(assetPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        Collider[] colliders = prefabObject == null ? null : prefabObject.GetComponentsInChildren<Collider>(true);
        bool hasTriggerCollider = false;

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].isTrigger)
                {
                    hasTriggerCollider = true;
                    break;
                }
            }
        }

        return AppendCheck(reportBuilder, label, hasTriggerCollider, string.IsNullOrEmpty(assetPath) ? prefabKey : assetPath);
    }

    private static string FindExactPrefabPath(string prefabKey, out string firstSimilarPath)
    {
        firstSimilarPath = "";
        string exactPath = "";
        if (string.IsNullOrEmpty(prefabKey))
        {
            return exactPath;
        }

        string[] guids = AssetDatabase.FindAssets(prefabKey + " t:Prefab", new[] { "Assets" });
        if (guids == null)
        {
            return exactPath;
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(firstSimilarPath))
            {
                firstSimilarPath = assetPath;
            }

            if (string.Equals(Path.GetFileNameWithoutExtension(assetPath), prefabKey, StringComparison.OrdinalIgnoreCase))
            {
                exactPath = assetPath;
                break;
            }
        }

        return exactPath;
    }

    private static bool AppendMaterialKeyCheck(StringBuilder reportBuilder, string label, string materialKey)
    {
        string[] guids = AssetDatabase.FindAssets(materialKey + " t:Material", new[] { "Assets" });
        string exactPath = "";
        string firstSimilarPath = "";

        if (guids != null)
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(firstSimilarPath))
                {
                    firstSimilarPath = assetPath;
                }

                if (string.Equals(Path.GetFileNameWithoutExtension(assetPath), materialKey, StringComparison.OrdinalIgnoreCase))
                {
                    exactPath = assetPath;
                    break;
                }
            }
        }

        bool isOk = string.IsNullOrEmpty(exactPath) == false;
        string detail = isOk ? exactPath : string.IsNullOrEmpty(firstSimilarPath) ? materialKey : "similar only: " + firstSimilarPath;
        return AppendCheck(reportBuilder, label, isOk, detail);
    }

    private static RunResultStatDataCatalog LoadRunResultStatCatalog()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("Cinderkeep/data/run_result_stats");
        if (jsonAsset == null)
        {
            return null;
        }

        return JsonUtility.FromJson<RunResultStatDataCatalog>(jsonAsset.text);
    }

    private static Scene OpenTargetScene()
    {
        if (Application.isPlaying)
        {
            return SceneManager.GetActiveScene();
        }

        return EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
    }

    private static bool AppendSceneObjectCheck(Scene scene, StringBuilder reportBuilder, string objectName)
    {
        GameObject targetObject = FindObjectByName(scene, objectName);
        return AppendCheck(reportBuilder, "Scene Object: " + objectName, targetObject != null, targetObject == null ? "missing" : targetObject.name);
    }

    private static bool AppendComponentCheck<TComponent>(StringBuilder reportBuilder, string label, TComponent component)
        where TComponent : Component
    {
        return AppendCheck(reportBuilder, "Component: " + label, component != null, component == null ? "missing" : component.name);
    }

    private static bool AppendSerializedReferenceCheck(StringBuilder reportBuilder, UnityEngine.Object targetObject, string propertyName)
    {
        if (targetObject == null)
        {
            return AppendCheck(reportBuilder, "Reference: " + propertyName, false, "owner missing");
        }

        SerializedProperty property = GetSerializedProperty(targetObject, propertyName);
        if (property == null)
        {
            return AppendCheck(reportBuilder, "Reference: " + targetObject.name + "." + propertyName, false, "property missing");
        }

        UnityEngine.Object referenceValue = property.objectReferenceValue;
        return AppendCheck(
            reportBuilder,
            "Reference: " + targetObject.name + "." + propertyName,
            referenceValue != null,
            referenceValue == null ? "missing" : referenceValue.name);
    }

    private static bool AppendSerializedArrayCheck(StringBuilder reportBuilder, UnityEngine.Object targetObject, string propertyName, int minimumCount)
    {
        if (targetObject == null)
        {
            return AppendCheck(reportBuilder, "Array: " + propertyName, false, "owner missing");
        }

        SerializedProperty property = GetSerializedProperty(targetObject, propertyName);
        if (property == null || property.isArray == false)
        {
            return AppendCheck(reportBuilder, "Array: " + targetObject.name + "." + propertyName, false, "property missing");
        }

        bool hasEnoughItems = property.arraySize >= minimumCount;
        bool hasOnlyAssignedItems = true;
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty itemProperty = property.GetArrayElementAtIndex(i);
            if (itemProperty.objectReferenceValue == null)
            {
                hasOnlyAssignedItems = false;
                break;
            }
        }

        bool isOk = hasEnoughItems && hasOnlyAssignedItems;
        return AppendCheck(reportBuilder, "Array: " + targetObject.name + "." + propertyName, isOk, property.arraySize.ToString());
    }

    private static bool AppendCountCheck(StringBuilder reportBuilder, string label, int count, int minimumCount)
    {
        return AppendCheck(reportBuilder, label, count >= minimumCount, count.ToString());
    }

    private static bool AppendCheck(StringBuilder reportBuilder, string label, bool isOk, string detail)
    {
        reportBuilder.AppendLine((isOk ? "[OK] " : "[Issue] ") + label + " - " + detail);
        return isOk;
    }

    private static void AppendInfo(StringBuilder reportBuilder, string label, string detail)
    {
        reportBuilder.AppendLine("[Info] " + label + " - " + detail);
    }

    private static void AppendRuntimeGeneratedCheck(StringBuilder reportBuilder, string label, string detail)
    {
        reportBuilder.AppendLine("[Runtime] " + label + " - " + detail);
    }

    private static SerializedProperty GetSerializedProperty(UnityEngine.Object targetObject, string propertyName)
    {
        if (targetObject == null)
        {
            return null;
        }

        SerializedObject serializedObject = new SerializedObject(targetObject);
        return serializedObject.FindProperty(propertyName);
    }

    private static bool HasSerializedObjectReference(UnityEngine.Object targetObject, string propertyName)
    {
        SerializedProperty property = GetSerializedProperty(targetObject, propertyName);
        return property != null && property.objectReferenceValue != null;
    }

    private static TComponent FindComponentByName<TComponent>(Scene scene, string objectName)
        where TComponent : Component
    {
        GameObject targetObject = FindObjectByName(scene, objectName);
        if (targetObject == null)
        {
            return null;
        }

        return targetObject.GetComponent<TComponent>();
    }

    private static TComponent FindComponentInScene<TComponent>(Scene scene)
        where TComponent : Component
    {
        List<TComponent> components = FindComponentsInScene<TComponent>(scene);
        return components.Count <= 0 ? null : components[0];
    }

    private static List<TComponent> FindComponentsInScene<TComponent>(Scene scene)
        where TComponent : Component
    {
        List<TComponent> results = new List<TComponent>();
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            results.AddRange(rootObjects[i].GetComponentsInChildren<TComponent>(true));
        }

        return results;
    }

    private static GameObject FindObjectByName(Scene scene, string objectName)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject result = FindObjectByNameRecursive(rootObjects[i].transform, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static GameObject FindObjectByNameRecursive(Transform currentTransform, string objectName)
    {
        if (currentTransform.name == objectName)
        {
            return currentTransform.gameObject;
        }

        for (int i = 0; i < currentTransform.childCount; i++)
        {
            GameObject result = FindObjectByNameRecursive(currentTransform.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace("\\", "/");
    }

    private static void WriteReport(StringBuilder reportBuilder)
    {
        string directoryPath = Path.GetDirectoryName(ReportPath);
        if (string.IsNullOrEmpty(directoryPath) == false && Directory.Exists(directoryPath) == false)
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllText(ReportPath, reportBuilder.ToString());
    }

    [Serializable]
    private sealed class RawCatalogProbe
    {
        public List<RawItemProbe> Items = new List<RawItemProbe>();
    }

    [Serializable]
    private sealed class RawItemProbe
    {
        [SerializeField] private string _id;

        public string Id
        {
            get
            {
                return _id;
            }
        }
    }
}
