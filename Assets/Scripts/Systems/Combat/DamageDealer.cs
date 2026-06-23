using Cinderkeep.Gameplay;
using UnityEngine;

// 공격자가 가진 데미지 값을 관리하는 컴포넌트입니다.
// 무기, 몬스터 공격 판정, 함정 오브젝트에 붙여 재사용할 수 있습니다.
public sealed class DamageDealer : MonoBehaviour
{
    [Tooltip("이 공격 판정이 대상에게 전달할 피해량입니다.")]
    [SerializeField] private float _damage = 40f;

    public float GetDamageValue()
    {
        return _damage;
    }

    public void SetDamageValue(float damage)
    {
        _damage = Mathf.Max(0f, damage);
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
    }
}
