using System;

// 한 판 플레이 중 변하는 런타임 상태를 저장합니다.
// 상태 변경은 명시적인 메서드로 처리하고, UI와 시스템은 이 모델을 읽거나 요청만 보냅니다.
namespace Cinderkeep.Gameplay
{
    // 플레이어가 장착한 장비 ID를 저장하는 Instance Data입니다.
    // 실제 방어구 수치 적용은 PlayerEquipmentStatApplier가 PlayerStatus와 PlayerMovement에 반영합니다.
    [Serializable]
    public sealed class PlayerEquipmentModel
    {
        private string _helmetItemId;
        private string _armorItemId;
        private string _weaponItemId;
        private string _bootsItemId;

        public event Action OnEquipmentChanged;

        public void InitializeDefault()
        {
            _helmetItemId = string.Empty;
            _armorItemId = string.Empty;
            _weaponItemId = string.Empty;
            _bootsItemId = string.Empty;
            NotifyEquipmentChanged();
        }

        public bool TryEquipItem(InventoryItemModel itemModel, EquipmentSlotType slotType)
        {
            if (CanEquipItem(itemModel, slotType) == false)
            {
                return false;
            }

            SetEquippedItem(slotType, itemModel.ItemId);
            NotifyEquipmentChanged();
            return true;
        }

        public bool CanEquipItem(InventoryItemModel itemModel, EquipmentSlotType slotType)
        {
            if (itemModel == null || itemModel.IsEmpty)
            {
                return false;
            }

            if (slotType == EquipmentSlotType.Helmet)
            {
                return itemModel.ItemType == InventoryItemType.Helmet;
            }

            if (slotType == EquipmentSlotType.Armor)
            {
                return itemModel.ItemType == InventoryItemType.Armor;
            }

            if (slotType == EquipmentSlotType.Weapon)
            {
                return itemModel.ItemType == InventoryItemType.Weapon;
            }

            if (slotType == EquipmentSlotType.Boots)
            {
                return itemModel.ItemType == InventoryItemType.Boots;
            }

            return false;
        }

        public string GetEquippedItemId(EquipmentSlotType slotType)
        {
            if (slotType == EquipmentSlotType.Helmet)
            {
                return _helmetItemId;
            }

            if (slotType == EquipmentSlotType.Armor)
            {
                return _armorItemId;
            }

            if (slotType == EquipmentSlotType.Weapon)
            {
                return _weaponItemId;
            }

            if (slotType == EquipmentSlotType.Boots)
            {
                return _bootsItemId;
            }

            return string.Empty;
        }

        private void SetEquippedItem(EquipmentSlotType slotType, string itemId)
        {
            if (slotType == EquipmentSlotType.Helmet)
            {
                _helmetItemId = itemId;
                return;
            }

            if (slotType == EquipmentSlotType.Armor)
            {
                _armorItemId = itemId;
                return;
            }

            if (slotType == EquipmentSlotType.Weapon)
            {
                _weaponItemId = itemId;
                return;
            }

            if (slotType == EquipmentSlotType.Boots)
            {
                _bootsItemId = itemId;
            }
        }

        private void NotifyEquipmentChanged()
        {
            if (OnEquipmentChanged == null)
            {
                return;
            }

            OnEquipmentChanged.Invoke();
        }
    }
}
