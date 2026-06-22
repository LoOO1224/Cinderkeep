using System;

namespace Cinderkeep.Gameplay
{
    // 플레이어가 장착한 장비를 저장하는 Instance Data입니다.
    // 현재는 헬멧, 갑옷, 무기, 신발 칸만 열어두고 장비 효과 계산은 후속 작업에서 붙입니다.
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
