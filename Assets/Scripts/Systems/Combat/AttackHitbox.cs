using UnityEngine;

// 공격 판정용 Trigger Collider에 붙이는 컴포넌트입니다.
// 충돌 감지만 담당하고, 실제 데미지 값은 DamageDealer가 관리합니다.
public sealed class AttackHitbox : MonoBehaviour
{
    [SerializeField] private DamageDealer DamageDealer_DamageDealer;

    private void Awake()
    {
        ConnectDamageDealerIfNeeded();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (DamageDealer_DamageDealer == null)
        {
            Debug.LogWarning(gameObject.name + ": DamageDealer가 연결되지 않았습니다.");
            return;
        }

        Damageable damageable = GetDamageableFromCollider(otherCollider);
        if (damageable == null)
        {
            return;
        }

        DamageDealer_DamageDealer.ApplyDamage(damageable);
    }

    private void ConnectDamageDealerIfNeeded()
    {
        if (DamageDealer_DamageDealer != null)
        {
            return;
        }

        DamageDealer_DamageDealer = GetComponent<DamageDealer>();
    }

    private Damageable GetDamageableFromCollider(Collider otherCollider)
    {
        if (otherCollider == null)
        {
            return null;
        }

        Damageable damageable = otherCollider.GetComponent<Damageable>();
        if (damageable != null)
        {
            return damageable;
        }

        return otherCollider.GetComponentInParent<Damageable>();
    }
}
