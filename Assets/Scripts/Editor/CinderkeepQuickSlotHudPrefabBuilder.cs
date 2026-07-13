using Cinderkeep.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 항상 표시되는 1~7 퀵슬롯 HUD를 프리팹으로 만들고 메인 게임 씬에 연결합니다.
// 런타임 즉석 생성은 사용하지 않으며 팀원이 프리팹에서 배치와 이미지를 수정할 수 있게 합니다.
public static class CinderkeepQuickSlotHudPrefabBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string QuickSlotHudPrefabPath = "Assets/Prefabs/UI/QuickSlotHud.prefab";

    [MenuItem("Cinderkeep/UI/Build And Connect Quick Slot HUD")]
    public static void BuildAndConnectQuickSlotHud()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
        QuickSlotHud quickSlotHud = Object.FindFirstObjectByType<QuickSlotHud>(
            FindObjectsInactive.Include);

        if (quickSlotHud == null)
        {
            quickSlotHud = CreateQuickSlotHud();
        }

        if (quickSlotHud == null)
        {
            Debug.LogError("[CinderkeepQuickSlotHudPrefabBuilder] 퀵슬롯 HUD를 생성하지 못했습니다.");
            return;
        }

        quickSlotHud.gameObject.SetActive(true);
        PrefabUtility.SaveAsPrefabAssetAndConnect(
            quickSlotHud.gameObject,
            QuickSlotHudPrefabPath,
            InteractionMode.AutomatedAction);

        EditorUtility.SetDirty(quickSlotHud);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[CinderkeepQuickSlotHudPrefabBuilder] 퀵슬롯 HUD 프리팹과 씬 연결을 완료했습니다.");
    }

    private static QuickSlotHud CreateQuickSlotHud()
    {
        bool previousPolicy = HudRuntimeCreationPolicy.IsRuntimeCreationEnabled;
        HudRuntimeCreationPolicy.IsRuntimeCreationEnabled = true;

        try
        {
            return QuickSlotHud.EnsureSceneHud();
        }
        finally
        {
            HudRuntimeCreationPolicy.IsRuntimeCreationEnabled = previousPolicy;
        }
    }
}
