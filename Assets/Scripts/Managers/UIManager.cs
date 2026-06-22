using UnityEngine;

using UnityEngine.Serialization;

namespace Cinderkeep.Gameplay
{
    // 게임 씬 전용 UI 매니저입니다.
    // 현재는 3일 게임 루프에 필요한 HUD, 인벤토리, 게임오버 UI부터 관리합니다.
    // UI는 코드에서 새로 만들지 않고, 씬이나 프리팹에 준비된 오브젝트를 켜고 끄는 방식으로 관리합니다.
    public sealed class UIManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private GameObject _hudRoot;
        [SerializeField] private GameObject _inventoryRoot;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private InventoryUI _inventoryUI;
        [SerializeField] private CraftingUI _craftingUI;
        [SerializeField] private FurnaceUI _furnaceUI;
        [SerializeField] private CinderHeartSkillSelectionUI _cinderHeartSkillSelectionUI;

        private bool _isInitialized;

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            CloseHud();
            CloseInventory();
            CloseGameOverPanel();
            CloseCraftingUI();
            CloseFurnaceUI();
            CloseCinderHeartSkillSelectionUI();
            RefreshCursorState();
            _isInitialized = true;
        }

        private void Update()
        {
            if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.Tab))
            {
                ToggleInventory();
            }
        }

        public void OpenHud()
        {
            SetActive(_hudRoot, true);
        }

        public void CloseHud()
        {
            SetActive(_hudRoot, false);
        }

        public void OpenInventory()
        {
            if (_inventoryUI != null)
            {
                _inventoryUI.Open();
                RefreshCursorState();
                return;
            }

            SetActive(_inventoryRoot, true);
        }

        public void CloseInventory()
        {
            if (_inventoryUI != null)
            {
                _inventoryUI.Close();
                RefreshCursorState();

                return;
            }

            SetActive(_inventoryRoot, false);
        }

        public void ToggleInventory()
        {
            if (_inventoryUI != null)
            {
                _inventoryUI.Toggle();
                RefreshCursorState();

                return;
            }

            if (_inventoryRoot == null)
            {
                return;
            }

            SetActive(_inventoryRoot, _inventoryRoot.activeSelf == false);
        }

        public void OpenGameOverPanel()
        {
            SetActive(_gameOverPanel, true);
        }

        public void CloseGameOverPanel()
        {
            SetActive(_gameOverPanel, false);
        }

        public void OpenCraftingUI(CraftingStation craftingStation, GameObject interactor)
        {
            if (_craftingUI == null)
            {
                return;
            }

            CloseFurnaceUI();
            _craftingUI.OpenStation(craftingStation, interactor);
            RefreshCursorState();
        }

        public void CloseCraftingUI()
        {
            if (_craftingUI == null)
            {
                return;
            }

            _craftingUI.Close();
            RefreshCursorState();
        }

        public void ToggleCraftingUI(CraftingStation craftingStation, GameObject interactor)
        {
            if (_craftingUI == null)
            {
                return;
            }

            CloseFurnaceUI();
            _craftingUI.Toggle(craftingStation, interactor);
            RefreshCursorState();
        }

        public void OpenFurnaceUI(FurnaceStation furnaceStation, GameObject interactor)
        {
            if (_furnaceUI == null)
            {
                return;
            }

            CloseCraftingUI();
            _furnaceUI.OpenFurnace(furnaceStation);
            RefreshCursorState();
        }

        public void CloseFurnaceUI()
        {
            if (_furnaceUI == null)
            {
                return;
            }

            _furnaceUI.Close();
            RefreshCursorState();
        }







        public void OpenCinderHeartSkillSelectionUI(
            System.Collections.Generic.IReadOnlyList<CinderHeartSkillData> skillOptions,
            System.Action onClosed)
        {
            if (_cinderHeartSkillSelectionUI == null)
            {
                if (onClosed != null)
                {
                    onClosed.Invoke();
                }

                return;
            }

            CloseInventory();
            CloseCraftingUI();
            CloseFurnaceUI();
            _cinderHeartSkillSelectionUI.Open(skillOptions, onClosed);
        }

        public void CloseCinderHeartSkillSelectionUI()
        {
            if (_cinderHeartSkillSelectionUI == null)
            {
                return;
            }

            _cinderHeartSkillSelectionUI.Close();
        }

        private void SetActive(GameObject targetObject, bool isActive)
        {
            if (targetObject == null)
            {
                return;
            }

            targetObject.SetActive(isActive);
        }

        private void RefreshCursorState()
        {
            bool anyUIOpen = IsAnyBlockingUIOpen();

            if (anyUIOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        private bool IsAnyBlockingUIOpen()
        {
            if (_inventoryUI != null && _inventoryUI.IsOpen)
            {
                return true;
            }

            if (_craftingUI != null && _craftingUI.IsOpen)
            {
                return true;
            }

            if (_furnaceUI != null && _furnaceUI.IsOpen)
            {
                return true;
            }

            return false;
        }
    }
}
