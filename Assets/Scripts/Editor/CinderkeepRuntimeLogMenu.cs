using UnityEditor;

// 에디터에서 반복 런타임 로그를 필요할 때만 켜고 끄는 개발 보조 메뉴입니다.
// 실제 게임 빌드에는 포함되지 않으며, 콘솔 노이즈를 줄이는 용도로만 사용합니다.
public static class CinderkeepRuntimeLogMenu
{
    private const string MenuRoot = "Cinderkeep/Runtime Logs/";
    private const string VerboseRuntimeLogsMenu = MenuRoot + "Verbose Runtime Logs";

    [MenuItem(VerboseRuntimeLogsMenu)]
    public static void ToggleVerboseRuntimeLogs()
    {
        CinderkeepLog.EnableVerboseRuntimeLogs = CinderkeepLog.EnableVerboseRuntimeLogs == false;
        Menu.SetChecked(VerboseRuntimeLogsMenu, CinderkeepLog.EnableVerboseRuntimeLogs);
        string stateText = CinderkeepLog.EnableVerboseRuntimeLogs ? "enabled" : "disabled";
        UnityEngine.Debug.Log("Cinderkeep verbose runtime logs " + stateText + ".");
    }

    [MenuItem(VerboseRuntimeLogsMenu, true)]
    public static bool ValidateVerboseRuntimeLogs()
    {
        Menu.SetChecked(VerboseRuntimeLogsMenu, CinderkeepLog.EnableVerboseRuntimeLogs);
        return true;
    }
}
