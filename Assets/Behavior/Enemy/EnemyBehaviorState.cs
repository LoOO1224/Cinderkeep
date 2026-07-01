using System;
using UnityEngine;

public enum BTEnemyMode
{
    DayWander,
    NightAssault
}

// Behavior Graph 액션들이 읽는 적의 큰 행동 상태입니다.
public sealed class EnemyBehaviorState : MonoBehaviour
{
    [Header("Behavior Mode")]
    [Tooltip("현재 Enemy의 큰 행동 모드입니다. 낮에는 DayWander, 밤에는 NightAssault를 사용합니다.")]
    [SerializeField] private BTEnemyMode _mode = BTEnemyMode.DayWander;

    public BTEnemyMode Mode
    {
        get
        {
            return _mode;
        }
    }

    public void SetMode(BTEnemyMode mode)
    {
        _mode = mode;
    }

    public void SetDayWanderMode()
    {
        SetMode(BTEnemyMode.DayWander);
    }

    public void SetNightAssaultMode()
    {
        SetMode(BTEnemyMode.NightAssault);
    }

    public bool IsCurrentState(string requiredStateName)
    {
        if (string.IsNullOrWhiteSpace(requiredStateName))
        {
            return true;
        }

        BTEnemyMode requiredMode;
        bool canParse = Enum.TryParse(requiredStateName, true, out requiredMode);
        if (canParse == false)
        {
            Debug.LogWarning("EnemyBehaviorState: 알 수 없는 상태 이름입니다. requiredStateName=" + requiredStateName);
            return false;
        }

        return _mode == requiredMode;
    }

#if UNITY_EDITOR
    [ContextMenu("Set Mode/Day Wander")]
    private void DebugSetDayWanderMode()
    {
        SetDayWanderMode();
    }

    [ContextMenu("Set Mode/Night Assault")]
    private void DebugSetNightAssaultMode()
    {
        SetNightAssaultMode();
    }
#endif
}
