using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepQuickSlotDropTarget : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] private CinderkeepInventory CinderkeepInventory_Inventory;
        [SerializeField] private CinderkeepHudView CinderkeepHudView_HudView;
        [SerializeField] private Text Text_Label;
        [SerializeField] private Image Image_Frame;
        [SerializeField] private int _slotIndex;

        private bool _isSelected;

        public void OnDrop(PointerEventData eventData)
        {
            if (!CinderkeepInventoryItemDragView.HasDraggedItem)
            {
                return;
            }

            CinderkeepInventory sourceInventory = CinderkeepInventoryItemDragView.DraggedInventory;
            CinderkeepInventory targetInventory = CinderkeepInventory_Inventory != null ? CinderkeepInventory_Inventory : sourceInventory;
            targetInventory?.TryAssignQuickSlot(_slotIndex, CinderkeepInventoryItemDragView.DraggedItemId);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CinderkeepHudView_HudView?.SetSelectedQuickSlot(_slotIndex);
        }

        public void SetInventory(CinderkeepInventory inventory)
        {
            CinderkeepInventory_Inventory = inventory;
        }

        public void SetHudView(CinderkeepHudView hudView)
        {
            CinderkeepHudView_HudView = hudView;
        }

        public void SetSlotIndex(int slotIndex)
        {
            _slotIndex = slotIndex;
        }

        public void SetViewReferences(Text label, Image frameImage)
        {
            Text_Label = label;
            Image_Frame = frameImage;
        }

        public void Refresh(CinderkeepItemId itemId)
        {
            if (Text_Label != null)
            {
                string itemName = CinderkeepItemCatalog.GetDisplayName(itemId);
                Text_Label.text = $"{_slotIndex + 1}\n{itemName}";
            }

            RefreshSelected(_isSelected);
        }

        public void RefreshSelected(bool isSelected)
        {
            _isSelected = isSelected;
            if (Image_Frame != null)
            {
                Image_Frame.color = isSelected
                    ? new Color(0.95f, 0.67f, 0.18f, 0.95f)
                    : new Color(0.08f, 0.12f, 0.14f, 0.9f);
            }
        }
    }
}
