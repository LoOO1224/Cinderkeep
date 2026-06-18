using UnityEngine;

// 플레이어의 기본 근접 공격을 담당하는 컴포넌트입니다.
// 좌클릭으로 도끼/곡괭이 채집을 먼저 시도하고, 자원이 아니면 적에게 피해를 줍니다.
public sealed class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackDistance = 2.5f;
    [SerializeField] private float _attackRadius = 0.35f;
    [SerializeField] private float _attackInterval = 0.5f;
    [SerializeField] private LayerMask _attackLayerMask = ~0;

    [Header("Connected Objects")]
    [SerializeField] private Transform Transform_AttackOrigin;
    [SerializeField] private FirstPersonToolView FirstPersonToolView_FirstPersonToolView;

    private PlayerToolController _playerToolController;
    private float _lastAttackTime;

    private void Start()
    {
        ConnectComponents();
    }

    private void Update()
    {
        ReadAttackInput();
    }

    public void TryAttack()
    {
        if (CanAttack() == false)
        {
            return;
        }

        _lastAttackTime = Time.time;
        PlayAttackView();
        ApplyAttack();
    }

    private void ConnectComponents()
    {
        _playerToolController = GetComponent<PlayerToolController>();

        if (Transform_AttackOrigin == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                Transform_AttackOrigin = camera.transform;
            }
        }

        if (FirstPersonToolView_FirstPersonToolView == null)
        {
            FirstPersonToolView_FirstPersonToolView = GetComponentInChildren<FirstPersonToolView>();
        }
    }

    private void ReadAttackInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private bool CanAttack()
    {
        return Time.time >= _lastAttackTime + _attackInterval;
    }

    private void PlayAttackView()
    {
        if (FirstPersonToolView_FirstPersonToolView == null)
        {
            return;
        }

        FirstPersonToolView_FirstPersonToolView.PlaySwing();
    }

    private void ApplyAttack()
    {
        if (Transform_AttackOrigin == null)
        {
            return;
        }

        Ray attackRay = new Ray(Transform_AttackOrigin.position, Transform_AttackOrigin.forward);
        RaycastHit hitInfo;

        if (Physics.SphereCast(attackRay, _attackRadius, out hitInfo, _attackDistance, _attackLayerMask) == false)
        {
            return;
        }

        if (TryGatherResource(hitInfo.collider))
        {
            return;
        }

        ApplyDamageToHitTarget(hitInfo.collider);
    }

    private bool TryGatherResource(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (_playerToolController == null)
        {
            return false;
        }

        ResourceNode resourceNode = targetCollider.GetComponentInParent<ResourceNode>();
        if (resourceNode == null)
        {
            return false;
        }

        return resourceNode.TryGatherWithTool(gameObject, _playerToolController.CurrentToolType);
    }

    private void ApplyDamageToHitTarget(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return;
        }

        EnemyStatus enemyStatus = targetCollider.GetComponentInParent<EnemyStatus>();
        if (enemyStatus != null)
        {
            enemyStatus.TakeDamage(_attackDamage);
            return;
        }

        Damageable damageable = targetCollider.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_attackDamage);
        }
    }
}
