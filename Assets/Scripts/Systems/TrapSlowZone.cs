using System.Collections.Generic;
using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart 주변에 설치되는 감속 함정입니다.
// 적이 범위에 들어오면 일정 시간 느려지고, Run Result에 이동 방해 점수를 보고합니다.
public sealed class TrapSlowZone : MonoBehaviour
{
    [Tooltip("적 이동속도에 곱할 배율입니다. 낮을수록 강한 감속입니다.")]
    [SerializeField] private float _speedMultiplier = 0.5f;
    [Tooltip("감속이 유지되는 시간입니다.")]
    [SerializeField] private float _slowDurationSeconds = 2.5f;
    [Tooltip("같은 적에게 다시 감속을 적용하기 전 기다리는 시간입니다.")]
    [SerializeField] private float _reapplyCooldownSeconds = 1.2f;
    [SerializeField] private TrapCrowdControlReporter _crowdControlReporter;

    private readonly Dictionary<EnemyMovement, float> _lastAppliedTimes = new Dictionary<EnemyMovement, float>();

    private void Awake()
    {
        ConnectReporter();
    }

    public void Initialize(BuildingData buildingData)
    {
        if (buildingData == null)
        {
            return;
        }

        int tier = Mathf.Clamp(buildingData.Tier, 1, 4);
        _speedMultiplier = Mathf.Clamp(0.75f - (tier * 0.1f), 0.35f, 0.75f);
        _slowDurationSeconds = 1.5f + (tier * 0.5f);

        if (buildingData.AttackInterval > 0f)
        {
            _reapplyCooldownSeconds = buildingData.AttackInterval;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        EnemyMovement enemyMovement = other == null ? null : other.GetComponentInParent<EnemyMovement>();
        if (enemyMovement == null)
        {
            return;
        }

        if (CanApplySlow(enemyMovement) == false)
        {
            return;
        }

        enemyMovement.ApplyMoveSpeedMultiplier(_speedMultiplier, _slowDurationSeconds);
        _lastAppliedTimes[enemyMovement] = Time.time;
        ReportCrowdControlScore();
    }

    private bool CanApplySlow(EnemyMovement enemyMovement)
    {
        if (enemyMovement == null)
        {
            return false;
        }

        float lastAppliedTime;
        if (_lastAppliedTimes.TryGetValue(enemyMovement, out lastAppliedTime) == false)
        {
            return true;
        }

        return Time.time >= lastAppliedTime + _reapplyCooldownSeconds;
    }

    private void ReportCrowdControlScore()
    {
        ConnectReporter();
        if (_crowdControlReporter == null)
        {
            return;
        }

        float slowScore = 1f + Mathf.Clamp01(1f - _speedMultiplier);
        _crowdControlReporter.ReportScore(slowScore);
    }

    private void ConnectReporter()
    {
        if (_crowdControlReporter != null)
        {
            return;
        }

        _crowdControlReporter = GetComponent<TrapCrowdControlReporter>();
        if (_crowdControlReporter == null)
        {
            _crowdControlReporter = gameObject.AddComponent<TrapCrowdControlReporter>();
        }
    }
}
