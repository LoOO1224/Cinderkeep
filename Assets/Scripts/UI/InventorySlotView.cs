using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 5.00 direction: Displays or controls UI for the 5.00 playable loop without owning gameplay rules.
// 5.01+ note: Keep UI as a view/controller layer; read models and dispatch requests instead of duplicating game logic.
namespace Cinderkeep.Gameplay
{
    // 인벤토리 한 칸을 표시하는 UI 컴포넌트입니다.
    // 드래그 시작만 기록하고, 실제 이동 처리는 InventoryUI가 맡습니다.
    public sealed class InventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Text UI")]
        [SerializeField] private TMP_Text _itemText;

        [Header("Image UI")]
        [SerializeField] private Image _backgroundImage;

        private InventoryUI _ownerInventoryUI;
        private int _slotIndex;

        public int SlotIndex
        {
            get
            {
                return _slotIndex;
            }
        }

        public void SetSlot(int slotIndex, InventoryItemModel itemModel, InventoryUI ownerInventoryUI)
        {
            _slotIndex = slotIndex;
            _ownerInventoryUI = ownerInventoryUI;
            RefreshItemText(itemModel);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_ownerInventoryUI == null)
            {
                return;
            }

            _ownerInventoryUI.BeginDragInventorySlot(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ownerInventoryUI == null)
            {
                return;
            }

            _ownerInventoryUI.EndDragInventorySlot();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_ownerInventoryUI == null || eventData == null)
            {
                return;
            }

            if (eventData.clickCount < 2)
            {
                return;
            }

            _ownerInventoryUI.DoubleClickInventorySlot(this);
        }

        private void RefreshItemText(InventoryItemModel itemModel)
        {
            if (_itemText == null)
            {
                return;
            }

            if (itemModel == null || itemModel.IsEmpty)
            {
                _itemText.text = "";
                RefreshBackground(false);
                return;
            }

            _itemText.text = itemModel.ItemId + "\n" + itemModel.Amount;
            RefreshBackground(true);
        }

        private void RefreshBackground(bool hasItem)
        {
            if (_backgroundImage == null)
            {
                return;
            }

            if (hasItem)
            {
                _backgroundImage.color = new Color(0.22f, 0.30f, 0.34f, 0.92f);
            }
            else
            {
                _backgroundImage.color = new Color(0.10f, 0.13f, 0.16f, 0.78f);
            }
        }
    }
}
