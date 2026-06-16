using System;
using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepInventory : MonoBehaviour
    {
        [SerializeField] private bool _grantStarterPickaxe = true;
        [SerializeField] private bool _grantStarterArrows = true;
        [SerializeField] private int _starterPickaxeCount = 1;
        [SerializeField] private int _starterArrowCount = 30;
        [SerializeField] private int _starterArrowQuickSlotIndex = 0;
        [SerializeField] private int _starterPickaxeQuickSlotIndex = 1;

        private readonly CinderkeepInventoryModel _model = new CinderkeepInventoryModel();

        public CinderkeepInventoryModel Model => _model;

        public event Action Changed
        {
            add => _model.Changed += value;
            remove => _model.Changed -= value;
        }

        private void Awake()
        {
            GrantStarterLoadout();
        }

        public void AddItem(CinderkeepItemId itemId, int count)
        {
            _model.AddItem(itemId, count);
        }

        public bool TryRemoveItem(CinderkeepItemId itemId, int count)
        {
            return _model.TryRemoveItem(itemId, count);
        }

        public bool HasItem(CinderkeepItemId itemId)
        {
            return _model.HasItem(itemId);
        }

        public int GetItemCount(CinderkeepItemId itemId)
        {
            return _model.GetItemCount(itemId);
        }

        public CinderkeepItemId GetQuickSlotItem(int slotIndex)
        {
            return _model.GetQuickSlotItem(slotIndex);
        }

        public bool TryAssignQuickSlot(int slotIndex, CinderkeepItemId itemId)
        {
            return _model.TryAssignQuickSlot(slotIndex, itemId);
        }

        private void GrantStarterLoadout()
        {
            if (_grantStarterArrows)
            {
                AddItem(CinderkeepItemId.Arrow, Mathf.Max(1, _starterArrowCount));
                TryAssignQuickSlot(_starterArrowQuickSlotIndex, CinderkeepItemId.Arrow);
            }

            if (!_grantStarterPickaxe)
            {
                return;
            }

            AddItem(CinderkeepItemId.Pickaxe, Mathf.Max(1, _starterPickaxeCount));
            TryAssignQuickSlot(_starterPickaxeQuickSlotIndex, CinderkeepItemId.Pickaxe);
        }
    }
}
