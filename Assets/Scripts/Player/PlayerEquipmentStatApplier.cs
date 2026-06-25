using Cinderkeep.Gameplay;
using UnityEngine;

// PlayerEquipmentModel에 장착된 방어구 ID를 읽어 실제 플레이어 수치에 적용합니다.
// 장비 상태 저장은 모델이 맡고, 이 컴포넌트는 JSON 데이터와 플레이어 컴포넌트 연결만 담당합니다.
public sealed class PlayerEquipmentStatApplier : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [Header("Connected Components")]
    [SerializeField] private PlayerStatus _playerStatus;
    [SerializeField] private PlayerMovement _playerMovement;

    private PlayerEquipmentModel _equipmentModel;
    private GameDataManager _gameDataManager;

    public static PlayerEquipmentStatApplier EnsureSceneApplier()
    {
        GameObject playerObject = FindPlayerObject();
        if (playerObject == null)
        {
            return null;
        }

        PlayerEquipmentStatApplier applier = playerObject.GetComponent<PlayerEquipmentStatApplier>();
        if (applier == null)
        {
            applier = playerObject.AddComponent<PlayerEquipmentStatApplier>();
        }

        applier.ConnectRuntimeReferences();
        applier.SubscribeEquipmentChanged();
        applier.ApplyEquipmentStats();
        return applier;
    }

    private static GameObject FindPlayerObject()
    {
        try
        {
            return GameObject.FindGameObjectWithTag(PlayerTag);
        }
        catch (UnityException)
        {
            return GameObject.Find("Player");
        }
    }

    private void OnEnable()
    {
        ConnectRuntimeReferences();
        SubscribeEquipmentChanged();
        ApplyEquipmentStats();
    }

    private void OnDisable()
    {
        UnsubscribeEquipmentChanged();
    }

    private void ConnectRuntimeReferences()
    {
        if (_playerStatus == null)
        {
            _playerStatus = GetComponent<PlayerStatus>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (GameManager.Inst == null)
        {
            return;
        }

        _equipmentModel = GameManager.Inst.PlayerEquipmentModel;
        _gameDataManager = GameManager.Inst.GetGameDataManager();
    }

    private void SubscribeEquipmentChanged()
    {
        if (_equipmentModel == null)
        {
            return;
        }

        _equipmentModel.OnEquipmentChanged -= HandleEquipmentChanged;
        _equipmentModel.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void UnsubscribeEquipmentChanged()
    {
        if (_equipmentModel == null)
        {
            return;
        }

        _equipmentModel.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    private void HandleEquipmentChanged()
    {
        ApplyEquipmentStats();
    }

    private void ApplyEquipmentStats()
    {
        ConnectRuntimeReferences();
        if (_equipmentModel == null || _gameDataManager == null)
        {
            return;
        }

        ArmorStatSum statSum = new ArmorStatSum();
        AddArmorStats(_equipmentModel.GetEquippedItemId(EquipmentSlotType.Helmet), statSum);
        AddArmorStats(_equipmentModel.GetEquippedItemId(EquipmentSlotType.Armor), statSum);
        AddArmorStats(_equipmentModel.GetEquippedItemId(EquipmentSlotType.Boots), statSum);

        if (_playerStatus != null)
        {
            _playerStatus.SetEquipmentStatBonuses(
                statSum.Defense,
                statSum.MaxHealthBonus,
                statSum.StaminaBonus);
        }

        if (_playerMovement != null)
        {
            _playerMovement.SetEquipmentMoveSpeedBonus(statSum.MoveSpeedBonus);
        }
    }

    private void AddArmorStats(string armorId, ArmorStatSum statSum)
    {
        if (string.IsNullOrEmpty(armorId) || statSum == null)
        {
            return;
        }

        ArmorData armorData = _gameDataManager.GetArmor(armorId);
        if (armorData == null)
        {
            return;
        }

        statSum.Defense += armorData.Defense;
        statSum.MaxHealthBonus += armorData.MaxHealthBonus;
        statSum.StaminaBonus += armorData.StaminaBonus;
        statSum.MoveSpeedBonus += armorData.MoveSpeedBonus;
    }

    private sealed class ArmorStatSum
    {
        public float Defense;
        public float MaxHealthBonus;
        public float StaminaBonus;
        public float MoveSpeedBonus;
    }
}
