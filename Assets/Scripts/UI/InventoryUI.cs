using TMPro;
using UnityEngine;

// 인벤토리, 장비 슬롯, 퀵슬롯을 표시하고 플레이어의 장착 요청을 모델로 전달합니다.
// UI는 게임 규칙을 직접 소유하지 않고 PlayerInventoryModel / PlayerEquipmentModel 상태만 반영합니다.
namespace Cinderkeep.Gameplay
{
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
        [Tooltip("제작 창 안에 붙을 때 숨길 장식/배경 오브젝트입니다.")]
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
            RefreshMessage("장비 슬롯이나 퀵슬롯을 선택하세요.");
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
                RefreshMessage("인벤토리 모델 연결이 필요합니다.");
                return;
            }

            bool isEquipped = _playerInventoryModel.TryMoveInventoryToEquipmentSlot(
                _draggingInventorySlot.SlotIndex,
                equipmentSlotView.SlotType,
                _playerEquipmentModel);

            if (isEquipped)
            {
                PlayUiSuccessSfx();
                RefreshMessage("장비 슬롯에 장착했습니다.");
                RefreshUI();
                return;
            }

            PlayUiFailSfx();
            RefreshMessage("이 아이템은 선택한 장비 슬롯에 장착할 수 없습니다.");
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
                RefreshMessage("인벤토리 모델 연결이 필요합니다.");
                return;
            }

            InventoryItemModel sourceItem = _playerInventoryModel.GetInventoryItem(_draggingInventorySlot.SlotIndex);
            bool isMoved = TryMoveInventoryItemToQuickSlot(sourceItem, quickSlotView.SlotIndex);

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
                RefreshMessage("인벤토리 모델 연결이 필요합니다.");
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
            InventorySlotRefreshPresenter.RefreshEquipmentSlots(_equipmentSlotViews, _playerEquipmentModel, this);
            InventorySlotRefreshPresenter.RefreshInventorySlots(_inventorySlotViews, _playerInventoryModel, this);
            InventorySlotRefreshPresenter.RefreshQuickSlots(_quickSlotViews, _playerInventoryModel, this);
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
                string weaponItemId = itemModel.ItemId;
                InventoryItemType weaponItemType = itemModel.ItemType;
                int weaponAmount = itemModel.Amount;
                bool isEquipped = TryEquipToEquipmentSlot(inventorySlotIndex, EquipmentSlotType.Weapon);
                int quickSlotIndex;
                TryAssignQuickSlotShortcut(
                    weaponItemId,
                    weaponItemType,
                    weaponAmount,
                    QuickSlotAssignmentPolicy.WeaponPreferredSlotIndex,
                    out quickSlotIndex);
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
                int preferredSlotIndex = QuickSlotAssignmentPolicy.GetPreferredSlotIndex(itemModel);
                int quickSlotIndex;
                if (TryAssignQuickSlotShortcut(itemModel, preferredSlotIndex, out quickSlotIndex) == false)
                {
                    return false;
                }

                EquipToolNow(itemModel.ItemId);
                RefreshMessage((quickSlotIndex + 1).ToString() + "번 퀵슬롯에 도구를 연결하고 장착했습니다.");
                return true;
            }

            if (itemModel.ItemType == InventoryItemType.Food)
            {
                int movedAmount = itemModel.Amount;
                int quickSlotIndex;
                if (TryAddStackableQuickSlotItem(
                    itemModel,
                    QuickSlotAssignmentPolicy.FoodPreferredSlotIndex,
                    out quickSlotIndex) == false)
                {
                    return false;
                }

                _playerInventoryModel.TryConsumeItem(itemModel.ItemId, movedAmount);
                RefreshMessage((quickSlotIndex + 1).ToString() + "번 퀵슬롯에 음식을 옮겼습니다.");
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
                QuickSlotAssignmentPolicy.ReplacementSlotIndex,
                out quickSlotIndex);
        }

        private bool TryAssignQuickSlotShortcut(
            string itemId,
            InventoryItemType itemType,
            int amount,
            int preferredSlotIndex,
            out int quickSlotIndex)
        {
            quickSlotIndex = -1;
            if (_playerInventoryModel == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            {
                return false;
            }

            return _playerInventoryModel.TryAssignQuickSlotShortcut(
                itemId,
                itemType,
                amount,
                preferredSlotIndex,
                QuickSlotAssignmentPolicy.ReplacementSlotIndex,
                out quickSlotIndex);
        }

        private bool TryAddStackableQuickSlotItem(InventoryItemModel itemModel, int preferredSlotIndex, out int quickSlotIndex)
        {
            quickSlotIndex = -1;
            if (_playerInventoryModel == null || itemModel == null || itemModel.IsEmpty)
            {
                return false;
            }

            return _playerInventoryModel.TryAddQuickSlotItem(
                itemModel.ItemId,
                itemModel.ItemType,
                itemModel.Amount,
                preferredSlotIndex,
                QuickSlotAssignmentPolicy.ReplacementSlotIndex,
                out quickSlotIndex);
        }

        private bool TryMoveInventoryItemToQuickSlot(InventoryItemModel itemModel, int quickSlotIndex)
        {
            if (_draggingInventorySlot == null || itemModel == null || itemModel.IsEmpty)
            {
                return false;
            }

            if (itemModel.ItemType != InventoryItemType.Food)
            {
                return _playerInventoryModel.TryMoveInventoryToQuickSlot(
                    _draggingInventorySlot.SlotIndex,
                    quickSlotIndex);
            }

            int movedAmount = itemModel.Amount;
            int resolvedQuickSlotIndex;
            if (TryAddStackableQuickSlotItem(itemModel, quickSlotIndex, out resolvedQuickSlotIndex) == false)
            {
                return false;
            }

            return _playerInventoryModel.TryConsumeItem(itemModel.ItemId, movedAmount);
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

        // 제작 창 안에서 인벤토리 UI를 재사용할 때 불필요한 장식 오브젝트를 숨깁니다.
        public void OpenEmbedded()
        {
            Open();
            SetEmbeddedObjectsVisible(false);
        }

        // 단독 인벤토리 창으로 열 때 숨겼던 장식 오브젝트를 다시 표시합니다.
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
