using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 5.00 direction: Displays or controls UI for the 5.00 playable loop without owning gameplay rules.
// 5.01+ note: Keep UI as a view/controller layer; read models and dispatch requests instead of duplicating game logic.
namespace Cinderkeep.Gameplay
{
    // 1~7번 퀵슬롯을 표시하는 UI 컴포넌트입니다.
    // 드래그 앤 드롭과 더블클릭 자동 등록 결과를 플레이어가 즉시 확인할 수 있게 보여줍니다.
    public sealed class QuickSlotView : MonoBehaviour, IDropHandler
    {
        [Header("Slot")]
        [SerializeField] private int _slotIndex;

        [Header("Text UI")]
        [SerializeField] private TMP_Text _slotText;

        [Header("Image UI")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _itemIconImage;
        [SerializeField] private Image _cookProgressImage;

        private InventoryUI _ownerInventoryUI;
        private InventoryItemModel _currentItemModel;
        private CinderHeartFoodCooker _foodCooker;

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
            RefreshSlotText(itemModel);
        }

        private void Update()
        {
            RefreshCookProgress(_currentItemModel);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_ownerInventoryUI == null)
            {
                return;
            }

            _ownerInventoryUI.DropInventoryToQuickSlot(this);
        }

        private void RefreshSlotText(InventoryItemModel itemModel)
        {
            if (_slotText == null)
            {
                return;
            }

            string numberText = (_slotIndex + 1).ToString();
            if (itemModel == null || itemModel.IsEmpty)
            {
                _slotText.text = numberText;
                RefreshBackground(false);
                RefreshItemIcon(null);
                RefreshCookProgress(null);
                return;
            }

            _slotText.text = numberText + "\n" + UiItemDisplayFormatter.GetItemName(itemModel);
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
                _backgroundImage.color = new Color(0.18f, 0.31f, 0.24f, 0.95f);
            }
            else
            {
                _backgroundImage.color = new Color(0.08f, 0.10f, 0.12f, 0.86f);
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

            if (itemType == InventoryItemType.Building)
            {
                return new Color(0.50f, 0.70f, 0.95f, 0.95f);
            }

            return new Color(0.78f, 0.84f, 0.92f, 0.95f);
        }
    }
}
