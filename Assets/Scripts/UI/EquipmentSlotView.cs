using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
namespace Cinderkeep.Gameplay
{
    // 헬멧, 갑옷, 무기, 신발 장비 칸을 표시하는 UI 컴포넌트입니다.
    // 인벤토리 슬롯에서 드롭하면 InventoryUI에 장착 요청을 전달하고 결과만 표시합니다.
    public sealed class EquipmentSlotView : MonoBehaviour, IDropHandler
    {
        [Header("Slot")]
        [SerializeField] private EquipmentSlotType _slotType;

        [Header("Text UI")]
        [SerializeField] private TMP_Text _slotText;

        [Header("Image UI")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _itemIconImage;

        private InventoryUI _ownerInventoryUI;

        public EquipmentSlotType SlotType
        {
            get
            {
                return _slotType;
            }
        }

        public void SetSlot(EquipmentSlotType slotType, string equippedItemId, InventoryUI ownerInventoryUI)
        {
            _slotType = slotType;
            _ownerInventoryUI = ownerInventoryUI;
            RefreshSlotText(equippedItemId);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_ownerInventoryUI == null)
            {
                return;
            }

            _ownerInventoryUI.DropInventoryToEquipmentSlot(this);
        }

        private void RefreshSlotText(string equippedItemId)
        {
            if (_slotText == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(equippedItemId))
            {
                _slotText.text = GetSlotLabel() + "\nEmpty";
                RefreshBackground(false);
                RefreshItemIcon(false);
                return;
            }

            _slotText.text = GetSlotLabel() + "\n" + UiItemDisplayFormatter.GetItemName(equippedItemId, ResolveItemType());
            RefreshBackground(true);
            RefreshItemIcon(true);
        }

        private string GetSlotLabel()
        {
            return UiItemDisplayFormatter.GetEquipmentSlotLabel(_slotType);
        }

        private void RefreshBackground(bool hasItem)
        {
            if (_backgroundImage == null)
            {
                return;
            }

            if (hasItem)
            {
                _backgroundImage.color = new Color(0.28f, 0.24f, 0.15f, 0.95f);
            }
            else
            {
                _backgroundImage.color = new Color(0.11f, 0.11f, 0.13f, 0.86f);
            }
        }

        private void RefreshItemIcon(bool hasItem)
        {
            if (_itemIconImage == null)
            {
                return;
            }

            _itemIconImage.enabled = hasItem;
            if (hasItem == false)
            {
                return;
            }

            _itemIconImage.color = _slotType == EquipmentSlotType.Weapon
                ? new Color(0.95f, 0.35f, 0.32f, 0.95f)
                : new Color(0.86f, 0.76f, 0.55f, 0.95f);
        }

        private InventoryItemType ResolveItemType()
        {
            if (_slotType == EquipmentSlotType.Weapon)
            {
                return InventoryItemType.Weapon;
            }

            if (_slotType == EquipmentSlotType.Helmet)
            {
                return InventoryItemType.Helmet;
            }

            if (_slotType == EquipmentSlotType.Boots)
            {
                return InventoryItemType.Boots;
            }

            return InventoryItemType.Armor;
        }
    }
}
