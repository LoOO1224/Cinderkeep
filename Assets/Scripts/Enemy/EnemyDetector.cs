using UnityEngine;
using Cinderkeep.Gameplay;

public sealed class EnemyDetector : MonoBehaviour
{
    [SerializeField] private float _viewAngle = 90f;

    private const string PlayerTag = "Player";
    private const int MaxOverlapCount = 20;

    private readonly Collider[] _overlapColliders = new Collider[MaxOverlapCount];

    private float _detectorDistance;

    public Transform DetectedPlayer { get; private set; }

    public bool HasDetectedPlayer
    {
        get
        {
            return DetectedPlayer != null;
        }
    }

    private void Update()
    {
        DetectPlayer();
    }

    public void Initialize(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            return;
        }

        _detectorDistance = enemyData.DetectorDistance;
    }

    public void EnableAlertMode()
    {
        if (HasDetectedPlayer)
        {
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = _overlapColliders[i];
            if (IsPlayerTarget(targetCollider))
            {
                DetectedPlayer = targetCollider.transform;
                Debug.Log(gameObject.name + ": 피격으로 플레이어를 감지했습니다.");
                return;
            }
        }

        Debug.Log(gameObject.name + ": 피격되었지만 감지 범위 안에 플레이어가 없습니다.");
    }

    private void DetectPlayer()
    {
        if (HasDetectedPlayer)
        {
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider targetCollider = _overlapColliders[i];
            if (!IsPlayerTarget(targetCollider))
            {
                continue;
            }

            if (IsInViewAngle(targetCollider.transform))
            {
                DetectedPlayer = targetCollider.transform;
                return;
            }
        }
    }

    private bool IsPlayerTarget(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.transform == transform)
        {
            return false;
        }

        return targetCollider.CompareTag(PlayerTag);
    }

    private bool IsInViewAngle(Transform targetTransform)
    {
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= _viewAngle * 0.5f;
    }
}
