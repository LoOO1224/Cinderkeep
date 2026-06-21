using System;
using UnityEngine;

// 건축물의 체력과 파괴 상태를 관리하는 컴포넌트입니다.
// 배치, 수리 비용, 제작 규칙은 다른 컴포넌트가 담당하고 이 클래스는 체력만 담당합니다.
public sealed class BuildingHp : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("건축물 최대 체력입니다. 0이 되면 파괴됩니다.")]
    [SerializeField] private float _maxHp = 100f;

    private float _currentHp;
    private bool _isDestroyed;

    public event Action<BuildingHp> OnBuildingDestroyed;

    public float CurrentHp
    {
        get
        {
            return _currentHp;
        }
    }

    public float MaxHp
    {
        get
        {
            return _maxHp;
        }
    }

    public bool IsDestroyed
    {
        get
        {
            return _isDestroyed;
        }
    }

    private void Awake()
    {
        InitializeHp();
    }

    public void InitializeHp()
    {
        _currentHp = Mathf.Max(1f, _maxHp);
        _isDestroyed = false;
    }

    public void TakeDamage(float damage)
    {
        if (_isDestroyed)
        {
            return;
        }

        if (damage <= 0f)
        {
            return;
        }

        _currentHp = Mathf.Max(0f, _currentHp - damage);
        Debug.Log(gameObject.name + " 건축물 피해: " + damage + ", 현재 체력: " + _currentHp + " / " + _maxHp);

        if (_currentHp <= 0f)
        {
            DestroyBuilding();
        }
    }

    public void Repair(float amount)
    {
        if (_isDestroyed)
        {
            return;
        }

        if (amount <= 0f)
        {
            return;
        }

        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        Debug.Log(gameObject.name + " 건축물 수리: " + amount + ", 현재 체력: " + _currentHp + " / " + _maxHp);
    }

    private void DestroyBuilding()
    {
        _isDestroyed = true;
        Debug.Log(gameObject.name + " 건축물이 파괴되었습니다.");

        if (OnBuildingDestroyed != null)
        {
            OnBuildingDestroyed(this);
        }

        Destroy(gameObject);
    }

    // 건축물 파괴 테스트용 (나중에 제거해도 됨)
#if UNITY_EDITOR
    [ContextMenu("Test Take Damage 50")]
    private void TestTakeDamage()
    {
        TakeDamage(50f);
    }
#endif
}
