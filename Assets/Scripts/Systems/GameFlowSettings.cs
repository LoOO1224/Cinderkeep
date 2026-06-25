using UnityEngine;

// GameFlowController가 JSON 데이터를 못 읽을 때 사용하는 Inspector fallback 시간 설정입니다.
// 실제 일반/테스트초고속 모드 시간은 GameLaunchSettings와 game_mode_settings.json을 우선 사용합니다.
[System.Serializable]
public sealed class GameFlowSettings
{
    [Tooltip("낮 페이즈 fallback 시간입니다. 180이면 3분입니다.")]
    [SerializeField] private float _dayDuration = 180f;

    [Tooltip("밤 페이즈 fallback 시간입니다. 120이면 2분입니다.")]
    [SerializeField] private float _nightDuration = 120f;

    [Tooltip("밤이 끝난 뒤 보상 선택과 아침 정비에 사용하는 fallback 시간입니다.")]
    [SerializeField] private float _morningRewardDuration = 30f;

    [Tooltip("마지막 밤 이후 보스 접근 페이즈에 사용하는 fallback 시간입니다.")]
    [SerializeField] private float _bossApproachDuration = 180f;

    public float DayDuration
    {
        get
        {
            return _dayDuration;
        }
    }

    public float NightDuration
    {
        get
        {
            return _nightDuration;
        }
    }

    public float MorningRewardDuration
    {
        get
        {
            return _morningRewardDuration;
        }
    }

    public float BossApproachDuration
    {
        get
        {
            return _bossApproachDuration;
        }
    }

    public void ClampValues()
    {
        _dayDuration = Mathf.Max(1f, _dayDuration);
        _nightDuration = Mathf.Max(1f, _nightDuration);
        _morningRewardDuration = Mathf.Max(1f, _morningRewardDuration);
        _bossApproachDuration = Mathf.Max(1f, _bossApproachDuration);
    }
}
