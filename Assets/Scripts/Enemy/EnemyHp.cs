using UnityEditor;
using UnityEngine;

public class EnemyHp : MonoBehaviour
{
    [Header("Enemy Hp")]
    [SerializeField] private int _maxHp = 100;

    [Header("Enemy Hud")]
    [SerializeField] private EnemyHud EnemyHud_EnemyHud;

    private int _curHp;

    public int MaxHp
    {
        get
        {
            return _maxHp;
        }
    }

    public int CurHp
    {
        get
        {
            return _curHp;
        }
    }

    private void Awake()
    {
        ResolveComponents();

        InitHp();
    }

    private void InitHp()
    {
        _curHp = _maxHp;

        RefreshHud();

        Debug.Log($"{gameObject.name} 체력 생성 완료. 현재 체력: {_curHp} / 최대 체력: {_maxHp}");
    }

    private void ResolveComponents()
    {
        if(EnemyHud_EnemyHud == null)
        {
            EnemyHud_EnemyHud = GetComponentInChildren<EnemyHud>();
        }
    }

    // 테스트용 데미지 함수
    // 나중에 공격, 총알, 스킬에서 이 함수를 호출하면 된다.
    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            Debug.LogWarning("EnemyHp: 데미지는 1 이상이어야 합니다.");
            return;
        }

        // 현재 체력 감소
        _curHp -= damage;

        // 체력이 음수로 내려가지 않도록 0으로 고정한다.
        if (_curHp < 0)
        {
            _curHp = 0;
        }

        Debug.Log($"{gameObject.name}이 데미지를 받았습니다. 현재 체력: {_curHp} / 최대 체력: {_maxHp}");

        RefreshHud();
    }

    public int GetCurHp()
    {
        return _curHp;
    }

    public int GetMaxHp()
    {
        return _maxHp;
    }

    public void PrintHp()
    {
        Debug.Log($"{gameObject.name} 현재 체력 : {_curHp} / 최대 체력 : {_maxHp}");
    }

    // HUD에 현재 체력 정보를 전달한다
    private void RefreshHud()
    {
        if (EnemyHud_EnemyHud == null)
        {
            return;
        }
        EnemyHud_EnemyHud.RefreshHp(_curHp, _maxHp);

    }



}
