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

        public bool TryAddItem(string itemId, InventoryItemType itemType, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            for (int i = 0; i < InventorySlotCount; i++)
            {
                InventoryItemModel slot = _inventorySlots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId == itemId && slot.ItemType == itemType)
                {
                    SetInventorySlot(i, itemId, itemType, slot.Amount + amount);
                    NotifyInventoryChanged();
                    return true;
                }
            }

            for (int i = 0; i < InventorySlotCount; i++)
            {
                InventoryItemModel slot = _inventorySlots[i];
                if (slot == null || slot.IsEmpty)
                {
                    SetInventorySlot(i, itemId, itemType, amount);
                    NotifyInventoryChanged();
                    return true;
                }
            }

            return false;
        }

        public bool HasItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            return GetItemAmount(itemId) >= amount;
        }

        public int GetItemAmount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 0;
            }

            int totalAmount = 0;
            for (int i = 0; i < InventorySlotCount; i++)
            {
                InventoryItemModel slot = _inventorySlots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId == itemId)
                {
                    totalAmount += slot.Amount;
                }
            }

            return totalAmount;
        }

        public bool TryConsumeItem(string itemId, int amount)
        {
            if (HasItem(itemId, amount) == false)
            {
                return false;
            }

            int remainAmount = amount;
            for (int i = 0; i < InventorySlotCount; i++)
            {
                InventoryItemModel slot = _inventorySlots[i];
                if (slot == null || slot.IsEmpty || slot.ItemId != itemId)
                {
                    continue;
                }

                int consumeAmount = Math.Min(slot.Amount, remainAmount);
                int nextAmount = slot.Amount - consumeAmount;
                remainAmount -= consumeAmount;

                if (nextAmount <= 0)
                {
                    slot.Clear();
                }
                else
                {
                    slot.SetItem(slot.ItemId, slot.ItemType, nextAmount);
                }

                if (remainAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }

            NotifyInventoryChanged();
            return true;
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

        public bool TrySetQuickSlotItem(int quickSlotIndex, string itemId, InventoryItemType itemType, int amount)
        {
            if (IsQuickSlotIndexValid(quickSlotIndex) == false)
            {
                return false;
            }

            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            if (_quickSlots[quickSlotIndex] == null)
            {
                _quickSlots[quickSlotIndex] = new InventoryItemModel();
            }

            _quickSlots[quickSlotIndex].SetItem(itemId, itemType, amount);
            NotifyInventoryChanged();
            return true;
        }

        public void ClearQuickSlot(int quickSlotIndex)
        {
            if (IsQuickSlotIndexValid(quickSlotIndex) == false)
            {
                return;
            }

            if (_quickSlots[quickSlotIndex] == null)
            {
                _quickSlots[quickSlotIndex] = new InventoryItemModel();
            }

            _quickSlots[quickSlotIndex].Clear();
            NotifyInventoryChanged();
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
