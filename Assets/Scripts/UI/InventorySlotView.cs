using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 5.00 direction: Displays or controls UI for the 5.00 playable loop without owning gameplay rules.
// 5.01+ note: Keep UI as a view/controller layer; read models and dispatch requests instead of duplicating game logic.
namespace Cinderkeep.Gameplay
{
    // 인벤토리 한 칸을 표시하는 UI 컴포넌트입니다.
    // 실제 이동과 장착 판단은 InventoryUI가 맡고, 이 클래스는 슬롯 표시와 입력 전달만 담당합니다.
    public sealed class InventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("Text UI")]
        [SerializeField] private TMP_Text _itemText;

        [Header("Image UI")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _itemIconImage;
        [SerializeField] private Image _cookProgressImage;

        private InventoryUI _ownerInventoryUI;
        private InventoryItemModel _currentItemModel;
        private CinderHeartFoodCooker _foodCooker;
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
            _currentItemModel = itemModel;
            RefreshItemText(itemModel);
        }

        private void Update()
        {
            RefreshCookProgress(_currentItemModel);
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
                RefreshItemIcon(null);
                RefreshCookProgress(null);
                return;
            }

            _itemText.text = UiItemDisplayFormatter.BuildItemStackText(itemModel, true);
            RefreshBackground(true);
            RefreshItemIcon(itemModel);
            RefreshCookProgress(itemModel);
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

        private void RefreshItemIcon(InventoryItemModel itemModel)
        {
            if (_itemIconImage == null)
            {
                return;
            }

            bool hasItem = itemModel != null && itemModel.IsEmpty == false;
            _itemIconImage.enabled = hasItem;
            if (hasItem == false)
            {
                return;
            }

            _itemIconImage.color = GetFallbackItemColor(itemModel.ItemType);
        }

        private void RefreshCookProgress(InventoryItemModel itemModel)
        {
            EnsureCookProgressImage();
            if (_cookProgressImage == null)
            {
                return;
            }

            float cookProgress = GetFoodCookerProgress();
            bool shouldShow = itemModel != null
                && itemModel.IsEmpty == false
                && itemModel.ItemType == InventoryItemType.Food
                && itemModel.ItemId == FoodItemIds.RawMeat
                && cookProgress > 0f;

            _cookProgressImage.enabled = shouldShow;
            _cookProgressImage.fillAmount = shouldShow ? cookProgress : 0f;
        }

        private float GetFoodCookerProgress()
        {
            if (_foodCooker == null)
            {
                _foodCooker = Object.FindFirstObjectByType<CinderHeartFoodCooker>();
            }

            return _foodCooker == null ? 0f : _foodCooker.CookProgress01;
        }

        private void EnsureCookProgressImage()
        {
            if (_cookProgressImage != null)
            {
                return;
            }

            Transform parentTransform = _itemIconImage == null ? transform : _itemIconImage.transform;
            GameObject progressObject = new GameObject("Image_CookProgress", typeof(RectTransform), typeof(Image));
            progressObject.transform.SetParent(parentTransform, false);

            RectTransform progressRect = progressObject.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0f, 0f);
            progressRect.anchorMax = new Vector2(1f, 0f);
            progressRect.pivot = new Vector2(0.5f, 0f);
            progressRect.anchoredPosition = new Vector2(0f, -2f);
            progressRect.sizeDelta = new Vector2(0f, 4f);

            _cookProgressImage = progressObject.GetComponent<Image>();
            _cookProgressImage.color = new Color(0.25f, 0.95f, 0.35f, 0.95f);
            _cookProgressImage.type = Image.Type.Filled;
            _cookProgressImage.fillMethod = Image.FillMethod.Horizontal;
            _cookProgressImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            _cookProgressImage.fillAmount = 0f;
            _cookProgressImage.enabled = false;
        }

        private Color GetFallbackItemColor(InventoryItemType itemType)
        {
            if (itemType == InventoryItemType.Resource)
            {
                return new Color(0.68f, 0.78f, 0.86f, 0.95f);
            }

            if (itemType == InventoryItemType.Weapon)
            {
                return new Color(0.95f, 0.35f, 0.32f, 0.95f);
            }

            if (itemType == InventoryItemType.Tool)
            {
                return new Color(0.82f, 0.73f, 0.50f, 0.95f);
            }

            if (itemType == InventoryItemType.Food)
            {
                return new Color(0.35f, 0.92f, 0.45f, 0.95f);
            }

            return new Color(0.78f, 0.84f, 0.92f, 0.95f);
        }
    }
}
