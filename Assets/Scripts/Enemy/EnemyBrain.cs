using UnityEngine;

// 몬스터가 어떤 대상을 공격할지 판단하는 컴포넌트입니다.
// 이동은 EnemyMovement, 감지는 EnemyDetector, 피해 적용은 EnemyAttack이 담당합니다.
public sealed class EnemyBrain : MonoBehaviour
{
    private const string PlayerTag = "Player";
    private const string BuildTag = "Build";
    private const string CinderHeartTag = "CinderHeart";

    [Tooltip("플레이어 감지 결과를 가져오는 컴포넌트입니다.")]
    [SerializeField] private EnemyDetector _enemyDetector;
    [Tooltip("실제 피해 적용과 공격 쿨타임을 담당하는 컴포넌트입니다.")]
    [SerializeField] private EnemyAttack _enemyAttack;
    [Tooltip("플레이어를 감지하지 못했을 때 공격할 CinderHeart 피해 대상입니다.")]
    [SerializeField] private Damageable _cinderHeartDamageable;
    [Tooltip("플레이어와 구조물을 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _attackDistance = 2.3f;
    [Tooltip("CinderHeart를 공격할 수 있는 거리입니다.")]
    [SerializeField] private float _cinderHeartAttackDistance = 3f;

    private Damageable _currentAttackTarget;

    private void Awake()
    {
        ConnectComponents();
    }

    private void Update()
    {
        UpdatePlayerTargetFromDetector();
        UpdateCinderHeartTargetIfNeeded();
        TryAttackCurrentTarget();
    }

    public void SetCinderHeartTarget(Damageable cinderHeartDamageable)
    {
        _cinderHeartDamageable = cinderHeartDamageable;
    }

    // 벽이나 구조물 공격이 필요할 때 외부에서 공격 대상을 지정하는 진입점입니다.
    public void SetAttackTarget(Damageable targetDamageable)
    {
        _currentAttackTarget = targetDamageable;
    }

    public void ClearAttackTarget(Damageable targetDamageable)
    {
        if (_currentAttackTarget != targetDamageable)
        {
            return;
        }

        _currentAttackTarget = null;
    }

    private void ConnectComponents()
    {
        if (_enemyDetector == null)
        {
            _enemyDetector = GetComponent<EnemyDetector>();
        }

        if (_enemyAttack == null)
        {
            _enemyAttack = GetComponent<EnemyAttack>();
        }
    }

    private void UpdatePlayerTargetFromDetector()
    {
        if (_enemyDetector == null)
        {
            return;
        }

        if (_enemyDetector.HasDetectedPlayer == false)
        {
            ClearPlayerAttackTarget();
            return;
        }

        Damageable detectedPlayerDamageable = GetDamageableFromTransform(_enemyDetector.DetectedPlayer);
        if (detectedPlayerDamageable == null)
        {
            return;
        }

        _currentAttackTarget = detectedPlayerDamageable;
    }

    private void TryAttackCurrentTarget()
    {
        if (_currentAttackTarget == null)
        {
            return;
        }

        if (CanAttackTarget(_currentAttackTarget.gameObject) == false)
        {
            return;
        }

        if (_enemyAttack == null)
        {
            return;
        }

        _enemyAttack.TryAttack(_currentAttackTarget);
    }

    private void UpdateCinderHeartTargetIfNeeded()
    {
        if (_enemyDetector != null && _enemyDetector.HasDetectedPlayer)
        {
            return;
        }

        if (_cinderHeartDamageable == null)
        {
            return;
        }

        if (IsInCinderHeartAttackDistance() == false)
        {
            ClearCinderHeartAttackTarget();
            return;
        }

        _currentAttackTarget = _cinderHeartDamageable;
    }

    private bool IsInCinderHeartAttackDistance()
    {
        float distance = Vector3.Distance(transform.position, _cinderHeartDamageable.transform.position);
        return distance <= _cinderHeartAttackDistance;
    }

    private bool CanAttackTarget(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return false;
        }

        if (targetObject.CompareTag(PlayerTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(BuildTag))
        {
            return IsInAttackDistance(targetObject.transform, _attackDistance);
        }

        if (targetObject.CompareTag(CinderHeartTag))
        {
            return IsInAttackDistance(targetObject.transform, _cinderHeartAttackDistance);
        }

        return false;
    }

    private bool IsInAttackDistance(Transform targetTransform, float attackDistance)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);
        return distance <= attackDistance;
    }

    private void ClearPlayerAttackTarget()
    {
        if (_currentAttackTarget == null)
        {
            return;
        }

        if (_currentAttackTarget.CompareTag(PlayerTag))
        {
            _currentAttackTarget = null;
        }
    }

    private void ClearCinderHeartAttackTarget()
    {
        if (_currentAttackTarget == _cinderHeartDamageable)
        {
            _currentAttackTarget = null;
        }
    }

    private Damageable GetDamageableFromTransform(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return null;
        }

        Damageable damageable = targetTransform.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return targetTransform.GetComponentInParent<Damageable>();
    }
}
