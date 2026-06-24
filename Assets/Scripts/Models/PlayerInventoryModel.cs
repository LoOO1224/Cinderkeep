using System;
using System.Collections.Generic;

// 5.00 direction: Stores runtime state for one play run in the 5.00 loop.
// 5.01+ note: Keep state mutation explicit and let UI or gameplay systems observe it instead of owning it.
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
        private readonly Dictionary<string, int> _preparedBuildingCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public event Action OnInventoryChanged;

        public void InitializeDefault()
        {
            InitializeSlotArray(_inventorySlots);
            InitializeSlotArray(_quickSlots);
            _preparedBuildingCounts.Clear();

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

        public bool CanAddItem(string itemId, InventoryItemType itemType, int amount)
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
                    return true;
                }

                if (slot.ItemId == itemId && slot.ItemType == itemType)
                {
                    return true;
                }
            }

            return false;
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

        public bool TryAddPreparedBuilding(string buildingId, int amount)
        {
            if (string.IsNullOrEmpty(buildingId) || amount <= 0)
            {
                return false;
            }

            int currentAmount = GetPreparedBuildingCount(buildingId);
            _preparedBuildingCounts[buildingId] = currentAmount + amount;
            NotifyInventoryChanged();
            return true;
        }

        public bool HasPreparedBuilding(string buildingId)
        {
            return GetPreparedBuildingCount(buildingId) > 0;
        }

        public int GetPreparedBuildingCount(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                return 0;
            }

            int amount;
            if (_preparedBuildingCounts.TryGetValue(buildingId, out amount) == false)
            {
                return 0;
            }

            return Math.Max(0, amount);
        }

        public bool TryConsumePreparedBuilding(string buildingId, int amount)
        {
            if (string.IsNullOrEmpty(buildingId) || amount <= 0)
            {
                return false;
            }

            int currentAmount = GetPreparedBuildingCount(buildingId);
            if (currentAmount < amount)
            {
                return false;
            }

            int nextAmount = currentAmount - amount;
            if (nextAmount <= 0)
            {
                _preparedBuildingCounts.Remove(buildingId);
            }
            else
            {
                _preparedBuildingCounts[buildingId] = nextAmount;
            }

            NotifyInventoryChanged();
            return true;
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

        public bool TryAddQuickSlotItem(
            string itemId,
            InventoryItemType itemType,
            int amount,
            int preferredSlotIndex,
            int replacementSlotIndex,
            out int slotIndex)
        {
            slotIndex = -1;
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            if (TryStackQuickSlotItem(itemId, itemType, amount, out slotIndex))
            {
                return true;
            }

            if (TrySetEmptyPreferredQuickSlot(itemId, itemType, amount, preferredSlotIndex, out slotIndex))
            {
                return true;
            }

            if (TrySetFirstEmptyQuickSlot(itemId, itemType, amount, out slotIndex))
            {
                return true;
            }

            slotIndex = ClampQuickSlotIndex(replacementSlotIndex);
            return TrySetQuickSlotItem(slotIndex, itemId, itemType, amount);
        }

        public bool TryAssignQuickSlotShortcut(
            string itemId,
            InventoryItemType itemType,
            int amount,
            int preferredSlotIndex,
            int replacementSlotIndex,
            out int slotIndex)
        {
            slotIndex = -1;
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            if (TryFindQuickSlotItem(itemId, itemType, out slotIndex))
            {
                return TrySetQuickSlotItem(slotIndex, itemId, itemType, amount);
            }

            if (TrySetEmptyPreferredQuickSlot(itemId, itemType, amount, preferredSlotIndex, out slotIndex))
            {
                return true;
            }

            if (TrySetFirstEmptyQuickSlot(itemId, itemType, amount, out slotIndex))
            {
                return true;
            }

            slotIndex = ClampQuickSlotIndex(replacementSlotIndex);
            return TrySetQuickSlotItem(slotIndex, itemId, itemType, amount);
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

        public bool TryConsumeQuickSlotItem(int quickSlotIndex, int amount)
        {
            if (IsQuickSlotIndexValid(quickSlotIndex) == false || amount <= 0)
            {
                return false;
            }

            InventoryItemModel slot = _quickSlots[quickSlotIndex];
            if (slot == null || slot.IsEmpty || slot.Amount < amount)
            {
                return false;
            }

            int nextAmount = slot.Amount - amount;
            if (nextAmount <= 0)
            {
                slot.Clear();
            }
            else
            {
                slot.SetItem(slot.ItemId, slot.ItemType, nextAmount);
            }

            NotifyInventoryChanged();
            return true;
        }

        public bool HasItemInInventoryOrQuickSlot(string itemId, InventoryItemType itemType)
        {
            return HasItemInSlots(_inventorySlots, itemId, itemType)
                || HasItemInSlots(_quickSlots, itemId, itemType);
        }

        public bool TryReplaceItemIdEverywhere(string fromItemId, string toItemId, InventoryItemType itemType)
        {
            if (string.IsNullOrEmpty(fromItemId) || string.IsNullOrEmpty(toItemId))
            {
                return false;
            }

            bool changed = ReplaceItemIdInSlots(_inventorySlots, fromItemId, toItemId, itemType);
            changed |= ReplaceItemIdInSlots(_quickSlots, fromItemId, toItemId, itemType);
            if (changed)
            {
                NotifyInventoryChanged();
            }

            return changed;
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

        private bool HasItemInSlots(InventoryItemModel[] slots, string itemId, InventoryItemType itemType)
        {
            if (slots == null || string.IsNullOrEmpty(itemId))
            {
                return false;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                InventoryItemModel slot = slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId == itemId && slot.ItemType == itemType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ReplaceItemIdInSlots(InventoryItemModel[] slots, string fromItemId, string toItemId, InventoryItemType itemType)
        {
            if (slots == null)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < slots.Length; i++)
            {
                InventoryItemModel slot = slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId != fromItemId || slot.ItemType != itemType)
                {
                    continue;
                }

                slot.SetItem(toItemId, itemType, slot.Amount);
                changed = true;
            }

            return changed;
        }

        private bool IsInventorySlotIndexValid(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < InventorySlotCount;
        }

        private bool IsQuickSlotIndexValid(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < QuickSlotCount;
        }

        private bool TryStackQuickSlotItem(string itemId, InventoryItemType itemType, int amount, out int slotIndex)
        {
            slotIndex = -1;
            for (int i = 0; i < QuickSlotCount; i++)
            {
                InventoryItemModel slot = _quickSlots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId != itemId || slot.ItemType != itemType)
                {
                    continue;
                }

                slot.SetItem(itemId, itemType, slot.Amount + amount);
                slotIndex = i;
                NotifyInventoryChanged();
                return true;
            }

            return false;
        }

        private bool TryFindQuickSlotItem(string itemId, InventoryItemType itemType, out int slotIndex)
        {
            slotIndex = -1;
            for (int i = 0; i < QuickSlotCount; i++)
            {
                InventoryItemModel slot = _quickSlots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (slot.ItemId != itemId || slot.ItemType != itemType)
                {
                    continue;
                }

                slotIndex = i;
                return true;
            }

            return false;
        }

        private bool TrySetEmptyPreferredQuickSlot(
            string itemId,
            InventoryItemType itemType,
            int amount,
            int preferredSlotIndex,
            out int slotIndex)
        {
            slotIndex = -1;
            if (IsQuickSlotIndexValid(preferredSlotIndex) == false)
            {
                return false;
            }

            InventoryItemModel preferredSlot = _quickSlots[preferredSlotIndex];
            if (preferredSlot != null && preferredSlot.IsEmpty == false)
            {
                return false;
            }

            slotIndex = preferredSlotIndex;
            return TrySetQuickSlotItem(slotIndex, itemId, itemType, amount);
        }

        private bool TrySetFirstEmptyQuickSlot(string itemId, InventoryItemType itemType, int amount, out int slotIndex)
        {
            slotIndex = -1;
            for (int i = 0; i < QuickSlotCount; i++)
            {
                InventoryItemModel slot = _quickSlots[i];
                if (slot != null && slot.IsEmpty == false)
                {
                    continue;
                }

                slotIndex = i;
                return TrySetQuickSlotItem(slotIndex, itemId, itemType, amount);
            }

            return false;
        }

        private int ClampQuickSlotIndex(int slotIndex)
        {
            if (slotIndex < 0)
            {
                return 0;
            }

            if (slotIndex >= QuickSlotCount)
            {
                return QuickSlotCount - 1;
            }

            return slotIndex;
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
