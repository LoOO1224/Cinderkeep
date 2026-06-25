using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cinderkeep.UI
{
    // Main_Lobby 버튼 입력을 처리하고 게임 씬 진입 모드를 선택합니다.
    // BGM은 MainMenuBgmController가 맡고, 이 클래스는 화면 전환과 설정창 표시만 담당합니다.
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string UiClickClipPath = "Cinderkeep/audio/sfx/sfx_ui_click";
        private const string UiBackClipPath = "Cinderkeep/audio/sfx/sfx_ui_back";
        private const string TestFastButtonName = "Button_TestFastMode";
        private const string TestFastButtonLabel = "테스트초고속 모드";

        [SerializeField] private string _gameSceneName = "Cinderkeep_Game";
        [SerializeField] private bool _createMissingTestFastModeButton = true;

        [FormerlySerializedAs("Button_StartGame")]
        [SerializeField] private Button _startGameButton;
        [FormerlySerializedAs("Button_TestFastMode")]
        [SerializeField] private Button _testFastModeButton;
        [FormerlySerializedAs("Button_Settings")]
        [SerializeField] private Button _settingsButton;
        [FormerlySerializedAs("Button_QuitGame")]
        [SerializeField] private Button _quitGameButton;
        [FormerlySerializedAs("GameObject_SettingsPanel")]
        [SerializeField] private GameObject _settingsPanel;
        [FormerlySerializedAs("Button_CloseSettings")]
        [SerializeField] private Button _closeSettingsButton;

        private AudioSource _effectAudioSource;
        private AudioClip _uiClickClip;
        private AudioClip _uiBackClip;
        private bool _hasStarted;

        private void Start()
        {
            InitializeSfx();
            EnsureTestFastModeButton();
            InitializeButtons();
            CloseSettings();
            _hasStarted = true;
        }

        private void OnDestroy()
        {
            ReleaseButtons();
        }

        public void SetReferences(
            Button buttonStartGame,
            Button buttonTestFastMode,
            Button buttonSettings,
            Button buttonQuitGame,
            GameObject gameObjectSettingsPanel,
            Button buttonCloseSettings)
        {
            _startGameButton = buttonStartGame;
            _testFastModeButton = buttonTestFastMode;
            _settingsButton = buttonSettings;
            _quitGameButton = buttonQuitGame;
            _settingsPanel = gameObjectSettingsPanel;
            _closeSettingsButton = buttonCloseSettings;
        }

        public void SetReferences(
            Button buttonStartGame,
            Button buttonSettings,
            Button buttonQuitGame,
            GameObject gameObjectSettingsPanel,
            Button buttonCloseSettings)
        {
            SetReferences(buttonStartGame, null, buttonSettings, buttonQuitGame, gameObjectSettingsPanel, buttonCloseSettings);
        }

        public void StartGame()
        {
            LoadGameScene(GameLaunchMode.Normal);
        }

        public void StartTestFastMode()
        {
            LoadGameScene(GameLaunchMode.TestFast);
        }

        public void OpenSettings()
        {
            if (_settingsPanel == null)
            {
                return;
            }

            PlayUiClickSfx();
            _settingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (_settingsPanel == null)
            {
                return;
            }

            if (_hasStarted)
            {
                PlayUiBackSfx();
            }

            _settingsPanel.SetActive(false);
        }

        public void QuitGame()
        {
            PlayUiClickSfx();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LoadGameScene(GameLaunchMode launchMode)
        {
            if (string.IsNullOrEmpty(_gameSceneName))
            {
                Debug.LogWarning("MainMenuController: game scene name is empty.");
                return;
            }

            if (launchMode == GameLaunchMode.TestFast)
            {
                GameLaunchSettings.SetTestFastMode();
            }
            else
            {
                GameLaunchSettings.SetNormalMode();
            }

            PlayUiClickSfx();
            SceneManager.LoadScene(_gameSceneName);
        }

        private void InitializeButtons()
        {
            if (_startGameButton != null)
            {
                _startGameButton.onClick.AddListener(StartGame);
            }

            if (_testFastModeButton != null)
            {
                _testFastModeButton.onClick.AddListener(StartTestFastMode);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OpenSettings);
            }

            if (_quitGameButton != null)
            {
                _quitGameButton.onClick.AddListener(QuitGame);
            }

            if (_closeSettingsButton != null)
            {
                _closeSettingsButton.onClick.AddListener(CloseSettings);
            }
        }

        private void ReleaseButtons()
        {
            if (_startGameButton != null)
            {
                _startGameButton.onClick.RemoveListener(StartGame);
            }

            if (_testFastModeButton != null)
            {
                _testFastModeButton.onClick.RemoveListener(StartTestFastMode);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveListener(OpenSettings);
            }

            if (_quitGameButton != null)
            {
                _quitGameButton.onClick.RemoveListener(QuitGame);
            }

            if (_closeSettingsButton != null)
            {
                _closeSettingsButton.onClick.RemoveListener(CloseSettings);
            }
        }

        private void EnsureTestFastModeButton()
        {
            if (_testFastModeButton == null)
            {
                _testFastModeButton = FindButtonInChildren(TestFastButtonName);
            }

            if (_testFastModeButton != null || _createMissingTestFastModeButton == false || _startGameButton == null)
            {
                return;
            }

            _testFastModeButton = Instantiate(_startGameButton, _startGameButton.transform.parent);
            _testFastModeButton.name = TestFastButtonName;
            _testFastModeButton.onClick = new Button.ButtonClickedEvent();
            SetButtonLabel(_testFastModeButton, TestFastButtonLabel);
            ApplyDefaultButtonLayout();
        }

        private Button FindButtonInChildren(string objectName)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null && buttons[i].name == objectName)
                {
                    return buttons[i];
                }
            }

            return null;
        }

        private void ApplyDefaultButtonLayout()
        {
            SetButtonAnchors(_startGameButton, new Vector2(0.36f, 0.53f), new Vector2(0.64f, 0.61f));
            SetButtonAnchors(_testFastModeButton, new Vector2(0.36f, 0.42f), new Vector2(0.64f, 0.50f));
            SetButtonAnchors(_settingsButton, new Vector2(0.36f, 0.31f), new Vector2(0.64f, 0.39f));
            SetButtonAnchors(_quitGameButton, new Vector2(0.36f, 0.20f), new Vector2(0.64f, 0.28f));
        }

        private void SetButtonAnchors(Button button, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
            }

            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                tmpText.text = label;
            }
        }

        private void InitializeSfx()
        {
            _effectAudioSource = GetComponent<AudioSource>();
            if (_effectAudioSource == null)
            {
                _effectAudioSource = gameObject.AddComponent<AudioSource>();
            }

            _effectAudioSource.playOnAwake = false;
            _effectAudioSource.loop = false;
            _effectAudioSource.spatialBlend = 0f;
            _effectAudioSource.volume = 0.7f;

            _uiClickClip = Resources.Load<AudioClip>(UiClickClipPath);
            _uiBackClip = Resources.Load<AudioClip>(UiBackClipPath);
        }

        private void PlayUiClickSfx()
        {
            PlaySfx(_uiClickClip);
        }

        private void PlayUiBackSfx()
        {
            PlaySfx(_uiBackClip);
        }

        private void PlaySfx(AudioClip audioClip)
        {
            if (_effectAudioSource == null || audioClip == null)
            {
                return;
            }

            _effectAudioSource.PlayOneShot(audioClip);
        }
    }
}
