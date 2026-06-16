using System;
using System.Collections.Generic;

namespace OODong.Cinderkeep
{
    public struct CinderkeepInventoryStack
    {
        public CinderkeepInventoryStack(CinderkeepItemId itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }

        public CinderkeepItemId ItemId { get; }
        public int Count { get; }
    }

    public sealed class CinderkeepInventoryModel
    {
        public const int QuickSlotCount = 7;

        private readonly Dictionary<CinderkeepItemId, int> _items = new Dictionary<CinderkeepItemId, int>();
        private readonly CinderkeepItemId[] _quickSlots = new CinderkeepItemId[QuickSlotCount];

        public event Action Changed;

        public void AddItem(CinderkeepItemId itemId, int count)
        {
            if (itemId == CinderkeepItemId.None || count <= 0)
            {
                return;
            }

            int currentCount = GetItemCount(itemId);
            _items[itemId] = currentCount + count;
            NotifyChanged();
        }

        public bool TryRemoveItem(CinderkeepItemId itemId, int count)
        {
            if (itemId == CinderkeepItemId.None || count <= 0)
            {
                return false;
            }

            int currentCount = GetItemCount(itemId);
            if (currentCount < count)
            {
                return false;
            }

            int nextCount = currentCount - count;
            if (nextCount <= 0)
            {
                _items.Remove(itemId);
            }
            else
            {
                _items[itemId] = nextCount;
            }

            NotifyChanged();
            return true;
        }

        public bool HasItem(CinderkeepItemId itemId)
        {
            return GetItemCount(itemId) > 0;
        }

        public int GetItemCount(CinderkeepItemId itemId)
        {
            return _items.TryGetValue(itemId, out int count) ? count : 0;
        }

        public CinderkeepItemId GetQuickSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _quickSlots.Length)
            {
                return CinderkeepItemId.None;
            }

            return _quickSlots[slotIndex];
        }

        public bool TryAssignQuickSlot(int slotIndex, CinderkeepItemId itemId)
        {
            if (slotIndex < 0 || slotIndex >= _quickSlots.Length)
            {
                return false;
            }

            if (!CinderkeepItemCatalog.CanAssignQuickSlot(itemId) || !HasItem(itemId))
            {
                return false;
            }

            _quickSlots[slotIndex] = itemId;
            NotifyChanged();
            return true;
        }

        public IEnumerable<CinderkeepInventoryStack> GetStacks(CinderkeepItemId[] itemOrder)
        {
            for (int i = 0; i < itemOrder.Length; i++)
            {
                CinderkeepItemId itemId = itemOrder[i];
                yield return new CinderkeepInventoryStack(itemId, GetItemCount(itemId));
            }
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
