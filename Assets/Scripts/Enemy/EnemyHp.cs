using UnityEngine;

// 민석님 Enemy HP 작업을 기존 EnemyStatus 구조와 충돌하지 않게 연결하는 보조 컴포넌트입니다.
// 실제 체력 원본은 EnemyStatus가 담당하고, 이 스크립트는 테스트 호출과 HUD 갱신을 돕습니다.
// 기준: 새 체력 로직은 이 파일에 추가하지 않고 EnemyStatus를 기준으로 작업합니다.
// 이 컴포넌트는 기존 작업 호환 여부를 확인한 뒤 EnemyStatus 기준으로 흡수하거나 제거할 예정입니다.
public sealed class EnemyHp : MonoBehaviour
{
    [SerializeField] private EnemyStatus _targetEnemyStatus;
    [SerializeField] private EnemyHud _enemyHud;

    private void Awake()
    {
        ConnectComponents();
        RefreshHud();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            Debug.LogWarning("EnemyHp: 데미지는 1 이상이어야 합니다.");
            return;
        }

        if (_targetEnemyStatus == null)
        {
            Debug.LogWarning("EnemyHp: EnemyStatus가 연결되지 않았습니다.");
            return;
        }

        _targetEnemyStatus.TakeDamage(damage);
        RefreshHud();
    }

    public int GetCurHp()
    {
        if (_targetEnemyStatus == null)
        {
            return 0;
        }

        return Mathf.RoundToInt(_targetEnemyStatus.GetCurrentHealth());
    }

    public int GetMaxHp()
    {
        if (_targetEnemyStatus == null)
        {
            return 0;
        }

        return Mathf.RoundToInt(_targetEnemyStatus.GetMaxHealth());
    }

    public void PrintHp()
    {
        Debug.Log(gameObject.name + " 현재 체력 : " + GetCurHp() + " / 최대 체력 : " + GetMaxHp());
    }

    private void ConnectComponents()
    {
        if (_targetEnemyStatus == null)
        {
            _targetEnemyStatus = GetComponent<EnemyStatus>();
        }

        if (_enemyHud == null)
        {
            _enemyHud = GetComponentInChildren<EnemyHud>();
        }
    }

    private void RefreshHud()
    {
        if (_enemyHud == null)
        {
            return;
        }

        _enemyHud.RefreshHealth(GetCurHp(), GetMaxHp());
    }
}
