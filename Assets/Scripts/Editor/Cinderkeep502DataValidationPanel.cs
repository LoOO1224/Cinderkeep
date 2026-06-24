using System.Collections.Generic;
using System.IO;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEngine;

// JSON 데이터가 현재 구현된 게임 규칙과 맞는지 검사하는 에디터 도구입니다.
// 구현되지 않은 데이터는 로드맵 후보로 보고하되, 실제 플레이 후보와 구분합니다.
public static class Cinderkeep502DataValidationPanel
{
    private const string MenuRoot = "Cinderkeep/5.02 Data Validation/";
    private const string ReportPath = "Temp/Cinderkeep_5_02_DataValidationReport.txt";

    [MenuItem(MenuRoot + "Run Full 5.02 Data Validation")]
    public static void RunFullDataValidation()
    {
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunValidation(reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.02] Data validation passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.02] Data validation found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Generate Data Validation Report")]
    public static void GenerateDataValidationReport()
    {
        StringBuilder reportBuilder = new StringBuilder();
        RunValidation(reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.02] Data validation report generated: " + ReportPath);
    }

    private static bool RunValidation(StringBuilder reportBuilder)
    {
        GameObject tempObject = new GameObject("Temp_5_02_GameDataManager_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            bool isOk = true;
            reportBuilder.AppendLine("[5.02 Data Validation]");
            isOk &= ValidateCatalogCounts(gameDataManager, reportBuilder);
            isOk &= ValidateCraftingRecipes(gameDataManager, reportBuilder);
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
                GameDataValidationRules.IsSupportedCraftingRecipeResultType(recipeData.ResultDataType),
                recipeData.ResultDataType);

            isOk &= ValidateRecipeResult(gameDataManager, reportBuilder, recipeData);
            isOk &= ValidateRecipeCosts(gameDataManager, reportBuilder, recipeData);
        }

        return isOk;
    }

    private static bool ValidateRecipeResult(GameDataManager gameDataManager, StringBuilder reportBuilder, CraftingRecipeData recipeData)
    {
        if (string.IsNullOrEmpty(recipeData.ResultItemId))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", false, "empty");
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeResource, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ResourceDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeTool, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ToolDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeWeapon, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.WeaponDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeArmor, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.ArmorDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeBuilding, System.StringComparison.OrdinalIgnoreCase))
        {
            return AppendCheck(reportBuilder, recipeData.Id + ".ResultItemId", gameDataManager.BuildingDataList.ContainsKey(recipeData.ResultItemId), recipeData.ResultItemId);
        }

        if (string.Equals(recipeData.ResultDataType, GameDataValidationRules.RecipeResultTypeCinderHeartUpgrade, System.StringComparison.OrdinalIgnoreCase))
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

            if (GameDataValidationRules.IsImplementedCinderHeartRewardEffect(skillData.EffectType))
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
