using System;

namespace Cinderkeep.Gameplay
{
    // 플레이어 인벤토리와 퀵슬롯을 저장하는 Instance Data입니다.
    // 제작 결과가 들어오면 InventorySlots에 넣고, 전투/채집 단축키는 QuickSlots를 기준으로 확장합니다.
    [Serializable]
    public sealed class PlayerInventoryModel
    {
        public const int InventorySlotCount = 24;
        public const int QuickSlotCount = 7;

        private InventoryItemModel[] _inventorySlots = new InventoryItemModel[InventorySlotCount];
        private InventoryItemModel[] _quickSlots = new InventoryItemModel[QuickSlotCount];

        public event Action OnInventoryChanged;

        public void InitializeDefault()
        {
            InitializeSlotArray(_inventorySlots);
            InitializeSlotArray(_quickSlots);

            // UI 연결 기준을 확인하기 위해 최소 장비 후보만 넣어둡니다.
            // 제작 루프가 붙으면 이 초기 아이템은 제작 결과로 대체할 수 있습니다.
            SetInventorySlot(0, "stone_sword", InventoryItemType.Weapon, 1);
            SetInventorySlot(1, "stone_helmet", InventoryItemType.Helmet, 1);
            SetInventorySlot(2, "stone_armor", InventoryItemType.Armor, 1);
            SetInventorySlot(3, "stone_boots", InventoryItemType.Boots, 1);
            SetInventorySlot(4, "stone_axe", InventoryItemType.Tool, 1);
            SetInventorySlot(5, "stone_pickaxe", InventoryItemType.Tool, 1);

            NotifyInventoryChanged();
        }

        public InventoryItemModel GetInventoryItem(int slotIndex)
        {
            if (IsInventorySlotIndexValid(slotIndex) == false)
            {
                return null;
            }

            return _inventorySlots[slotIndex];
        }

        public InventoryItemModel GetQuickSlotItem(int slotIndex)
        {
            if (IsQuickSlotIndexValid(slotIndex) == false)
            {
                return null;
            }

            return _quickSlots[slotIndex];
        }

        public bool TryMoveInventoryToQuickSlot(int inventorySlotIndex, int quickSlotIndex)
        {
            if (IsInventorySlotIndexValid(inventorySlotIndex) == false)
            {
                return false;
            }

            if (IsQuickSlotIndexValid(quickSlotIndex) == false)
            {
                return false;
            }

            InventoryItemModel sourceItem = _inventorySlots[inventorySlotIndex];
            if (sourceItem == null || sourceItem.IsEmpty)
            {
                return false;
            }

            _quickSlots[quickSlotIndex] = sourceItem.Clone();
            NotifyInventoryChanged();
            return true;
        }

        public bool TryMoveInventoryToEquipmentSlot(int inventorySlotIndex, EquipmentSlotType slotType, PlayerEquipmentModel equipmentModel)
        {
            if (equipmentModel == null)
            {
                return false;
            }

            if (IsInventorySlotIndexValid(inventorySlotIndex) == false)
            {
                return false;
            }

            InventoryItemModel sourceItem = _inventorySlots[inventorySlotIndex];
            if (equipmentModel.TryEquipItem(sourceItem, slotType) == false)
            {
                return false;
            }

            NotifyInventoryChanged();
            return true;
        }

        public void SetInventorySlot(int slotIndex, string itemId, InventoryItemType itemType, int amount)
        {
            if (IsInventorySlotIndexValid(slotIndex) == false)
            {
                return;
            }

            if (_inventorySlots[slotIndex] == null)
            {
                _inventorySlots[slotIndex] = new InventoryItemModel();
            }

            _inventorySlots[slotIndex].SetItem(itemId, itemType, amount);
        }

        private void InitializeSlotArray(InventoryItemModel[] targetSlots)
        {
            if (targetSlots == null)
            {
                return;
            }

            for (int i = 0; i < targetSlots.Length; i++)
            {
                if (targetSlots[i] == null)
                {
                    targetSlots[i] = new InventoryItemModel();
                }

                targetSlots[i].Clear();
            }
        }

        private bool IsInventorySlotIndexValid(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < InventorySlotCount;
        }

        private bool IsQuickSlotIndexValid(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < QuickSlotCount;
        }

        private void NotifyInventoryChanged()
        {
            if (OnInventoryChanged == null)
            {
                return;
            }

            OnInventoryChanged.Invoke();
        }
    }
}
