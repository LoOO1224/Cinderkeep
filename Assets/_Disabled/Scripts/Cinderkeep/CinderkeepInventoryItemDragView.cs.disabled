using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepInventoryItemDragView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private static CinderkeepItemId _draggedItemId = CinderkeepItemId.None;
        private static CinderkeepInventory _draggedInventory;

        [SerializeField] private CinderkeepInventory CinderkeepInventory_Inventory;
        [SerializeField] private Text Text_Label;
        [SerializeField] private Image Image_Frame;
        [SerializeField] private CanvasGroup CanvasGroup_CanvasGroup;
        [SerializeField] private int _defaultQuickSlotIndex;
        [SerializeField] private CinderkeepItemId _itemId;

        public static bool HasDraggedItem => _draggedItemId != CinderkeepItemId.None;
        public static CinderkeepItemId DraggedItemId => _draggedItemId;
        public static CinderkeepInventory DraggedInventory => _draggedInventory;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CinderkeepInventory_Inventory == null || !CinderkeepInventory_Inventory.HasItem(_itemId))
            {
                return;
            }

            _draggedItemId = _itemId;
            _draggedInventory = CinderkeepInventory_Inventory;

            if (CanvasGroup_CanvasGroup != null)
            {
                CanvasGroup_CanvasGroup.alpha = 0.55f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _draggedItemId = CinderkeepItemId.None;
            _draggedInventory = null;

            if (CanvasGroup_CanvasGroup != null)
            {
                CanvasGroup_CanvasGroup.alpha = 1f;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount < 2 || CinderkeepInventory_Inventory == null)
            {
                return;
            }

            CinderkeepInventory_Inventory.TryAssignQuickSlot(_defaultQuickSlotIndex, _itemId);
        }

        public void SetInventory(CinderkeepInventory inventory)
        {
            CinderkeepInventory_Inventory = inventory;
        }

        public void SetItem(CinderkeepItemId itemId)
        {
            _itemId = itemId;
        }

        public void SetDefaultQuickSlotIndex(int defaultQuickSlotIndex)
        {
            _defaultQuickSlotIndex = defaultQuickSlotIndex;
        }

        public void SetViewReferences(Text label, Image frameImage, CanvasGroup canvasGroup)
        {
            Text_Label = label;
            Image_Frame = frameImage;
            CanvasGroup_CanvasGroup = canvasGroup;
        }

        public void Refresh(int count)
        {
            if (Text_Label != null)
            {
                Text_Label.text = $"{CinderkeepItemCatalog.GetDisplayName(_itemId)} x{count}";
            }

            if (Image_Frame != null)
            {
                Image_Frame.color = count > 0
                    ? new Color(0.12f, 0.18f, 0.16f, 0.92f)
                    : new Color(0.08f, 0.08f, 0.08f, 0.75f);
            }
        }
    }
}
