using System;
using UnityEngine;

public sealed class BuildingHp : MonoBehaviour
{
    [SerializeField] private float _maxHp = 100f;

    private float _currentHp;
    private bool _isDestroyed = false;

    public float CurrentHp { get { return _currentHp; } }
    public float MaxHp {  get { return _maxHp; } }
    public bool IsDestroyed { get { return _isDestroyed; } }

    // 어떤 건물이 파괴되었는지 매니저가 바로 알수있도록 자신(BuildingHp)을 넘겨줌
    public event Action<BuildingHp> OnBuildingDestroyed;

    private void Awake()
    {
        // 건물 소환시 최대체력으로 초기화
        _currentHp = _maxHp;
    }

    public void TakeDamage(float damage)
    {
        if (_isDestroyed) return;

        _currentHp -= damage;
        Debug.Log($"{gameObject.name}이 {damage}의 피해를 받음");

        if (_currentHp <= 0f)
        {
            _currentHp = 0f;
            Destroyed();
        }
    }

    // 수리
    public void Repair(float amount) // 회복량 간단히 구현
    {
        // 건물이 완전 파괴되었으면 수리 못하게
        if ( _isDestroyed) return;

        _currentHp += amount;
        if(_currentHp > _maxHp)
        {
            _currentHp = _maxHp;
        }

        Debug.Log($"{gameObject.name}이 수리됨 현재 Hp: {_currentHp}/{_maxHp}");
    }

    private void Destroyed()
    {
        _isDestroyed = true;
        Debug.Log($"[{gameObject.name}]가 파괴되었습니다");

        // 해당 건물이 파괴되었다고 신호 보냄
        OnBuildingDestroyed?.Invoke(this);

        // 일단 Destory 할게요 SetActive(false)하려면 오브젝트 풀링까지 해야해서 일단 이 단계에선 Destroy로 해두겠습니다
        Destroy(gameObject);
    }
}
