using Cinderkeep.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Cinderkeep.UI.Editor
{
    // Main_Lobby를 실제 게임의 첫 화면으로 다시 만드는 편집기 도구입니다.
    // BGM은 별도 화면 버튼을 두지 않고, 설정창의 체크박스와 슬라이더에서만 관리합니다.
    public static class MainMenuSceneBuilder
    {
        private const string MainMenuScenePath = "Assets/Scenes/Main_Lobby.unity";
        private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
        private const string MainBackgroundPath = "Assets/Arts/MainMenu/MainMenu_Background.png";
        private const string SettingBackgroundPath = "Assets/Arts/MainMenu/Setting_Background.png";
        private const string KoreanFontPath = "Assets/Fonts/ChosunCentennial.ttf";
        private const string BgmFirstPath = "Assets/Audio/BGM/Cinderkeep_BGM_V1.mp3";
        private const string BgmSecondPath = "Assets/Audio/BGM/Cinderkeep_BGM_V1_2.mp3";
        private const float BgmVolume = 0.3f;

        [MenuItem("Cinderkeep/Main Menu/Rebuild Main Menu")]
        public static void RebuildMainMenuScene()
        {
            PrepareSprite(MainBackgroundPath);
            PrepareSprite(SettingBackgroundPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMainCamera();
            CreateEventSystem();
            BgmBuildReferences bgmBuildReferences = CreateBgmController();
            CreateMainMenuCanvas(bgmBuildReferences);
            SaveScene(scene);
            UpdateBuildSettings();
        }

        private static void PrepareSprite(string assetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
            {
                return;
            }

            bool isChanged = false;

            if (textureImporter.textureType != TextureImporterType.Sprite)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                isChanged = true;
            }

            if (textureImporter.spriteImportMode != SpriteImportMode.Single)
            {
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                isChanged = true;
            }

            if (textureImporter.mipmapEnabled)
            {
                textureImporter.mipmapEnabled = false;
                isChanged = true;
            }

            if (isChanged)
            {
                textureImporter.SaveAndReimport();
            }
        }

        private static void CreateMainCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.025f, 0.03f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem_MainMenu");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static BgmBuildReferences CreateBgmController()
        {
            GameObject bgmControllerObject = new GameObject("MainMenu_BgmController");
            AudioSource audioSource = bgmControllerObject.AddComponent<AudioSource>();
            MainMenuBgmController bgmController = bgmControllerObject.AddComponent<MainMenuBgmController>();

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = BgmVolume;

            BgmBuildReferences bgmBuildReferences = new BgmBuildReferences();
            bgmBuildReferences.Controller = bgmController;
            bgmBuildReferences.AudioSourceBgm = audioSource;
            bgmBuildReferences.AudioClipsBgm = LoadBgmClips();
            return bgmBuildReferences;
        }

        private static AudioClip[] LoadBgmClips()
        {
            AudioClip[] bgmClips = new AudioClip[2];
            bgmClips[0] = AssetDatabase.LoadAssetAtPath<AudioClip>(BgmFirstPath);
            bgmClips[1] = AssetDatabase.LoadAssetAtPath<AudioClip>(BgmSecondPath);
            return bgmClips;
        }

        private static void CreateMainMenuCanvas(BgmBuildReferences bgmBuildReferences)
        {
            GameObject canvasObject = new GameObject("Canvas_MainMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Image background = CreateImage("Image_Background", canvasRect, Color.white);
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MainBackgroundPath);
            background.preserveAspect = false;
            Stretch(background.rectTransform);

            RectTransform menuRoot = CreateRect("MainMenuRoot", canvasRect);
            Stretch(menuRoot);

            Font font = LoadFont();
            Image menuBackdrop = CreateImage("Image_MenuReadable_Backdrop", menuRoot, new Color(0.02f, 0.025f, 0.03f, 0.64f));
            SetAnchors(menuBackdrop.rectTransform, new Vector2(0.30f, 0.20f), new Vector2(0.70f, 0.85f));

            CreateText("Text_Title", menuRoot, "Cinderkeep", 88, Color.white, TextAnchor.MiddleCenter, font, new Vector2(0.16f, 0.70f), new Vector2(0.84f, 0.84f));
            CreateText("Text_Subtitle", menuRoot, "불꽃 심장을 지키는 생존 방어 게임", 28, new Color(0.88f, 0.91f, 0.94f, 1f), TextAnchor.MiddleCenter, font, new Vector2(0.16f, 0.63f), new Vector2(0.84f, 0.70f));

            Button startButton = CreateButton("Button_StartGame", menuRoot, "게임 시작하기", 34, new Color(0.20f, 0.48f, 0.32f, 0.96f), font, new Vector2(0.36f, 0.49f), new Vector2(0.64f, 0.57f));
            Button settingsButton = CreateButton("Button_Settings", menuRoot, "설정", 34, new Color(0.18f, 0.30f, 0.46f, 0.96f), font, new Vector2(0.36f, 0.38f), new Vector2(0.64f, 0.46f));
            Button quitButton = CreateButton("Button_QuitGame", menuRoot, "게임 끝내기", 34, new Color(0.46f, 0.16f, 0.16f, 0.96f), font, new Vector2(0.36f, 0.27f), new Vector2(0.64f, 0.35f));

            SettingsPanelReferences settingsPanel = CreateSettingsPanel(menuRoot, font);

            MainMenuController controller = menuRoot.gameObject.AddComponent<MainMenuController>();
            controller.SetReferences(startButton, settingsButton, quitButton, settingsPanel.GameObjectSettingsPanel, settingsPanel.ButtonCloseSettings);
            EditorUtility.SetDirty(controller);

            if (bgmBuildReferences != null && bgmBuildReferences.Controller != null)
            {
                bgmBuildReferences.Controller.SetReferences(
                    bgmBuildReferences.AudioSourceBgm,
                    settingsPanel.ToggleVolumeMute,
                    settingsPanel.SliderVolume,
                    settingsPanel.TextVolumeMute,
                    settingsPanel.TextVolumeValue,
                    bgmBuildReferences.AudioClipsBgm,
                    BgmVolume);
                EditorUtility.SetDirty(bgmBuildReferences.Controller);
            }
        }

        private static SettingsPanelReferences CreateSettingsPanel(Transform parent, Font font)
        {
            Image panelImage = CreateImage("Panel_Settings", parent, Color.white);
            panelImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SettingBackgroundPath);
            panelImage.preserveAspect = false;
            Stretch(panelImage.rectTransform);

            Image readableBox = CreateImage("Image_SettingsReadableBox", panelImage.transform, new Color(0.05f, 0.07f, 0.09f, 0.88f));
            SetAnchors(readableBox.rectTransform, new Vector2(0.31f, 0.22f), new Vector2(0.69f, 0.78f));

            CreateText("Text_SettingsTitle", readableBox.transform, "설정", 48, Color.white, TextAnchor.MiddleCenter, font, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.94f));

            Toggle muteToggle = CreateToggle("Toggle_VolumeMute", readableBox.transform, font, new Vector2(0.14f, 0.56f), new Vector2(0.86f, 0.68f));
            Text muteText = muteToggle.GetComponentInChildren<Text>();

            CreateText("Text_VolumeTitle", readableBox.transform, "볼륨", 28, Color.white, TextAnchor.MiddleLeft, font, new Vector2(0.14f, 0.39f), new Vector2(0.34f, 0.50f));
            Slider volumeSlider = CreateSlider("Slider_Volume", readableBox.transform, new Vector2(0.34f, 0.40f), new Vector2(0.70f, 0.49f));
            Text volumeValueText = CreateText("Text_VolumeValue", readableBox.transform, "3 / 10", 26, Color.white, TextAnchor.MiddleCenter, font, new Vector2(0.72f, 0.38f), new Vector2(0.88f, 0.50f));

            Button closeButton = CreateButton("Button_CloseSettings", readableBox.transform, "돌아가기", 30, new Color(0.48f, 0.34f, 0.12f, 0.96f), font, new Vector2(0.26f, 0.12f), new Vector2(0.74f, 0.24f));

            panelImage.gameObject.SetActive(false);

            SettingsPanelReferences settingsPanelReferences = new SettingsPanelReferences();
            settingsPanelReferences.GameObjectSettingsPanel = panelImage.gameObject;
            settingsPanelReferences.ButtonCloseSettings = closeButton;
            settingsPanelReferences.ToggleVolumeMute = muteToggle;
            settingsPanelReferences.SliderVolume = volumeSlider;
            settingsPanelReferences.TextVolumeMute = muteText;
            settingsPanelReferences.TextVolumeValue = volumeValueText;
            return settingsPanelReferences;
        }

        private static Toggle CreateToggle(string objectName, Transform parent, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject toggleObject = new GameObject(objectName, typeof(RectTransform), typeof(Toggle));
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            toggleRect.SetParent(parent, false);
            SetAnchors(toggleRect, anchorMin, anchorMax);

            Image boxImage = CreateImage("Image_CheckBox", toggleRect, new Color(0.95f, 0.95f, 0.95f, 1f));
            SetAnchors(boxImage.rectTransform, new Vector2(0f, 0.18f), new Vector2(0.12f, 0.82f));

            Image checkImage = CreateImage("Image_CheckMark", boxImage.transform, new Color(0.16f, 0.48f, 0.32f, 1f));
            SetAnchors(checkImage.rectTransform, new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.78f));

            Text label = CreateText("Text_Label", toggleRect, "볼륨 끄기", 26, Color.white, TextAnchor.MiddleLeft, font, new Vector2(0.17f, 0f), new Vector2(1f, 1f));

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = false;
            label.raycastTarget = false;
            return toggle;
        }

        private static Slider CreateSlider(string objectName, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject sliderObject = new GameObject(objectName, typeof(RectTransform), typeof(Slider));
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.SetParent(parent, false);
            SetAnchors(sliderRect, anchorMin, anchorMax);

            Image background = CreateImage("Image_Background", sliderRect, new Color(0.25f, 0.28f, 0.32f, 1f));
            Stretch(background.rectTransform);

            RectTransform fillArea = CreateRect("FillArea", sliderRect);
            SetAnchors(fillArea, new Vector2(0f, 0.25f), new Vector2(1f, 0.75f));

            Image fill = CreateImage("Image_Fill", fillArea, new Color(0.25f, 0.58f, 0.86f, 1f));
            Stretch(fill.rectTransform);

            RectTransform handleArea = CreateRect("HandleArea", sliderRect);
            Stretch(handleArea);

            Image handle = CreateImage("Image_Handle", handleArea, Color.white);
            SetAnchors(handle.rectTransform, new Vector2(0f, 0.05f), new Vector2(0.08f, 0.95f));

            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 10f;
            slider.wholeNumbers = true;
            slider.value = 3f;
            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            return slider;
        }

        private static Font LoadFont()
        {
            return AssetDatabase.LoadAssetAtPath<Font>(KoreanFontPath);
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);

            Image image = gameObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string objectName, Transform parent, string text, int fontSize, Color color, TextAnchor alignment, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Text), typeof(Shadow));
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            SetAnchors(rectTransform, anchorMin, anchorMax);

            Text label = gameObject.GetComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.font = font;

            Shadow shadow = gameObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        private static Button CreateButton(string objectName, Transform parent, string labelText, int labelSize, Color color, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            Image image = CreateImage(objectName, parent, color);
            SetAnchors(image.rectTransform, anchorMin, anchorMax);

            Button button = image.gameObject.AddComponent<Button>();
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = color;
            colorBlock.highlightedColor = new Color(color.r + 0.08f, color.g + 0.08f, color.b + 0.08f, color.a);
            colorBlock.pressedColor = new Color(color.r * 0.75f, color.g * 0.75f, color.b * 0.75f, color.a);
            button.colors = colorBlock;

            CreateText("Text_Label", image.transform, labelText, labelSize, Color.white, TextAnchor.MiddleCenter, font, Vector2.zero, Vector2.one);
            return button;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            SetAnchors(rectTransform, Vector2.zero, Vector2.one);
        }

        private static void SetAnchors(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void SaveScene(Scene scene)
        {
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            AssetDatabase.SaveAssets();
        }

        private static void UpdateBuildSettings()
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[2];
            scenes[0] = new EditorBuildSettingsScene(MainMenuScenePath, true);
            scenes[1] = new EditorBuildSettingsScene(GameScenePath, true);
            EditorBuildSettings.scenes = scenes;
        }

        private sealed class SettingsPanelReferences
        {
            public GameObject GameObjectSettingsPanel { get; set; }
            public Button ButtonCloseSettings { get; set; }
            public Toggle ToggleVolumeMute { get; set; }
            public Slider SliderVolume { get; set; }
            public Text TextVolumeMute { get; set; }
            public Text TextVolumeValue { get; set; }
        }

        private sealed class BgmBuildReferences
        {
            public MainMenuBgmController Controller { get; set; }
            public AudioSource AudioSourceBgm { get; set; }
            public AudioClip[] AudioClipsBgm { get; set; }
        }
    }
}
