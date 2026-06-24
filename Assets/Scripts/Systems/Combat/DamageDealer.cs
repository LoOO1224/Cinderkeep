using Cinderkeep.Gameplay;
using System;
using UnityEngine;

// Damageable 대상에게 피해를 전달하는 공통 컴포넌트입니다.
// 플레이어, 적, 타워, 함정, 보스가 같은 피해 전달 규칙과 Run Result 기록 이벤트를 공유합니다.
public enum DamageSourceType
{
    Unknown,
    Player,
    Enemy,
    Tower,
    Trap,
    CinderHeart,
    Boss
}

public sealed class DamageDealer : MonoBehaviour
{
    public static event Action<DamageDealer, Damageable, float, DamageSourceType> DamageAppliedGlobal;

    [Tooltip("대상 Damageable에게 전달할 피해량입니다.")]
    [SerializeField] private float _damage = 40f;
    [Tooltip("Run Result에서 플레이어, 타워, 함정, 보스 피해를 구분하기 위한 출처입니다.")]
    [SerializeField] private DamageSourceType _sourceType = DamageSourceType.Unknown;

    public float GetDamageValue()
    {
        return _damage;
    }

    public void SetDamageValue(float damage)
    {
        _damage = Mathf.Max(0f, damage);
    }

    public void SetSourceType(DamageSourceType sourceType)
    {
        _sourceType = sourceType;
    }

    public void SetDamageValue(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            return;
        }

        SetDamageValue(weaponData.Damage);
    }

    public void ApplyDamage(Damageable damageable)
    {
        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(_damage);
        NotifyDamageApplied(damageable);
    }

    private void NotifyDamageApplied(Damageable damageable)
    {
        if (DamageAppliedGlobal == null)
        {
            return;
        }

        DamageAppliedGlobal(this, damageable, _damage, _sourceType);
    }
}
