using System.Collections.Generic;
using System.IO;
using System.Text;
using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Cinderkeep_Game 씬의 필수 참조 연결 상태를 검사하고, 안전한 누락 참조만 자동 보정하는 에디터 도구입니다.
// 데이터/전투 Check은 다른 패널에서 처리하고, 이 패널은 씬과 프리팹 연결 확인에만 집중합니다.
public static class Cinderkeep501ControlPanel
{
    private const string MenuRoot = "Cinderkeep/5.01 Control Panel/";
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string ReportPath = "Temp/Cinderkeep_5_01_SceneWiringReport.txt";

    [MenuItem(MenuRoot + "Run Full 5.01 Check")]
    public static void RunFullCheck()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunSceneWiringAudit(scene, reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.01] Full check passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.01] Full check found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Scene Wiring Audit")]
    public static void RunSceneWiringAudit()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        bool isOk = RunSceneWiringAudit(scene, reportBuilder);
        WriteReport(reportBuilder);

        if (isOk)
        {
            Debug.Log("[Cinderkeep 5.01] Scene wiring audit passed.\n" + reportBuilder);
            return;
        }

        Debug.LogWarning("[Cinderkeep 5.01] Scene wiring audit found issues.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Apply Safe Scene Wiring Fixes")]
    public static void ApplySafeSceneWiringFixes()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        int fixedCount = 0;

        reportBuilder.AppendLine("[5.01 Safe Scene Wiring Fixes]");
        ApplyManagerReferences(scene, reportBuilder, ref fixedCount);
        ApplyLoopConnectorReferences(scene, reportBuilder, ref fixedCount);
        ApplyUiReferences(scene, reportBuilder, ref fixedCount);
        ApplyCinderHeartRewardReferences(scene, reportBuilder, ref fixedCount);
        ApplyEnemySpawnReferences(scene, reportBuilder, ref fixedCount);
        reportBuilder.AppendLine("Fixed references: " + fixedCount);
        reportBuilder.AppendLine();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RunSceneWiringAudit(scene, reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.01] Safe scene wiring fixes applied.\n" + reportBuilder);
    }

    [MenuItem(MenuRoot + "Generate 5.01 Scene Wiring Report")]
    public static void GenerateSceneWiringReport()
    {
        Scene scene = OpenTargetScene();
        StringBuilder reportBuilder = new StringBuilder();
        RunSceneWiringAudit(scene, reportBuilder);
        WriteReport(reportBuilder);
        Debug.Log("[Cinderkeep 5.01] Scene wiring report generated: " + ReportPath);
    }

    private static Scene OpenTargetScene()
    {
        if (Application.isPlaying)
        {
            return SceneManager.GetActiveScene();
        }

        return EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
    }

    private static bool RunSceneWiringAudit(Scene scene, StringBuilder reportBuilder)
    {
        reportBuilder.AppendLine("[5.01 Scene Wiring Audit]");
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
        UIManager uiManager = FindComponentByName<UIManager>(scene, "UIManager");
        GameFlowController gameFlowController = FindComponentByName<GameFlowController>(scene, "GameFlowController");
        GameFlowEnemySpawnDirector enemySpawnDirector = FindComponentByName<GameFlowEnemySpawnDirector>(scene, "GameFlowEnemySpawnDirector");
        CinderHeartSkillSelectionUI skillSelectionUI = FindComponentByName<CinderHeartSkillSelectionUI>(scene, "Panel_CinderHeartSkillSelection");
        CinderHeartSkillApplier skillApplier = FindComponentByName<CinderHeartSkillApplier>(scene, "Panel_CinderHeartSkillSelection");

        isOk &= AppendComponentCheck(reportBuilder, "GameManager", gameManager);
        isOk &= AppendComponentCheck(reportBuilder, "GameDataManager", FindComponentByName<GameDataManager>(scene, "GameDataManager"));
        isOk &= AppendComponentCheck(reportBuilder, "ResourceManager", FindComponentByName<ResourceManager>(scene, "ResourceManager"));
        isOk &= AppendComponentCheck(reportBuilder, "GameObjectManager", FindComponentByName<GameObjectManager>(scene, "GameObjectManager"));
        isOk &= AppendComponentCheck(reportBuilder, "BuildingManager", FindComponentByName<BuildingManager>(scene, "BuildingManager"));
        isOk &= AppendComponentCheck(reportBuilder, "UIManager", uiManager);
        isOk &= AppendComponentCheck(reportBuilder, "SoundManager", FindComponentByName<SoundManager>(scene, "SoundManager"));
        isOk &= AppendComponentCheck(reportBuilder, "MapManager", FindComponentByName<MapManager>(scene, "MapManager"));
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowController", gameFlowController);
        isOk &= AppendComponentCheck(reportBuilder, "GameFlowEnemySpawnDirector", enemySpawnDirector);

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

        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_hudRoot");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_inventoryRoot");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_gameOverPanel");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_runResultText");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_inventoryUI");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, uiManager, "_cinderHeartSkillSelectionUI");

        isOk &= AppendSerializedReferenceCheck(reportBuilder, skillSelectionUI, "_rootObject");
        isOk &= AppendSerializedArrayCheck(reportBuilder, skillSelectionUI, "_optionViews", 3);
        isOk &= AppendSerializedReferenceCheck(reportBuilder, skillSelectionUI, "_skillApplier");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, skillApplier, "_cinderHeart");
        isOk &= AppendSerializedReferenceCheck(reportBuilder, skillApplier, "_playerStatus");

        isOk &= AppendComponentCheck(reportBuilder, "PlayerStatus", FindComponentByName<PlayerStatus>(scene, "Player"));
        isOk &= AppendComponentCheck(reportBuilder, "PlayerToolController", FindComponentByName<PlayerToolController>(scene, "Player"));
        isOk &= AppendComponentCheck(reportBuilder, "CinderHeart", FindComponentByName<CinderHeart>(scene, "CinderHeart"));
        isOk &= AppendComponentCountCheck(reportBuilder, "EnemySpawnPoint", FindComponentsInScene<EnemySpawnPoint>(scene).Count, 1);
        isOk &= AppendComponentCountCheck(reportBuilder, "ResourceNode", FindComponentsInScene<ResourceNode>(scene).Count, 1);
        isOk &= AppendComponentCountCheck(reportBuilder, "BuildingSpot", FindComponentsInScene<BuildingSpot>(scene).Count, 1);

        reportBuilder.AppendLine();
        return isOk;
    }

    private static void ApplyManagerReferences(Scene scene, StringBuilder reportBuilder, ref int fixedCount)
    {
        GameManager gameManager = FindComponentByName<GameManager>(scene, "GameManager");
        SetReference(gameManager, "_gameDataManager", FindComponentByName<GameDataManager>(scene, "GameDataManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_resourceManager", FindComponentByName<ResourceManager>(scene, "ResourceManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_gameObjectManager", FindComponentByName<GameObjectManager>(scene, "GameObjectManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_buildingManager", FindComponentByName<BuildingManager>(scene, "BuildingManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_uiManager", FindComponentByName<UIManager>(scene, "UIManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_soundManager", FindComponentByName<SoundManager>(scene, "SoundManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_mapManager", FindComponentByName<MapManager>(scene, "MapManager"), reportBuilder, ref fixedCount);
        SetReference(gameManager, "_gameFlowController", FindComponentByName<GameFlowController>(scene, "GameFlowController"), reportBuilder, ref fixedCount);

        GameObjectManager gameObjectManager = FindComponentByName<GameObjectManager>(scene, "GameObjectManager");
        GameObject runtimeRoot = FindObjectByName(scene, "MainGame_RuntimeObjects");
        SetReference(gameObjectManager, "_objectRoot", runtimeRoot == null ? null : runtimeRoot.transform, reportBuilder, ref fixedCount);
    }

    private static void ApplyLoopConnectorReferences(Scene scene, StringBuilder reportBuilder, ref int fixedCount)
    {
        GameFlowController gameFlowController = FindComponentByName<GameFlowController>(scene, "GameFlowController");
        GameFlowEnemySpawnDirector enemySpawnDirector = FindComponentByName<GameFlowEnemySpawnDirector>(scene, "GameFlowEnemySpawnDirector");
        SetReference(gameFlowController, "_enemySpawnDirector", enemySpawnDirector, reportBuilder, ref fixedCount);

        CinderkeepGameLoopConnector loopConnector = FindComponentByName<CinderkeepGameLoopConnector>(scene, "MainGame_LoopConnector");
        SetReference(loopConnector, "_playerLoopConnector", FindComponentByName<PlayerLoopConnector>(scene, "PlayerLoopConnector"), reportBuilder, ref fixedCount);
        SetReference(loopConnector, "_resourceLoopConnector", FindComponentByName<ResourceLoopConnector>(scene, "ResourceLoopConnector"), reportBuilder, ref fixedCount);
        SetReference(loopConnector, "_enemyLoopConnector", FindComponentByName<EnemyLoopConnector>(scene, "EnemyLoopConnector"), reportBuilder, ref fixedCount);
        SetReference(loopConnector, "_gameFlowLoopConnector", FindComponentByName<GameFlowLoopConnector>(scene, "GameFlowLoopConnector"), reportBuilder, ref fixedCount);

        SetReference(FindComponentByName<PlayerLoopConnector>(scene, "PlayerLoopConnector"), "_playerStatus", FindComponentByName<PlayerStatus>(scene, "Player"), reportBuilder, ref fixedCount);
        SetReference(FindComponentByName<PlayerLoopConnector>(scene, "PlayerLoopConnector"), "_playerHud", FindComponentByName<PlayerHUD>(scene, "Panel_PlayerHUD"), reportBuilder, ref fixedCount);

        SetReference(FindComponentByName<ResourceLoopConnector>(scene, "ResourceLoopConnector"), "_gameManager", FindComponentByName<GameManager>(scene, "GameManager"), reportBuilder, ref fixedCount);
        SetReference(FindComponentByName<ResourceLoopConnector>(scene, "ResourceLoopConnector"), "_resourceUi", FindComponentByName<ResourceUI>(scene, "Panel_ResourceUI"), reportBuilder, ref fixedCount);

        EnemyLoopConnector enemyLoopConnector = FindComponentByName<EnemyLoopConnector>(scene, "EnemyLoopConnector");
        SetReference(enemyLoopConnector, "_gameDataManager", FindComponentByName<GameDataManager>(scene, "GameDataManager"), reportBuilder, ref fixedCount);
        GameObject cinderHeartObject = FindObjectByName(scene, "CinderHeart");
        SetReference(enemyLoopConnector, "_cinderHeartTarget", cinderHeartObject == null ? null : cinderHeartObject.transform, reportBuilder, ref fixedCount);
        SetReference(enemyLoopConnector, "_gameCamera", FindComponentInScene<Camera>(scene), reportBuilder, ref fixedCount);

        SetReference(FindComponentByName<GameFlowLoopConnector>(scene, "GameFlowLoopConnector"), "_gameManager", FindComponentByName<GameManager>(scene, "GameManager"), reportBuilder, ref fixedCount);
        SetReference(FindComponentByName<GameFlowLoopConnector>(scene, "GameFlowLoopConnector"), "_gameObjectManager", FindComponentByName<GameObjectManager>(scene, "GameObjectManager"), reportBuilder, ref fixedCount);
        SetReference(FindComponentByName<GameFlowLoopConnector>(scene, "GameFlowLoopConnector"), "_enemyLoopConnector", enemyLoopConnector, reportBuilder, ref fixedCount);
    }

    private static void ApplyUiReferences(Scene scene, StringBuilder reportBuilder, ref int fixedCount)
    {
        UIManager uiManager = FindComponentByName<UIManager>(scene, "UIManager");
        GameObject hudRoot = FindObjectByName(scene, "Panel_HUDRoot");
        GameObject inventoryRoot = FindObjectByName(scene, "Panel_InventoryRoot");
        GameObject gameOverPanel = FindObjectByName(scene, "Panel_GameOver_Disabled");
        if (gameOverPanel == null)
        {
            gameOverPanel = FindObjectByName(scene, "Panel_GameOver");
        }

        SetReference(uiManager, "_hudRoot", hudRoot, reportBuilder, ref fixedCount);
        SetReference(uiManager, "_inventoryRoot", inventoryRoot, reportBuilder, ref fixedCount);
        SetReference(uiManager, "_gameOverPanel", gameOverPanel, reportBuilder, ref fixedCount);
        SetReference(uiManager, "_runResultText", FindTextInObject(gameOverPanel), reportBuilder, ref fixedCount);
        SetReference(uiManager, "_inventoryUI", FindComponentByName<InventoryUI>(scene, "Panel_InventoryRoot"), reportBuilder, ref fixedCount);
        SetReference(uiManager, "_craftingUI", FindComponentInScene<CraftingUI>(scene), reportBuilder, ref fixedCount);
        SetReference(uiManager, "_furnaceUI", FindComponentInScene<FurnaceUI>(scene), reportBuilder, ref fixedCount);
        SetReference(uiManager, "_cinderHeartSkillSelectionUI", FindComponentByName<CinderHeartSkillSelectionUI>(scene, "Panel_CinderHeartSkillSelection"), reportBuilder, ref fixedCount);
    }

    private static void ApplyCinderHeartRewardReferences(Scene scene, StringBuilder reportBuilder, ref int fixedCount)
    {
        CinderHeart cinderHeart = FindComponentByName<CinderHeart>(scene, "CinderHeart");
        PlayerStatus playerStatus = FindComponentByName<PlayerStatus>(scene, "Player");
        CinderHeartSkillApplier skillApplier = FindComponentByName<CinderHeartSkillApplier>(scene, "Panel_CinderHeartSkillSelection");
        CinderHeartSkillSelectionUI skillSelectionUI = FindComponentByName<CinderHeartSkillSelectionUI>(scene, "Panel_CinderHeartSkillSelection");

        SetReference(skillApplier, "_cinderHeart", cinderHeart, reportBuilder, ref fixedCount);
        SetReference(skillApplier, "_playerStatus", playerStatus, reportBuilder, ref fixedCount);

        GameObject skillRoot = FindObjectByName(scene, "Panel_CinderHeartSkillSelection");
        SetReference(skillSelectionUI, "_rootObject", skillRoot, reportBuilder, ref fixedCount);
        SetReference(skillSelectionUI, "_skillApplier", skillApplier, reportBuilder, ref fixedCount);
    }

    private static void ApplyEnemySpawnReferences(Scene scene, StringBuilder reportBuilder, ref int fixedCount)
    {
        GameFlowEnemySpawnDirector enemySpawnDirector = FindComponentByName<GameFlowEnemySpawnDirector>(scene, "GameFlowEnemySpawnDirector");
        List<EnemySpawnPoint> spawnPoints = FindComponentsInScene<EnemySpawnPoint>(scene);
        SetReferenceArray(enemySpawnDirector, "_enemySpawnPoints", spawnPoints.ToArray(), reportBuilder, ref fixedCount);
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

    private static bool AppendComponentCountCheck(StringBuilder reportBuilder, string label, int count, int minimumCount)
    {
        return AppendCheck(reportBuilder, "Component Count: " + label, count >= minimumCount, count.ToString());
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

    private static bool AppendCheck(StringBuilder reportBuilder, string label, bool isOk, string detail)
    {
        reportBuilder.AppendLine((isOk ? "[OK] " : "[Missing] ") + label + " - " + detail);
        return isOk;
    }

    private static void SetReference(UnityEngine.Object targetObject, string propertyName, UnityEngine.Object referenceValue, StringBuilder reportBuilder, ref int fixedCount)
    {
        if (targetObject == null || referenceValue == null)
        {
            reportBuilder.AppendLine("[Skip] " + propertyName + " - owner or value missing");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            reportBuilder.AppendLine("[Skip] " + targetObject.name + "." + propertyName + " - property missing");
            return;
        }

        if (property.objectReferenceValue == referenceValue)
        {
            reportBuilder.AppendLine("[Keep] " + targetObject.name + "." + propertyName + " - " + referenceValue.name);
            return;
        }

        property.objectReferenceValue = referenceValue;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(targetObject);
        fixedCount++;
        reportBuilder.AppendLine("[Fix] " + targetObject.name + "." + propertyName + " -> " + referenceValue.name);
    }

    private static void SetReferenceArray<TComponent>(UnityEngine.Object targetObject, string propertyName, TComponent[] referenceValues, StringBuilder reportBuilder, ref int fixedCount)
        where TComponent : UnityEngine.Object
    {
        if (targetObject == null || referenceValues == null || referenceValues.Length == 0)
        {
            reportBuilder.AppendLine("[Skip] " + propertyName + " - owner or values missing");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(targetObject);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || property.isArray == false)
        {
            reportBuilder.AppendLine("[Skip] " + targetObject.name + "." + propertyName + " - property missing");
            return;
        }

        bool isSame = property.arraySize == referenceValues.Length;
        if (isSame)
        {
            for (int i = 0; i < referenceValues.Length; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != referenceValues[i])
                {
                    isSame = false;
                    break;
                }
            }
        }

        if (isSame)
        {
            reportBuilder.AppendLine("[Keep] " + targetObject.name + "." + propertyName + " - " + referenceValues.Length);
            return;
        }

        property.arraySize = referenceValues.Length;
        for (int i = 0; i < referenceValues.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = referenceValues[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(targetObject);
        fixedCount++;
        reportBuilder.AppendLine("[Fix] " + targetObject.name + "." + propertyName + " -> " + referenceValues.Length);
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

    private static Text FindTextInObject(GameObject rootObject)
    {
        if (rootObject == null)
        {
            return null;
        }

        return rootObject.GetComponentInChildren<Text>(true);
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
