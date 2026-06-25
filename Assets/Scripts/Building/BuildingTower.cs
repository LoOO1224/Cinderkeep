using System.Collections;
using Cinderkeep.Gameplay;
using UnityEngine;

// 방어 타워의 자동 공격 루프를 조율합니다.
// 타깃 탐색은 TowerTargeting, 피해 적용은 TowerAttack으로 분리해 건축 데이터와 전투 기록을 안정적으로 연결합니다.
public sealed class BuildingTower : MonoBehaviour
{
    private const float AttackLoopDelay = 0.2f;

    [Header("Connected Components")]
    [SerializeField] private TowerTargeting _towerTargeting;
    [SerializeField] private TowerAttack _towerAttack;
    [Header("Optional View")]
    [Tooltip("선택 사항입니다. 연결하면 공격 대상 방향으로 회전합니다.")]
    [SerializeField] private Transform _turretHead;

    private Coroutine _attackLoopRoutine;

    private void Awake()
    {
        ConnectTowerComponents();
    }

    private void OnEnable()
    {
        StartAttackLoopRoutine();
    }

    private void OnDisable()
    {
        StopAttackLoopRoutine();
    }

    public void Initialize(BuildingData buildingData)
    {
        if (buildingData == null)
        {
            Debug.LogWarning(gameObject.name + " BuildingData가 없어 타워 수치를 초기화하지 못했습니다.");
            return;
        }

        ConnectTowerComponents();

        if (_towerTargeting != null)
        {
            _towerTargeting.SetAttackRange(buildingData.AttackRange);
        }

        if (_towerAttack != null)
        {
            _towerAttack.SetAttackDamage(buildingData.AttackDamage);
            _towerAttack.SetAttackInterval(buildingData.AttackInterval);
        }
    }

    private void ConnectTowerComponents()
    {
        if (_towerTargeting == null)
        {
            _towerTargeting = GetComponent<TowerTargeting>();
        }

        if (_towerAttack == null)
        {
            _towerAttack = GetComponent<TowerAttack>();
        }
    }

    private void StartAttackLoopRoutine()
    {
        StopAttackLoopRoutine();
        _attackLoopRoutine = StartCoroutine(AttackLoopRoutine());
    }

    private void StopAttackLoopRoutine()
    {
        if (_attackLoopRoutine == null)
        {
            return;
        }

        StopCoroutine(_attackLoopRoutine);
        _attackLoopRoutine = null;
    }

    private IEnumerator AttackLoopRoutine()
    {
        WaitForSeconds waitDelay = new WaitForSeconds(AttackLoopDelay);

        while (true)
        {
            ProcessAttack();
            yield return waitDelay;
        }
    }

    private void ProcessAttack()
    {
        if (_towerTargeting == null || _towerAttack == null)
        {
            return;
        }

        if (_towerTargeting.HasTarget == false)
        {
            return;
        }

        EnemyStatus currentTarget = _towerTargeting.CurrentTarget;
        if (currentTarget == null)
        {
            return;
        }

        RotateTurretHeadTowardTarget(currentTarget.transform.position);
        _towerAttack.TryAttack(currentTarget);
    }

    private void RotateTurretHeadTowardTarget(Vector3 targetPosition)
    {
        if (_turretHead == null)
        {
            return;
        }

        Vector3 direction = targetPosition - _turretHead.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        _turretHead.rotation = lookRotation;
    }

#if UNITY_EDITOR
    private void Reset()
    {
        ConnectTowerComponents();
    }
#endif
}
