using System.Collections.Generic;
using System.IO;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEngine;

// JSON 데이터가 현재 구현된 게임 규칙과 맞는지 검사하는 에디터 도구입니다.
// 구현되지 않은 데이터는 로드맵 후보로 보고하되, 실제 플레이 후보와 구분합니다.
public static class Cinderkeep502DataCheckPanel
{
    private const string MenuRoot = "Cinderkeep/5.02 Data Check/";
    private const string MenuRoot524 = "Cinderkeep/5.24 Data Check/";
    private const string ReportPath = "Temp/Cinderkeep_5_24_DataCheckReport.txt";

    [MenuItem(MenuRoot524 + "Run Full 5.24 Data Check")]
    [MenuItem(MenuRoot + "Run Full 5.02 Data Check")]
    public static void RunFullDataCheck()
    {
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunCheck(reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.24] Data check passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.24] Data check found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot524 + "Generate Data Check Report")]
    [MenuItem(MenuRoot + "Generate Data Check Report")]
    public static void GenerateDataCheckReport()
    {
        StringBuilder reportBuilder = new StringBuilder();
        RunCheck(reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.24] Data check report generated: " + ReportPath);
    }

    private static bool RunCheck(StringBuilder reportBuilder)
    {
        GameObject tempObject = new GameObject("Temp_5_02_GameDataManager_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;
            reportBuilder.AppendLine("[5.24 Data Check]");
            isOk &= ValidateCatalogCounts(gameDataManager, reportBuilder);
            isOk &= ValidateCraftingRecipes(gameDataManager, reportBuilder);
            isOk &= ValidateBuildings(gameDataManager, reportBuilder);
            isOk &= ValidateHarvestNodes(gameDataManager, reportBuilder);
            isOk &= ValidateCinderHeartSkills(gameDataManager, reportBuilder);
            isOk &= ValidateRequiredStarterData(gameDataManager, reportBuilder);
            return isOk;
        }
        finally
        {
            Object.DestroyImmediate(tempObject);
        }
    }

    private static bool ValidateCatalogCounts(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[Catalog Counts]");
        bool isOk = true;
        isOk &= AppendCount(reportBuilder, "resources", gameDataManager.ResourceDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "harvest_nodes", gameDataManager.HarvestNodeDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "tools", gameDataManager.ToolDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "weapons", gameDataManager.WeaponDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "armors", gameDataManager.ArmorDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "buildings", gameDataManager.BuildingDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "crafting_recipes", gameDataManager.CraftingRecipeDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "enemy_spawn_rules", gameDataManager.EnemySpawnRuleDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "game_flow_phases", gameDataManager.GameFlowPhaseDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "cinderheart_skills", gameDataManager.CinderHeartSkillDataList.Count, 1);
        isOk &= AppendCount(reportBuilder, "bosses", gameDataManager.BossDataList.Count, 1);
        return isOk;
    }

    private static bool ValidateCraftingRecipes(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[Crafting Recipes]");
        bool isOk = true;
        foreach (KeyValuePair<string, CraftingRecipeData> pair in gameDataManager.CraftingRecipeDataList)
        {
            CraftingRecipeData recipeData = pair.Value;
            if (recipeData == null)
            {
                isOk &= AppendCheck(reportBuilder, pair.Key, false, "recipe is null");
                continue;
            }

            isOk &= AppendCheck(
                reportBuilder,
                recipeData.Id + ".ResultDataType",
                GameDataCheckRules.IsSupportedCraftingRecipeResultType(recipeData.ResultDataType),
                recipeData.ResultDataType);

            AppendRecipeExposure(reportBuilder, recipeData);
            isOk &= ValidateRecipeResult(gameDataManager, reportBuilder, recipeData);
            isOk &= ValidateRecipeCosts(gameDataManager, reportBuilder, recipeData);
        }

        return isOk;
    }

    private static void AppendRecipeExposure(StringBuilder reportBuilder, CraftingRecipeData recipeData)
    {
        if (recipeData == null)
        {
            return;
        }

        bool isLive = GameDataCheckRules.IsImplementedCraftingRecipeResultType(recipeData.ResultDataType);
        reportBuilder.AppendLine((isLive ? "[Live Recipe] " : "[Hidden Recipe] ")
            + recipeData.Id
            + " - "
            + recipeData.ResultDataType
            + " / "
            + recipeData.ResultItemId);
    }

    private static bool ValidateRecipeResult(GameDataManager gameDataManager, StringBuilder reportBuilder, CraftingRecipeData recipeData)
    {
        if (string.IsNullOrEmpty(recipeData.ResultItemId))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", false, "empty");
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeResource, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ResourceDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeTool, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ToolDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeWeapon, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.WeaponDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeArmor, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ArmorDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeBuilding, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.BuildingDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataCheckRules.RecipeResultTypeCinderHeartUpgrade, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.CinderHeartUpgradeDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        return false;
    }

    private static bool ValidateRecipeCosts(GameDataManager gameDataManager, StringBuilder reportBuilder, CraftingRecipeData recipeData)
    {
        bool isOk = true;
        IReadOnlyList<CraftingCostData> costs = recipeData.Costs;
        isOk &= AppendCheck(reportBuilder, recipeData.Id + ".Costs", costs != null && costs.Count > 0, costs == null ? "null" : costs.Count.ToString());
        if (costs == null)
        {
            return isOk;
        }

        for (int i = 0; i < costs.Count; i++)
        {
            CraftingCostData costData = costs[i];
            if (costData == null)
            {
                isOk &= AppendCheck(reportBuilder, recipeData.Id + ".Cost[" + i + "]", false, "null");
                continue;
            }

            isOk &= AppendCheck(
                reportBuilder,
                recipeData.Id + ".Cost[" + i + "].ResourceId",
                gameDataManager.ResourceDataList.ContainsKey(costData.ResourceId),
                costData.ResourceId);
            isOk &= AppendCheck(reportBuilder, recipeData.Id + ".Cost[" + i + "].Amount", costData.Amount > 0, costData.Amount.ToString());
        }

        return isOk;
    }

    private static bool ValidateBuildings(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[Buildings]");
        bool isOk = true;
        int wallCount = 0;
        int towerCount = 0;
        int trapCount = 0;
        int stationCount = 0;

        foreach (KeyValuePair<string, BuildingData> pair in gameDataManager.BuildingDataList)
        {
            BuildingData buildingData = pair.Value;
            if (buildingData == null)
            {
                isOk &= AppendCheck(reportBuilder, pair.Key, false, "building is null");
                continue;
            }

            if (string.Equals(buildingData.BuildingType, "Wall", System.StringComparison.OrdinalIgnoreCase))
            {
                wallCount++;
            }
            else if (string.Equals(buildingData.BuildingType, "Tower", System.StringComparison.OrdinalIgnoreCase))
            {
                towerCount++;
            }
            else if (string.Equals(buildingData.BuildingType, "Trap", System.StringComparison.OrdinalIgnoreCase))
            {
                trapCount++;
            }
            else if (string.Equals(buildingData.BuildingType, "Station", System.StringComparison.OrdinalIgnoreCase))
            {
                stationCount++;
            }
            else
            {
                isOk &= AppendCheck(reportBuilder, buildingData.Id + ".BuildingType", false, buildingData.BuildingType);
            }

            isOk &= AppendCheck(reportBuilder, buildingData.Id + ".DisplayName", string.IsNullOrEmpty(buildingData.DisplayName) == false, buildingData.DisplayName);
            isOk &= AppendCheck(reportBuilder, buildingData.Id + ".MaxHealth", buildingData.MaxHealth > 0f, buildingData.MaxHealth.ToString());

            bool hasRecipe = string.IsNullOrEmpty(buildingData.CraftingRecipeId) == false
                && gameDataManager.CraftingRecipeDataList.ContainsKey(buildingData.CraftingRecipeId);
            isOk &= AppendCheck(reportBuilder, buildingData.Id + ".CraftingRecipeId", hasRecipe, buildingData.CraftingRecipeId);

            if (string.Equals(buildingData.BuildingType, "Tower", System.StringComparison.OrdinalIgnoreCase))
            {
                isOk &= AppendCheck(reportBuilder, buildingData.Id + ".AttackDamage", buildingData.AttackDamage > 0f, buildingData.AttackDamage.ToString());
                isOk &= AppendCheck(reportBuilder, buildingData.Id + ".AttackRange", buildingData.AttackRange > 0f, buildingData.AttackRange.ToString());
            }

            if (string.Equals(buildingData.BuildingType, "Trap", System.StringComparison.OrdinalIgnoreCase))
            {
                isOk &= AppendCheck(reportBuilder, buildingData.Id + ".AttackInterval", buildingData.AttackInterval > 0f, buildingData.AttackInterval.ToString());
            }
        }

        isOk &= AppendCheck(reportBuilder, "wall count", wallCount > 0, wallCount.ToString());
        isOk &= AppendCheck(reportBuilder, "tower count", towerCount > 0, towerCount.ToString());
        isOk &= AppendCheck(reportBuilder, "trap count", trapCount > 0, trapCount.ToString());
        isOk &= AppendCheck(reportBuilder, "station count", stationCount > 0, stationCount.ToString());
        return isOk;
    }

    private static bool ValidateHarvestNodes(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[Harvest Nodes]");
        bool isOk = true;
        foreach (KeyValuePair<string, HarvestNodeData> pair in gameDataManager.HarvestNodeDataList)
        {
            HarvestNodeData nodeData = pair.Value;
            if (nodeData == null)
            {
                isOk &= AppendCheck(reportBuilder, pair.Key, false, "node is null");
                continue;
            }

            isOk &= AppendCheck(reportBuilder, nodeData.Id + ".ResourceId", gameDataManager.ResourceDataList.ContainsKey(nodeData.ResourceId), nodeData.ResourceId);
            isOk &= AppendCheck(reportBuilder, nodeData.Id + ".RequiredToolType", string.IsNullOrEmpty(nodeData.RequiredToolType) == false, nodeData.RequiredToolType);
            isOk &= AppendCheck(reportBuilder, nodeData.Id + ".RequiredToolTier", nodeData.RequiredToolTier >= 0, nodeData.RequiredToolTier.ToString());
            isOk &= AppendCheck(reportBuilder, nodeData.Id + ".GatherAmount", nodeData.GatherAmount > 0, nodeData.GatherAmount.ToString());
        }

        return isOk;
    }

    private static bool ValidateCinderHeartSkills(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[CinderHeart Skills]");
        bool isOk = true;
        int implementedCount = 0;
        int roadmapCount = 0;

        foreach (KeyValuePair<string, CinderHeartSkillData> pair in gameDataManager.CinderHeartSkillDataList)
        {
            CinderHeartSkillData skillData = pair.Value;
            if (skillData == null)
            {
                isOk &= AppendCheck(reportBuilder, pair.Key, false, "skill is null");
                continue;
            }

            bool hasEffectType = string.IsNullOrEmpty(skillData.EffectType) == false;
            isOk &= AppendCheck(reportBuilder, skillData.Id + ".EffectType", hasEffectType, skillData.EffectType);
            isOk &= AppendCheck(reportBuilder, skillData.Id + ".Weight", skillData.Weight > 0, skillData.Weight.ToString());

            if (GameDataCheckRules.IsImplementedCinderHeartRewardEffect(skillData.EffectType))
            {
                implementedCount++;
                reportBuilder.AppendLine("[Live] " + skillData.Id + " - " + skillData.EffectType);
            }
            else
            {
                roadmapCount++;
                reportBuilder.AppendLine("[Roadmap] " + skillData.Id + " - " + skillData.EffectType);
            }
        }

        isOk &= AppendCheck(reportBuilder, "Implemented reward effects", implementedCount > 0, implementedCount.ToString());
        reportBuilder.AppendLine("Roadmap reward candidates: " + roadmapCount);
        return isOk;
    }

    private static bool ValidateRequiredStarterData(GameDataManager gameDataManager, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("[Required Starter Data]");
        bool isOk = true;
        isOk &= AppendCheck(reportBuilder, "tool: hand_stone", gameDataManager.ToolDataList.ContainsKey(PlayerToolController.HandStoneToolDataId), PlayerToolController.HandStoneToolDataId);
        isOk &= AppendCheck(reportBuilder, "boss count", gameDataManager.BossDataList.Count > 0, gameDataManager.BossDataList.Count.ToString());
        isOk &= AppendCheck(reportBuilder, "game flow phase count", gameDataManager.GameFlowPhaseDataList.Count >= 4, gameDataManager.GameFlowPhaseDataList.Count.ToString());
        return isOk;
    }

    private static bool AppendCount(StringBuilder reportBuilder, string label, int count, int minimumCount)
    {
        return AppendCheck(reportBuilder, label, count >= minimumCount, count.ToString());
    }

    private static bool AppendCheck(StringBuilder reportBuilder, string label, bool isOk, string detail)
    {
        reportBuilder.AppendLine((isOk ? "[OK] " : "[Issue] ") + label + " - " + detail);
        return isOk;
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
}
