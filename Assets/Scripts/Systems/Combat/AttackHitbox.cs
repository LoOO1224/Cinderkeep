using UnityEngine;

// 플레이어, 적, 건축물, CinderHeart 사이의 타격 판정과 피해 전달을 담당합니다.
// 무기, 타워, 함정, 보스가 같은 피해 계약을 재사용할 수 있게 범용 흐름으로 유지합니다.
// 공격 판정용 Trigger Collider에 붙이는 컴포넌트입니다.
// 충돌 감지만 담당하고, 실제 데미지 값은 DamageDealer가 관리합니다.
public sealed class AttackHitbox : MonoBehaviour
{
    [SerializeField] private DamageDealer _damageDealer;

    private void Awake()
    {
        ConnectDamageDealerIfNeeded();
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (_damageDealer == null)
        {
            Debug.LogWarning(gameObject.name + ": DamageDealer가 연결되지 않았습니다.");
            return;
        }

        Damageable damageable = GetDamageableFromCollider(otherCollider);
        if (damageable == null)
        {
            return;
        }

        _damageDealer.ApplyDamage(damageable);
    }

    private void ConnectDamageDealerIfNeeded()
    {
        if (_damageDealer != null)
        {
            return;
        }

        _damageDealer = GetComponent<DamageDealer>();
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
