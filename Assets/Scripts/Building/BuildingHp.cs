using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// 건축물의 체력, 방어력, 파괴 상태를 관리합니다.
// 배치/비용/타워 공격은 다른 컴포넌트가 맡고, 이 클래스는 피해 처리만 담당합니다.
public sealed class BuildingHp : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("건축물 최대 체력입니다. buildings.json의 MaxHealth로 초기화됩니다.")]
    [SerializeField] private float _maxHp = 100f;
    [Tooltip("받는 피해에서 차감되는 방어력입니다. buildings.json의 Defense로 초기화됩니다.")]
    [SerializeField] private float _defense;

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

    public void Initialize(BuildingData buildingData)
    {
        if (buildingData != null)
        {
            _maxHp = Mathf.Max(1f, buildingData.MaxHealth);
            _defense = Mathf.Max(0f, buildingData.Defense);
        }

        InitializeHp();
    }

    public void TakeDamage(float damage)
    {
        if (_isDestroyed || damage <= 0f)
        {
            return;
        }

        float finalDamage = GetFinalDamage(damage);
        _currentHp = Mathf.Max(0f, _currentHp - finalDamage);
        global::CinderkeepLog.Verbose(gameObject.name + " 건축물 피해: " + finalDamage + ", 현재 체력: " + _currentHp + " / " + _maxHp);

        if (_currentHp <= 0f)
        {
            DestroyBuilding();
        }
    }

    public void Repair(float amount)
    {
        if (_isDestroyed || amount <= 0f)
        {
            return;
        }

        _currentHp = Mathf.Min(_maxHp, _currentHp + amount);
        global::CinderkeepLog.Verbose(gameObject.name + " 건축물 수리: " + amount + ", 현재 체력: " + _currentHp + " / " + _maxHp);
    }

    private float GetFinalDamage(float rawDamage)
    {
        if (_defense <= 0f)
        {
            return rawDamage;
        }

        return Mathf.Max(1f, rawDamage - _defense);
    }

    private void DestroyBuilding()
    {
        _isDestroyed = true;
        global::CinderkeepLog.Verbose(gameObject.name + " 건축물이 파괴되었습니다.");

        if (OnBuildingDestroyed != null)
        {
            OnBuildingDestroyed(this);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    [ContextMenu("Debug Take Damage 50")]
    private void ApplyDebugDamageFromContext()
    {
        TakeDamage(50f);
    }
#endif
}
