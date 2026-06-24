using Cinderkeep.Gameplay;
using UnityEngine;

// 플레이어의 기본 근접 공격을 담당하는 컴포넌트입니다.
// 도끼/곡괭이 채집은 PlayerToolUse가 담당하고, 이 클래스는 적과 피해 대상 공격만 담당합니다.
// 무기 공격력, 거리, 범위, 쿨타임은 weapons.json 값이 있으면 그 값을 우선 사용합니다.
public sealed class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("플레이어가 좌클릭 공격으로 주는 기본 피해량입니다. weapons.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _attackDamage = 10f;
    [Tooltip("플레이어 공격이 닿는 최대 거리입니다. weapons.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _attackDistance = 2.5f;
    [Tooltip("공격 판정의 두께입니다. weapons.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _attackRadius = 0.35f;
    [Tooltip("공격 후 다음 공격까지 기다리는 시간입니다. weapons.json 데이터가 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private float _attackInterval = 0.5f;
    [Tooltip("공격 판정에 사용할 레이어입니다.")]
    [SerializeField] private LayerMask _attackLayerMask = ~0;

    [Header("Weapon Data")]
    [Tooltip("true이면 weapons.json의 무기 데이터를 공격 수치로 사용합니다.")]
    [SerializeField] private bool _useWeaponData = true;
    [Tooltip("현재 장착한 무기의 weapons.json _id입니다. 장비 시스템이 붙으면 이 값을 교체합니다.")]
    [SerializeField] private string _weaponDataId = "stone_sword";

    [Header("Connected Objects")]
    [Tooltip("공격 Ray가 시작되는 위치입니다. 비어 있으면 자식 카메라를 찾아 사용합니다.")]
    [SerializeField] private Transform _attackOrigin;
    [Tooltip("좌클릭 공격 시 1인칭 도구 휘두르기 연출을 담당합니다.")]
    [SerializeField] private FirstPersonToolView _firstPersonToolView;
    [Tooltip("선택 사항입니다. 연결하면 Damageable 대상에게 피해를 전달할 때 DamageDealer를 경유합니다.")]
    [SerializeField] private DamageDealer _damageDealer;

    private float _lastAttackTime;
    private PlayerController _playerController;
    private string _lastEquippedWeaponItemId;
    private float _bonusAttackDamage;

    public string WeaponDataId
    {
        get
        {
            return _weaponDataId;
        }
    }

    private void Start()
    {
        ConnectComponents();
    }

    private void Update()
    {
        RefreshEquippedWeapon();

        if (ShouldReadAttackInputDirectly())
        {
            ReadAttackInput();
        }
    }

    public void SetWeaponDataId(string weaponDataId)
    {
        if (string.IsNullOrEmpty(weaponDataId))
        {
            return;
        }

        _weaponDataId = weaponDataId;
    }

    public void AddBonusAttackDamage(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        _bonusAttackDamage += amount;
    }

    public void TryAttack()
    {
        WeaponData weaponData = GetCurrentWeaponData();
        if (CanAttack(weaponData) == false)
        {
            return;
        }

        // 좌클릭 피드백은 적중 여부와 분리해서, 빗나가도 1인칭 휘두르기가 보이게 합니다.
        _lastAttackTime = Time.time;
        PlayAttackView();

        Collider targetCollider = GetAttackTargetCollider(weaponData);
        if (targetCollider == null)
        {
            return;
        }

        if (CanDamageTarget(targetCollider) == false)
        {
            return;
        }

        ApplyDamageToHitTarget(targetCollider, GetAttackDamage(weaponData), weaponData);
    }

    public void ExecuteAttack()
    {
        TryAttack();
    }

    private void ConnectComponents()
    {
        if (_attackOrigin == null)
        {
            Camera camera = GetComponentInChildren<Camera>();
            if (camera != null)
            {
                _attackOrigin = camera.transform;
            }
        }

        if (_firstPersonToolView == null)
        {
            _firstPersonToolView = GetComponentInChildren<FirstPersonToolView>();
        }

        if (_damageDealer == null)
        {
            _damageDealer = GetComponent<DamageDealer>();
        }

        if (_damageDealer != null)
        {
            _damageDealer.SetSourceType(DamageSourceType.Player);
        }

        _playerController = GetComponent<PlayerController>();
    }

    private void ReadAttackInput()
    {
        if (CinderkeepInput.WasLeftMousePressedThisFrame())
        {
            TryAttack();
        }
    }

    private void RefreshEquippedWeapon()
    {
        if (GameManager.Inst == null)
        {
            return;
        }

        PlayerEquipmentModel equipmentModel = GameManager.Inst.PlayerEquipmentModel;
        if (equipmentModel == null)
        {
            return;
        }

        string equippedWeaponItemId = equipmentModel.GetEquippedItemId(EquipmentSlotType.Weapon);
        if (string.IsNullOrEmpty(equippedWeaponItemId))
        {
            return;
        }

        if (_lastEquippedWeaponItemId == equippedWeaponItemId)
        {
            return;
        }

        _lastEquippedWeaponItemId = equippedWeaponItemId;
        SetWeaponDataId(equippedWeaponItemId);
    }

    public bool CanAttack(WeaponData weaponData)
    {
        return Time.time >= _lastAttackTime + GetAttackInterval(weaponData);
    }

    private Collider GetAttackTargetCollider(WeaponData weaponData)
    {
        if (_attackOrigin == null)
        {
            return null;
        }

        Ray attackRay = new Ray(_attackOrigin.position, _attackOrigin.forward);
        RaycastHit hitInfo;
        float attackRadius = GetAttackRadius(weaponData);
        float attackDistance = GetAttackDistance(weaponData);

        if (Physics.SphereCast(attackRay, attackRadius, out hitInfo, attackDistance, _attackLayerMask) == false)
        {
            return null;
        }

        return hitInfo.collider;
    }

    private bool CanDamageTarget(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return false;
        }

        if (targetCollider.GetComponentInParent<ResourceNode>() != null)
        {
            return false;
        }

        if (targetCollider.GetComponentInParent<EnemyStatus>() != null)
        {
            return true;
        }

        return targetCollider.GetComponentInParent<Damageable>() != null;
    }

    public WeaponData GetCurrentWeaponData()
    {
        if (_useWeaponData == false)
        {
            return null;
        }

        if (GameManager.Inst == null)
        {
            return null;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        if (gameDataManager == null)
        {
            return null;
        }

        return gameDataManager.GetWeapon(_weaponDataId);
    }

    private float GetAttackDamage(WeaponData weaponData)
    {
        if (weaponData != null && weaponData.Damage > 0f)
        {
            return weaponData.Damage + _bonusAttackDamage;
        }

        return _attackDamage + _bonusAttackDamage;
    }

    private float GetAttackDistance(WeaponData weaponData)
    {
        if (weaponData != null && weaponData.AttackDistance > 0f)
        {
            return weaponData.AttackDistance;
        }

        return _attackDistance;
    }

    private float GetAttackRadius(WeaponData weaponData)
    {
        if (weaponData != null && weaponData.AttackRadius > 0f)
        {
            return weaponData.AttackRadius;
        }

        return _attackRadius;
    }

    private float GetAttackInterval(WeaponData weaponData)
    {
        if (weaponData != null && weaponData.AttackInterval > 0f)
        {
            return weaponData.AttackInterval;
        }

        return _attackInterval;
    }

    private void PlayAttackView()
    {
        if (_firstPersonToolView == null)
        {
            return;
        }

        _firstPersonToolView.PlaySwing();
    }

    private void ApplyDamageToHitTarget(Collider targetCollider, float damage, WeaponData weaponData)
    {
        if (targetCollider == null)
        {
            return;
        }

        EnemyStatus enemyStatus = targetCollider.GetComponentInParent<EnemyStatus>();
        if (enemyStatus != null)
        {
            enemyStatus.TakeDamage(damage);
            RecordDirectEnemyDamage(damage);
            return;
        }

        Damageable damageable = targetCollider.GetComponentInParent<Damageable>();
        if (damageable != null)
        {
            ApplyDamageToDamageable(damageable, damage, weaponData);
        }
    }

    private void ApplyDamageToDamageable(Damageable damageable, float damage, WeaponData weaponData)
    {
        if (_damageDealer != null)
        {
            _damageDealer.SetSourceType(DamageSourceType.Player);
            if (weaponData != null)
            {
                _damageDealer.SetDamageValue(weaponData);
            }
            else
            {
                _damageDealer.SetDamageValue(damage);
            }

            _damageDealer.ApplyDamage(damageable);
            return;
        }

        damageable.TakeDamage(damage);
    }

    private void RecordDirectEnemyDamage(float damage)
    {
        if (RunResultTracker.Instance == null)
        {
            return;
        }

        RunResultTracker.Instance.RecordPlayerDamageDealt(damage);
    }

    private bool ShouldReadAttackInputDirectly()
    {
        return _playerController == null || _playerController.enabled == false;
    }
}
