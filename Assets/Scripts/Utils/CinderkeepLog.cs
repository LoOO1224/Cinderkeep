using UnityEngine;

// 반복 런타임 로그를 한 곳에서 제어합니다.
// 기본 플레이에서는 꺼두고, 에디터에서 필요할 때만 Verbose 로그를 켭니다.
public static class CinderkeepLog
{
    public static bool EnableVerboseRuntimeLogs { get; set; }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Verbose(string message)
    {
#if UNITY_EDITOR
        if (EnableVerboseRuntimeLogs == false)
        {
            return;
        }

        Debug.Log(message);
#endif
    }
}
