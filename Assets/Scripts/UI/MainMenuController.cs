using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cinderkeep.UI
{
    // 메인 메뉴의 큰 화면 흐름만 담당합니다.
    // 실제 BGM 조절은 MainMenuBgmController가 담당합니다.
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string _gameSceneName = "Cinderkeep_Game";
        [SerializeField] private Button Button_StartGame;
        [SerializeField] private Button Button_Settings;
        [SerializeField] private Button Button_QuitGame;
        [SerializeField] private GameObject GameObject_SettingsPanel;
        [SerializeField] private Button Button_CloseSettings;

        private void Start()
        {
            InitializeButtons();
            CloseSettings();
        }

        private void OnDestroy()
        {
            ReleaseButtons();
        }

        public void SetReferences(Button buttonStartGame, Button buttonSettings, Button buttonQuitGame, GameObject gameObjectSettingsPanel, Button buttonCloseSettings)
        {
            Button_StartGame = buttonStartGame;
            Button_Settings = buttonSettings;
            Button_QuitGame = buttonQuitGame;
            GameObject_SettingsPanel = gameObjectSettingsPanel;
            Button_CloseSettings = buttonCloseSettings;
        }

        public void StartGame()
        {
            if (string.IsNullOrEmpty(_gameSceneName))
            {
                Debug.LogWarning("MainMenuController: game scene name is empty.");
                return;
            }

            SceneManager.LoadScene(_gameSceneName);
        }

        public void OpenSettings()
        {
            if (GameObject_SettingsPanel == null)
            {
                return;
            }

            GameObject_SettingsPanel.SetActive(true);
        }

        public void CloseSettings()
        {
            if (GameObject_SettingsPanel == null)
            {
                return;
            }

            GameObject_SettingsPanel.SetActive(false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void InitializeButtons()
        {
            if (Button_StartGame != null)
            {
                Button_StartGame.onClick.AddListener(StartGame);
            }

            if (Button_Settings != null)
            {
                Button_Settings.onClick.AddListener(OpenSettings);
            }

            if (Button_QuitGame != null)
            {
                Button_QuitGame.onClick.AddListener(QuitGame);
            }

            if (Button_CloseSettings != null)
            {
                Button_CloseSettings.onClick.AddListener(CloseSettings);
            }
        }

        private void ReleaseButtons()
        {
            if (Button_StartGame != null)
            {
                Button_StartGame.onClick.RemoveListener(StartGame);
            }

            if (Button_Settings != null)
            {
                Button_Settings.onClick.RemoveListener(OpenSettings);
            }

            if (Button_QuitGame != null)
            {
                Button_QuitGame.onClick.RemoveListener(QuitGame);
            }

            if (Button_CloseSettings != null)
            {
                Button_CloseSettings.onClick.RemoveListener(CloseSettings);
            }
        }
    }
}
