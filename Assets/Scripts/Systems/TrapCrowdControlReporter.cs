using System;
using UnityEngine;

// 함정이 적을 느리게 하거나 멈춘 기여도를 Run Result에 보고하는 연결 컴포넌트입니다.
// 실제 감속/기절 동작은 함정별 스크립트가 처리하고, 이 클래스는 점수 보고만 담당합니다.
public sealed class TrapCrowdControlReporter : MonoBehaviour
{
    public static event Action<float> TrapCrowdControlScoredGlobal;

    [Tooltip("함정 방해 점수를 Run Result에 더할 때 곱하는 배율입니다.")]
    [SerializeField] private float _scoreMultiplier = 1f;

    public void ReportScore(float score)
    {
        float finalScore = Mathf.Max(0f, score) * Mathf.Max(0f, _scoreMultiplier);
        if (finalScore <= 0f)
        {
            return;
        }

        NotifyTrapCrowdControlScored(finalScore);
    }

    public void ReportSlow(float normalTravelSeconds, float slowedTravelSeconds)
    {
        if (normalTravelSeconds <= 0f || slowedTravelSeconds <= normalTravelSeconds)
        {
            return;
        }

        float score = slowedTravelSeconds / normalTravelSeconds;
        ReportScore(score);
    }

    public void ReportStun(float stunSeconds)
    {
        ReportScore(stunSeconds);
    }

    private void NotifyTrapCrowdControlScored(float score)
    {
        if (TrapCrowdControlScoredGlobal == null)
        {
            return;
        }

        TrapCrowdControlScoredGlobal(score);
    }
}
