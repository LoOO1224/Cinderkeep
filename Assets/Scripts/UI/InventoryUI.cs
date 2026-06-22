using TMPro;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // Tab으로 여는 인벤토리 UI의 표시와 드롭 연결을 담당합니다.
    // 실제 저장 상태는 PlayerInventoryModel, PlayerEquipmentModel이 담당합니다.
    public sealed class InventoryUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject _rootObject;

        [Header("Text UI")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _messageText;

        [Header("Slot Views")]
        [SerializeField] private EquipmentSlotView[] _equipmentSlotViews;
        [SerializeField] private InventorySlotView[] _inventorySlotViews;
        [SerializeField] private QuickSlotView[] _quickSlotViews;

        private PlayerInventoryModel _playerInventoryModel;
        private PlayerEquipmentModel _playerEquipmentModel;
        private InventorySlotView _draggingInventorySlot;
        private bool _isOpen;

        public bool IsOpen
        {
            get
            {
                return _isOpen;
            }
        }

        private void OnDisable()
        {
            DisconnectModels();
        }

        public void Open()
        {
            ConnectModels();
            SetVisible(true);
            RefreshUI();

        }

        public void Close()
        {
            _draggingInventorySlot = null;
            SetVisible(false);

        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        public void BeginDragInventorySlot(InventorySlotView slotView)
        {
            _draggingInventorySlot = slotView;
            RefreshMessage("드롭할 장비 칸이나 퀵슬롯을 선택하세요.");
        }

        public void EndDragInventorySlot()
        {
            _draggingInventorySlot = null;
        }

        public void DropInventoryToEquipmentSlot(EquipmentSlotView equipmentSlotView)
        {
            if (_draggingInventorySlot == null || equipmentSlotView == null)
            {
                return;
            }

            ConnectModels();
            if (_playerInventoryModel == null || _playerEquipmentModel == null)
            {
                RefreshMessage("인벤토리 모델 연결이 준비되지 않았습니다.");
                return;
            }

            bool isEquipped = _playerInventoryModel.TryMoveInventoryToEquipmentSlot(
                _draggingInventorySlot.SlotIndex,
                equipmentSlotView.SlotType,
                _playerEquipmentModel);

            if (isEquipped)
            {
                RefreshMessage("장비 칸에 연결했습니다.");
                RefreshUI();
                return;
            }

            RefreshMessage("이 장비 칸에는 넣을 수 없습니다.");
        }

        public void DropInventoryToQuickSlot(QuickSlotView quickSlotView)
        {
            if (_draggingInventorySlot == null || quickSlotView == null)
            {
                return;
            }

            ConnectModels();
            if (_playerInventoryModel == null)
            {
                RefreshMessage("인벤토리 모델 연결이 준비되지 않았습니다.");
                return;
            }

            bool isMoved = _playerInventoryModel.TryMoveInventoryToQuickSlot(
                _draggingInventorySlot.SlotIndex,
                quickSlotView.SlotIndex);

            if (isMoved)
            {
                RefreshMessage("퀵슬롯에 등록했습니다.");
                RefreshUI();
                return;
            }

            RefreshMessage("퀵슬롯에 등록할 수 없습니다.");
        }

        public void RefreshUI()
        {
            ConnectModels();
            RefreshTitle();
            RefreshEquipmentSlots();
            RefreshInventorySlots();
            RefreshQuickSlots();
        }

        private void ConnectModels()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            PlayerEquipmentModel equipmentModel = GameManager.Inst.PlayerEquipmentModel;

            if (_playerInventoryModel == inventoryModel && _playerEquipmentModel == equipmentModel)
            {
                return;
            }

            DisconnectModels();
            _playerInventoryModel = inventoryModel;
            _playerEquipmentModel = equipmentModel;

            if (_playerInventoryModel != null)
            {
                _playerInventoryModel.OnInventoryChanged += RefreshUI;
            }

            if (_playerEquipmentModel != null)
            {
                _playerEquipmentModel.OnEquipmentChanged += RefreshUI;
            }
        }

        private void DisconnectModels()
        {
            if (_playerInventoryModel != null)
            {
                _playerInventoryModel.OnInventoryChanged -= RefreshUI;
            }

            if (_playerEquipmentModel != null)
            {
                _playerEquipmentModel.OnEquipmentChanged -= RefreshUI;
            }

            _playerInventoryModel = null;
            _playerEquipmentModel = null;
        }

        private void RefreshTitle()
        {
            if (_titleText == null)
            {
                return;
            }

            _titleText.text = "Inventory / Equipment";
        }

        private void RefreshEquipmentSlots()
        {
            if (_equipmentSlotViews == null || _playerEquipmentModel == null)
            {
                return;
            }

            for (int i = 0; i < _equipmentSlotViews.Length; i++)
            {
                EquipmentSlotView slotView = _equipmentSlotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                EquipmentSlotType slotType = slotView.SlotType;
                string equippedItemId = _playerEquipmentModel.GetEquippedItemId(slotType);
                slotView.SetSlot(slotType, equippedItemId, this);
            }
        }

        private void RefreshInventorySlots()
        {
            if (_inventorySlotViews == null || _playerInventoryModel == null)
            {
                return;
            }

            for (int i = 0; i < _inventorySlotViews.Length; i++)
            {
                InventorySlotView slotView = _inventorySlotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                InventoryItemModel itemModel = _playerInventoryModel.GetInventoryItem(i);
                slotView.SetSlot(i, itemModel, this);
            }
        }

        private void RefreshQuickSlots()
        {
            if (_quickSlotViews == null || _playerInventoryModel == null)
            {
                return;
            }

            for (int i = 0; i < _quickSlotViews.Length; i++)
            {
                QuickSlotView slotView = _quickSlotViews[i];
                if (slotView == null)
                {
                    continue;
                }

                InventoryItemModel itemModel = _playerInventoryModel.GetQuickSlotItem(i);
                slotView.SetSlot(i, itemModel, this);
            }
        }

        private void RefreshMessage(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _messageText.text = message;
        }

        private void SetVisible(bool isVisible)
        {
            _isOpen = isVisible;
            if (_rootObject == null)
            {
                gameObject.SetActive(isVisible);
                return;
            }

            _rootObject.SetActive(isVisible);
        }


    }
}
