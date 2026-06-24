using UnityEngine;
using UnityEngine.UI;
using System.Text;

// HUD, 인벤토리, 제작 UI, 보상 선택 UI, Run Result 패널을 열고 닫는 UI 허브입니다.
// UI 계산과 게임 규칙은 각 전용 시스템에서 처리하고, 이 클래스는 화면 전환과 커서 상태를 관리합니다.
namespace Cinderkeep.Gameplay
{
    public sealed class UIManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private GameObject _hudRoot;
        [SerializeField] private GameObject _inventoryRoot;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private Text _runResultText;
        [SerializeField] private InventoryUI _inventoryUI;
        [SerializeField] private CraftingUI _craftingUI;
        [SerializeField] private FurnaceUI _furnaceUI;
        [SerializeField] private CinderHeartSkillSelectionUI _cinderHeartSkillSelectionUI;

        private bool _isInitialized;
        private bool _isRunResultPanelOpen;

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
            RefreshRunResultTextFromTracker(isClear);
            SetActive(_gameOverPanel, true);
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
            SetActive(_gameOverPanel, false);
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
            RunResultSnapshot snapshot = tracker == null ? CreateFallbackRunResultSnapshot(isClear, gameRunModel) : tracker.CreateSnapshot(isClear, gameRunModel);
            _runResultText.text = BuildRunResultText(snapshot);
        }

        private RunResultSnapshot CreateFallbackRunResultSnapshot(bool isClear, GameRunModel gameRunModel)
        {
            RunResultSnapshot snapshot = new RunResultSnapshot();
            snapshot.IsClear = isClear;
            snapshot.ReachedDay = gameRunModel == null ? 0 : gameRunModel.Day;
            snapshot.FailureReason = isClear ? "Clear" : "CinderHeart destroyed";
            return snapshot;
        }

        private string BuildRunResultText(RunResultSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(snapshot.IsClear ? "CLEAR" : "GAME OVER");
            builder.AppendLine(snapshot.IsClear ? "CinderHeart defended." : "CinderHeart destroyed.");
            builder.AppendLine();
            builder.AppendLine("Result");
            builder.AppendLine("Reached Day: " + snapshot.ReachedDay);
            builder.AppendLine("Survival Time: " + FormatTime(snapshot.SurvivalSeconds));
            builder.AppendLine("Failure Reason: " + snapshot.FailureReason);
            builder.AppendLine();
            builder.AppendLine("Combat");
            builder.AppendLine("Monster Kills: " + snapshot.MonsterKillCount);
            builder.AppendLine("Boss Defeated: " + FormatBool(snapshot.BossDefeated));
            builder.AppendLine("Damage Dealt: " + FormatNumber(snapshot.EnemyDamageDealt));
            builder.AppendLine("Tower Damage Dealt: " + FormatNumber(snapshot.TowerDamageDealt));
            builder.AppendLine("Trap Damage Dealt: " + FormatNumber(snapshot.TrapDamageDealt));
            builder.AppendLine("Player Damage Taken: " + FormatNumber(snapshot.PlayerDamageTaken));
            builder.AppendLine("Player Downs: " + snapshot.PlayerDownCount);
            builder.AppendLine("CinderHeart Damage Taken: " + FormatNumber(snapshot.CinderHeartDamageTaken));
            builder.AppendLine();
            builder.AppendLine("Resources");
            builder.AppendLine("Wood: " + snapshot.WoodGained + " / Stone: " + snapshot.StoneGained);
            builder.AppendLine("Iron: " + snapshot.IronGained + " / Gold: " + snapshot.GoldGained);
            builder.AppendLine("Mithril: " + snapshot.MithrilGained + " / Adamantium: " + snapshot.AdamantiumGained);
            builder.AppendLine();
            builder.AppendLine("Crafting / Building");
            builder.AppendLine("Crafted Items: " + snapshot.CraftedItemCount);
            builder.AppendLine("Placed Buildings: " + snapshot.PlacedBuildingCount);
            builder.AppendLine("Trap CC Score: " + FormatNumber(snapshot.TrapCrowdControlScore));
            builder.AppendLine();
            builder.AppendLine("CinderHeart Upgrades");
            builder.AppendLine(FormatSelectedSkills(snapshot));
            builder.AppendLine();
            builder.AppendLine("R: Restart");
            builder.AppendLine("Esc: Main_Lobby");
            return builder.ToString();
        }

        private string FormatSelectedSkills(RunResultSnapshot snapshot)
        {
            if (snapshot.SelectedCinderHeartSkillNames == null || snapshot.SelectedCinderHeartSkillNames.Count <= 0)
            {
                return "None";
            }

            return string.Join(", ", snapshot.SelectedCinderHeartSkillNames);
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainSeconds = totalSeconds % 60;
            return minutes.ToString("00") + ":" + remainSeconds.ToString("00");
        }

        private string FormatNumber(float value)
        {
            return Mathf.RoundToInt(value).ToString();
        }

        private string FormatBool(bool value)
        {
            return value ? "Yes" : "No";
        }

        private void RefreshRunResultText(bool isClear)
        {
            ConnectRunResultText();
            if (_runResultText == null)
            {
                return;
            }

            GameRunModel gameRunModel = GameManager.Inst == null ? null : GameManager.Inst.GameRunModel;
            int day = gameRunModel == null ? 0 : gameRunModel.Day;
            string title = isClear ? "CLEAR" : "GAME OVER";
            string summary = isClear ? "CinderHeart를 지켜냈습니다." : "CinderHeart가 꺼졌습니다.";

            _runResultText.text =
                title + "\n" +
                summary + "\n\n" +
                "도달 날짜: " + day + "일차\n" +
                "상세 기록: 5.00 Run Result 확장 단계에서 집계 예정\n\n" +
                "R: 다시 시작\n" +
                "Esc: Main_Lobby로 돌아가기";
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
