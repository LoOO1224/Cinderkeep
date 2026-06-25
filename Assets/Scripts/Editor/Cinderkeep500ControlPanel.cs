using System.Collections.Generic;
using System.IO;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEngine;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
public static class Cinderkeep500ControlPanel
{
    private const string MenuRoot = "Cinderkeep/5.00 Control Panel/";
    private const string ReportPath = "Temp/Cinderkeep_5_00_CheckReport.txt";

    [MenuItem(MenuRoot + "Run Full 5.00 Check")]
    public static void RunFullCheck()
    {
        StringBuilder reportBuilder = new StringBuilder();
        bool isDataOk = RunDataJsonCheckInternal(reportBuilder);
        bool isSceneOk = RunSceneWiringCheckInternal(reportBuilder);
        bool isOk = isDataOk && isSceneOk;
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.00] Full check passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.00] Full check found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Data JSON Check")]
    public static void RunDataJsonCheck()
    {
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunDataJsonCheckInternal(reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.00] Data JSON check passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.00] Data JSON check found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Scene Wiring Check")]
    public static void RunSceneWiringCheck()
    {
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunSceneWiringCheckInternal(reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.00] Scene wiring check passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.00] Scene wiring check found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Debug Give Basic Resources")]
    public static void DebugGiveBasicResources()
    {
        if (Application.isPlaying == false)
        {
            Debug.LogWarning("[Cinderkeep 5.00] Enter Play Mode before giving resources.");
            return;
        }

        if (GameManager.Inst == null || GameManager.Inst.PlayerModel == null)
        {
            Debug.LogWarning("[Cinderkeep 5.00] GameManager or PlayerModel is missing.");
            return;
        }

        GameManager.Inst.PlayerModel.AddResource(PlayerModel.ResourceWood, 50);
        GameManager.Inst.PlayerModel.AddResource(PlayerModel.ResourceStone, 50);
        GameManager.Inst.PlayerModel.AddResource(PlayerModel.ResourceIron, 20);
        GameManager.Inst.PlayerModel.AddResource(PlayerModel.ResourceGold, 10);
        Debug.Log("[Cinderkeep 5.00] Debug resources added.");
    }

    [MenuItem(MenuRoot + "Generate Check Report")]
    public static void GenerateCheckReport()
    {
        StringBuilder reportBuilder = new StringBuilder();
        RunDataJsonCheckInternal(reportBuilder);
        RunSceneWiringCheckInternal(reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.00] Check report generated: " + ReportPath);
    }

    private static bool RunDataJsonCheckInternal(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Data JSON Check]");
        bool isOk = true;

        GameObject tempObject = new GameObject("Temp_5_00_GameDataManager_Check");
        GameDataManager gameDataManager = tempObject.AddComponent<GameDataManager>();
        try
        {
            gameDataManager.Initialize();
            isOk &= AppendCount(reportBuilder, "enemies", gameDataManager.EnemyDataList);
            isOk &= AppendCount(reportBuilder, "resources", gameDataManager.ResourceDataList);
            isOk &= AppendCount(reportBuilder, "harvest_nodes", gameDataManager.HarvestNodeDataList);
            isOk &= AppendCount(reportBuilder, "tools", gameDataManager.ToolDataList);
            isOk &= AppendCount(reportBuilder, "weapons", gameDataManager.WeaponDataList);
            isOk &= AppendCount(reportBuilder, "armors", gameDataManager.ArmorDataList);
            isOk &= AppendCount(reportBuilder, "buildings", gameDataManager.BuildingDataList);
            isOk &= AppendCount(reportBuilder, "crafting_recipes", gameDataManager.CraftingRecipeDataList);
            isOk &= AppendCount(reportBuilder, "enemy_spawn_rules", gameDataManager.EnemySpawnRuleDataList);
            isOk &= AppendCount(reportBuilder, "game_flow_phases", gameDataManager.GameFlowPhaseDataList);
            isOk &= AppendCount(reportBuilder, "cinderheart_skills", gameDataManager.CinderHeartSkillDataList);
            isOk &= AppendCount(reportBuilder, "bosses", gameDataManager.BossDataList);
        }
        finally
        {
            Object.DestroyImmediate(tempObject);
        }

        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool RunSceneWiringCheckInternal(StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[Scene Wiring Check]");
        bool isOk = true;

        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        isOk &= AppendObjectCheck(reportBuilder, "GameManager", gameManager);
        if (gameManager != null)
        {
            isOk &= AppendObjectCheck(reportBuilder, "GameDataManager", gameManager.GetGameDataManager());
            isOk &= AppendObjectCheck(reportBuilder, "UIManager", gameManager.GetUIManager());
            isOk &= AppendObjectCheck(reportBuilder, "GameFlowController", gameManager.GetGameFlowController());
            isOk &= AppendObjectCheck(reportBuilder, "GameObjectManager", gameManager.GetGameObjectManager());
            isOk &= AppendObjectCheck(reportBuilder, "BuildingManager", gameManager.GetBuildingManager());
            isOk &= AppendObjectCheck(reportBuilder, "SoundManager", gameManager.GetSoundManager());
        }

        isOk &= AppendObjectCheck(reportBuilder, "PlayerStatus", Object.FindFirstObjectByType<PlayerStatus>());
        isOk &= AppendObjectCheck(reportBuilder, "CinderHeart", Object.FindFirstObjectByType<CinderHeart>());
        isOk &= AppendObjectCheck(reportBuilder, "EnemySpawnPoint", Object.FindFirstObjectByType<EnemySpawnPoint>());
        isOk &= AppendObjectCheck(reportBuilder, "BuildingSpot", Object.FindFirstObjectByType<BuildingSpot>());
        isOk &= AppendObjectCheck(reportBuilder, "ResourceNode", Object.FindFirstObjectByType<ResourceNode>());

        reportBuilder.AppendLine();
        return isOk;
    }

    private static bool AppendCount<TValue>(
        StringBuilder reportBuilder,
        string label,
        IReadOnlyDictionary<string, TValue> dataList)
    {
        int count = dataList == null ? 0 : dataList.Count;
        bool isOk = count > 0;
        reportBuilder.AppendLine(FormatCheckLine(label, isOk, count.ToString()));
        return isOk;
    }

    private static bool AppendObjectCheck(StringBuilder reportBuilder, string label, Object targetObject)
    {
        bool isOk = targetObject != null;
        reportBuilder.AppendLine(FormatCheckLine(label, isOk, isOk ? targetObject.name : "missing"));
        return isOk;
    }

    private static string FormatCheckLine(string label, bool isOk, string detail)
    {
        return (isOk ? "[OK] " : "[Missing] ") + label + " - " + detail;
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
