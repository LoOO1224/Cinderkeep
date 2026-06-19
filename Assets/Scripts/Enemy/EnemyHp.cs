using UnityEngine;

// 기존 Enemy HP 작업을 EnemyStatus 기준 구조와 연결하는 호환용 컴포넌트입니다.
// 기준: 적 체력 원본은 EnemyStatus입니다.
// 이 클래스는 체력을 직접 보관하지 않고 EnemyStatus로 요청을 전달합니다.
// 새 체력 기능, HUD 갱신, 사망 처리는 EnemyStatus에 추가합니다.
public sealed class EnemyHp : MonoBehaviour
{
    [SerializeField] private EnemyStatus _targetEnemyStatus;

    private void Awake()
    {
        ConnectComponents();
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
    }

    public int GetCurHp()
    {
        return GetCurrentHp();
    }

    public int GetMaxHp()
    {
        return GetMaximumHp();
    }

    public int GetCurrentHp()
    {
        if (_targetEnemyStatus == null)
        {
            return 0;
        }

        return Mathf.RoundToInt(_targetEnemyStatus.GetCurrentHealth());
    }

    public int GetMaximumHp()
    {
        if (_targetEnemyStatus == null)
        {
            return 0;
        }

        return Mathf.RoundToInt(_targetEnemyStatus.GetMaxHealth());
    }

    public void PrintHp()
    {
        PrintCurrentHp();
    }

    public void PrintCurrentHp()
    {
        Debug.Log(gameObject.name + " 현재 체력 : " + GetCurrentHp() + " / 최대 체력 : " + GetMaximumHp());
    }

    private void ConnectComponents()
    {
        if (_targetEnemyStatus == null)
        {
            _targetEnemyStatus = GetComponent<EnemyStatus>();
        }
    }
}
