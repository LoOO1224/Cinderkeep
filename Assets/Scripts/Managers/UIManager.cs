using UnityEngine;
using UnityEngine.UI;

// HUD, 인벤토리, 제작 UI, 보상 선택 UI, Run Result 패널을 열고 닫는 UI 허브입니다.
// 게임 규칙은 전용 시스템이 담당하고, 이 클래스는 화면 전환과 커서 상태, UI 사운드 호출만 조율합니다.
namespace Cinderkeep.Gameplay
{
    public sealed class UIManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private GameObject _hudRoot;
        [SerializeField] private GameObject _inventoryRoot;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text _runResultText;
        [SerializeField] private RunResultUI _runResultUI;
        [SerializeField] private InventoryUI _inventoryUI;
        [SerializeField] private CraftingUI _craftingUI;
        [SerializeField] private FurnaceUI _furnaceUI;
        [SerializeField] private CinderHeartSkillSelectionUI _cinderHeartSkillSelectionUI;

        private bool _isInitialized;
        private bool _isRunResultPanelOpen;
        private global::PlayerController _playerController;

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
            if (_isRunResultPanelOpen)
            {
                UpdateRunResultInput();
                return;
            }

            if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.Tab))
            {
                if (_craftingUI != null && _craftingUI.IsOpen)
                {
                    CloseCraftingUI();
                    return;
                }

                ToggleInventory();
            }
        }

        public void OpenHud()
        {
            global::HudTutorialGuide.EnsureSceneGuide();
            QuickSlotHud.EnsureSceneHud();
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
            RefreshCursorState();
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
            RefreshCursorState();
        }

        public void ToggleInventory()
        {
            if (_inventoryUI != null)
            {
                bool wasOpen = _inventoryUI.IsOpen;
                _inventoryUI.Toggle();
                RefreshCursorState();
                PlayUiToggleSfx(wasOpen);

                return;
            }

            if (_inventoryRoot == null)
            {
                return;
            }

            bool wasRootOpen = _inventoryRoot.activeSelf;
            SetActive(_inventoryRoot, wasRootOpen == false);
            RefreshCursorState();
            PlayUiToggleSfx(wasRootOpen);
        }

        public void OpenGameOverPanel()
        {
            OpenRunResultPanel(false);
        }

        public void OpenClearPanel()
        {
            OpenRunResultPanel(true);
        }

        public void OpenRunResultPanel(bool isClear)
        {
            CloseInventory();
            CloseCraftingUI();
            CloseFurnaceUI();
            CloseCinderHeartSkillSelectionUI();
            CloseHud();

            _isRunResultPanelOpen = true;
            OpenRunResultUI(isClear);
            RefreshCursorState();

            if (isClear)
            {
                PlayUiNotificationSfx();
                return;
            }

            PlayUiFailSfx();
        }

        public void CloseGameOverPanel()
        {
            _isRunResultPanelOpen = false;
            if (_runResultUI != null)
            {
                _runResultUI.Close();
            }
            else
            {
                SetActive(_gameOverPanel, false);
            }

            RefreshCursorState();
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
            PlayUiClickSfx();
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
            bool wasOpen = _craftingUI.IsOpen;
            _craftingUI.Toggle(craftingStation, interactor);
            RefreshCursorState();
            PlayUiToggleSfx(wasOpen);
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
            PlayUiClickSfx();
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
            PlayUiNotificationSfx();
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
            RefreshPlayerControlState(anyUIOpen);

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

            if (_inventoryUI == null && _inventoryRoot != null && _inventoryRoot.activeSelf)
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

            if (_isRunResultPanelOpen)
            {
                return true;
            }

            return false;
        }

        private void RefreshPlayerControlState(bool isBlockingUiOpen)
        {
            if (_playerController == null)
            {
                _playerController = Object.FindFirstObjectByType<global::PlayerController>();
            }

            if (_playerController == null)
            {
                return;
            }

            if (_playerController.CurrentState == global::PlayerControlState.Dead)
            {
                return;
            }

            if (isBlockingUiOpen)
            {
                _playerController.SetState(global::PlayerControlState.OpenUI);
                return;
            }

            if (_playerController.CurrentState == global::PlayerControlState.OpenUI)
            {
                _playerController.SetState(global::PlayerControlState.Normal);
            }
        }

        private void UpdateRunResultInput()
        {
            if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.R))
            {
                RestartRunFromResult();
                return;
            }

            if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.Escape))
            {
                ReturnToMainLobbyFromResult();
            }
        }

        private void RestartRunFromResult()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            CloseGameOverPanel();
            PlayUiClickSfx();
            GameManager.Inst.RestartRun();
        }

        private void ReturnToMainLobbyFromResult()
        {
            if (GameManager.Inst == null)
            {
                return;
            }

            PlayUiBackSfx();
            GameManager.Inst.ReturnToMainLobby();
        }

        private void RefreshRunResultTextFromTracker(bool isClear)
        {
            ConnectRunResultText();
            if (_runResultText == null)
            {
                return;
            }

            GameRunModel gameRunModel = GameManager.Inst == null ? null : GameManager.Inst.GameRunModel;
            RunResultTracker tracker = RunResultTracker.Instance;
            RunResultSnapshot snapshot = tracker == null
                ? RunResultTextFormatter.CreateFallbackSnapshot(isClear, gameRunModel)
                : tracker.CreateSnapshot(isClear, gameRunModel);
            _runResultText.text = RunResultTextFormatter.BuildText(snapshot);
        }

        private void OpenRunResultUI(bool isClear)
        {
            ConnectRunResultUI();
            if (_runResultUI != null)
            {
                _runResultUI.Open(isClear);
                return;
            }

            RefreshRunResultTextFromTracker(isClear);
            SetActive(_gameOverPanel, true);
        }

        private void ConnectRunResultUI()
        {
            if (_runResultUI != null || _gameOverPanel == null)
            {
                return;
            }

            _runResultUI = _gameOverPanel.GetComponent<RunResultUI>();
            if (_runResultUI == null)
            {
                _runResultUI = _gameOverPanel.AddComponent<RunResultUI>();
            }
        }

        private void ConnectRunResultText()
        {
            if (_runResultText != null || _gameOverPanel == null)
            {
                return;
            }

            _runResultText = _gameOverPanel.GetComponentInChildren<Text>(true);
        }

        private void PlayUiToggleSfx(bool wasOpen)
        {
            if (wasOpen)
            {
                PlayUiBackSfx();
                return;
            }

            PlayUiClickSfx();
        }

        private void PlayUiClickSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayUiClick();
        }

        private void PlayUiBackSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayUiBack();
        }

        private void PlayUiNotificationSfx()
        {
            SoundManager soundManager = GetSoundManager();
            if (soundManager == null)
            {
                return;
            }

            soundManager.PlayUiNotification();
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
