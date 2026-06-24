using TMPro;
using UnityEngine;

// 5.00 direction: Displays or controls UI for the 5.00 playable loop without owning gameplay rules.
// 5.01+ note: Keep UI as a view/controller layer; read models and dispatch requests instead of duplicating game logic.
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

        [Header("Crafting Embed")]
        [Tooltip("크래프팅 창 안에 띄울 때 숨길 오브젝트들(배경, 타이틀 등)")]
        [SerializeField] private GameObject[] _hideWhenEmbedded;

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
            SetEmbeddedObjectsVisible(true);
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
                PlayUiFailSfx();
                RefreshMessage("인벤토리 모델 연결이 준비되지 않았습니다.");
                return;
            }

            bool isEquipped = _playerInventoryModel.TryMoveInventoryToEquipmentSlot(
                _draggingInventorySlot.SlotIndex,
                equipmentSlotView.SlotType,
                _playerEquipmentModel);

            if (isEquipped)
            {
                PlayUiSuccessSfx();
                RefreshMessage("장비 칸에 연결했습니다.");
                RefreshUI();
                return;
            }

            PlayUiFailSfx();
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
                PlayUiFailSfx();
                RefreshMessage("인벤토리 모델 연결이 준비되지 않았습니다.");
                return;
            }

            bool isMoved = _playerInventoryModel.TryMoveInventoryToQuickSlot(
                _draggingInventorySlot.SlotIndex,
                quickSlotView.SlotIndex);

            if (isMoved)
            {
                PlayUiSuccessSfx();
                RefreshMessage("퀵슬롯에 등록했습니다.");
                RefreshUI();
                return;
            }

            PlayUiFailSfx();
            RefreshMessage("퀵슬롯에 등록할 수 없습니다.");
        }

        public void DoubleClickInventorySlot(InventorySlotView slotView)
        {
            if (slotView == null)
            {
                return;
            }

            ConnectModels();
            if (_playerInventoryModel == null)
            {
                PlayUiFailSfx();
                RefreshMessage("인벤토리 연결이 필요합니다.");
                return;
            }

            InventoryItemModel itemModel = _playerInventoryModel.GetInventoryItem(slotView.SlotIndex);
            if (itemModel == null || itemModel.IsEmpty)
            {
                return;
            }

            if (TryAutoEquipInventoryItem(slotView.SlotIndex, itemModel))
            {
                PlayUiSuccessSfx();
                RefreshUI();
                return;
            }

            PlayUiFailSfx();
            RefreshMessage("이 아이템은 바로 장착할 수 없습니다.");
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

        private bool TryAutoEquipInventoryItem(int inventorySlotIndex, InventoryItemModel itemModel)
        {
            if (itemModel.ItemType == InventoryItemType.Weapon)
            {
                bool isEquipped = TryEquipToEquipmentSlot(inventorySlotIndex, EquipmentSlotType.Weapon);
                int quickSlotIndex;
                TryAssignQuickSlotShortcut(itemModel, 0, out quickSlotIndex);
                RefreshMessage("무기를 장착하고 1번 퀵슬롯에 연결했습니다.");
                return isEquipped;
            }

            if (itemModel.ItemType == InventoryItemType.Helmet)
            {
                bool isEquipped = TryEquipToEquipmentSlot(inventorySlotIndex, EquipmentSlotType.Helmet);
                RefreshMessage("헬멧을 장착했습니다.");
                return isEquipped;
            }

            if (itemModel.ItemType == InventoryItemType.Armor)
            {
                bool isEquipped = TryEquipToEquipmentSlot(inventorySlotIndex, EquipmentSlotType.Armor);
                RefreshMessage("갑옷을 장착했습니다.");
                return isEquipped;
            }

            if (itemModel.ItemType == InventoryItemType.Boots)
            {
                bool isEquipped = TryEquipToEquipmentSlot(inventorySlotIndex, EquipmentSlotType.Boots);
                RefreshMessage("신발을 장착했습니다.");
                return isEquipped;
            }

            if (itemModel.ItemType == InventoryItemType.Tool)
            {
                int preferredSlotIndex = ResolveToolPreferredQuickSlot(itemModel.ItemId);
                int quickSlotIndex;
                if (TryAssignQuickSlotShortcut(itemModel, preferredSlotIndex, out quickSlotIndex) == false)
                {
                    return false;
                }

                EquipToolNow(itemModel.ItemId);
                RefreshMessage((quickSlotIndex + 1).ToString() + "번 퀵슬롯에 도구를 연결하고 장착했습니다.");
                return true;
            }

            return false;
        }

        private bool TryEquipToEquipmentSlot(int inventorySlotIndex, EquipmentSlotType slotType)
        {
            if (_playerInventoryModel == null || _playerEquipmentModel == null)
            {
                return false;
            }

            return _playerInventoryModel.TryMoveInventoryToEquipmentSlot(
                inventorySlotIndex,
                slotType,
                _playerEquipmentModel);
        }

        private bool TryAssignQuickSlotShortcut(InventoryItemModel itemModel, int preferredSlotIndex, out int quickSlotIndex)
        {
            quickSlotIndex = -1;
            if (_playerInventoryModel == null || itemModel == null || itemModel.IsEmpty)
            {
                return false;
            }

            return _playerInventoryModel.TryAssignQuickSlotShortcut(
                itemModel.ItemId,
                itemModel.ItemType,
                itemModel.Amount,
                preferredSlotIndex,
                PlayerInventoryModel.QuickSlotCount - 1,
                out quickSlotIndex);
        }

        private int ResolveToolPreferredQuickSlot(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return 1;
            }

            if (itemId == global::PlayerToolController.HandStoneToolDataId)
            {
                return 0;
            }

            if (itemId.IndexOf("pickaxe", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2;
            }

            if (itemId.IndexOf("axe", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1;
            }

            return 4;
        }

        private void EquipToolNow(string toolDataId)
        {
            global::PlayerToolController toolController = FindFirstObjectByType<global::PlayerToolController>();
            if (toolController == null)
            {
                return;
            }

            toolController.EquipToolData(toolDataId);
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

        // 크래프팅 창 안에 끼워 넣는 용도로 여는 메서드
        public void OpenEmbedded()
        {
            Open();
            SetEmbeddedObjectsVisible(false);
        }

        // 다시 단독으로 열 때 원상 복구
        public void OpenStandalone()
        {
            Open();
            SetEmbeddedObjectsVisible(true);
        }

        private void SetEmbeddedObjectsVisible(bool isVisible)
        {
            if (_hideWhenEmbedded == null)
            {
                return;
            }

            for (int i = 0; i < _hideWhenEmbedded.Length; i++)
            {
                if (_hideWhenEmbedded[i] == null)
                {
                    continue;
                }

                _hideWhenEmbedded[i].SetActive(isVisible);
            }
        }

        private void PlayUiSuccessSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayUiSuccess();
        }

        private void PlayUiFailSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayUiFail();
        }

        private SoundManager GetSoundManager()
        {
            if (GameManager.Inst == null)
            {
                return null;
            }

            return GameManager.Inst.GetSoundManager();
        }
    }
}
