using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MainHub.Workspace;
using MainHub.Shared;
using MainHub.TeamDocs;
using MainHub.UI;
using MainHub.Audio;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace MainHub.CharacterSelect.Editor
{
    public static class MainHub_SceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/CharacterScenes/SampleScene.unity";
        private const string MainLobbySceneName = "Main_Lobby";
        private const string PlayableGameSceneName = "Cinderkeep_Game";
        private const string SharedWorkspaceSceneName = "Cinderkeep_Workspace";
        private const string MainBuildWorkspaceSceneName = "MainWorkspaceRoom_ForBuild";
        private const string PersonalWorkspaceSceneName = "PersonalWorkspaceRoom";
        private const string SceneFolder = "Assets/Scenes";
        private const string MainGameSceneFolder = "Assets/Scenes/MainGame";
        private const string MainWorkspaceSceneFolder = "Assets/Scenes/MainWorkspace";
        private const string CharacterSceneFolder = "Assets/Scenes/CharacterScenes";
        private const string LobbyBackgroundSpritePath = "Assets/Image/MainHubImage/MainLobby_Background.png";
        private const string KoreanFallbackFontPath = "Assets/Fonts/ChosunCentennial.ttf";
        private const string ReadMeFolderPath = "Assets/ReadMe";
        private const string BgmClipPath = "Assets/Sounds/MainHubBGM/Cinderkeep_BGM_V1.mp3";
        private const string BgmClipPath2 = "Assets/Sounds/MainHubBGM/Cinderkeep_BGM_V1_2.mp3";
        private const float BgmVolume = 0.4f;

        private static readonly string[] CinderkeepRelatedAssetPaths =
        {
            "Assets/Scripts/Workspace/MainHub_WorkspaceActor.cs",
            "Assets/Scripts/Shared/MainHub_SceneCameraController.cs",
            "Assets/Scripts/MainHub/UI/MainHub_UIManager.cs",
            "Assets/Scripts/CharacterSelect/MainHub_SceneLoadButton.cs",
            "Assets/_TeamGuide/04_CINDERKEEP_SHARED_WORKSPACE_PLAN.txt"
        };

        private static readonly MainHub_CharacterEntry[] DefaultCharacters =
        {
            MainHub_CharacterEntry.Create("tactician", "Tactician", "Tactician", "Daniel", "Plans team priorities and keeps the shared survival loop readable."),
            MainHub_CharacterEntry.Create("summoner", "Summoner", "Summoner", "JiJaewook", "Builds reusable systems and brings tools into the workspace."),
            MainHub_CharacterEntry.Create("healer", "Healer", "Healer", "KangHeewon", "Stabilizes the project with QA, balance checks, and recovery flow."),
            MainHub_CharacterEntry.Create("blacksmith", "Blacksmith", "Blacksmith", "ChoiJaeho", "Shapes resources, crafting, upgrades, and base structures."),
            MainHub_CharacterEntry.Create("assassin", "Assassin", "Assassin", "KimDonghyuk", "Handles fast interactions, combat risks, and sharp implementation tasks."),
            MainHub_CharacterEntry.Create("archer", "Archer", "Archer", "KwonSeonghyeok", "Builds ranged combat, wave pressure, and readable enemy feedback."),
            MainHub_CharacterEntry.Create("mage", "Mage", "Mage", "JeongDongwon", "Works on game rules, effects, data, and presentation of systems."),
            MainHub_CharacterEntry.Create("spellblade", "Spellblade", "Spellblade", "KimMinseok", "Connects combat and ability systems with responsive game feel."),
            MainHub_CharacterEntry.Create("tanker", "Tanker", "Tanker", "KimSeonggwang", "Owns player durability, HUD pressure, and base-defense readability."),
            MainHub_CharacterEntry.Create("bard", "Bard", "Bard", "CheonWooyoung", "Reads the party flow and supports the team with rhythm.")
        };

        private static readonly CharacterSceneInfo[] CharacterScenes =
        {
            new CharacterSceneInfo("archer", "Archer", "KwonSeonghyeok"),
            new CharacterSceneInfo("assassin", "Assassin", "KimDonghyuk"),
            new CharacterSceneInfo("bard", "Bard", "CheonWooyoung"),
            new CharacterSceneInfo("blacksmith", "Blacksmith", "ChoiJaeho"),
            new CharacterSceneInfo("healer", "Healer", "KangHeewon"),
            new CharacterSceneInfo("mage", "Mage", "JeongDongwon"),
            new CharacterSceneInfo("spellblade", "Spellblade", "KimMinseok"),
            new CharacterSceneInfo("summoner", "Summoner", "JiJaewook"),
            new CharacterSceneInfo("tactician", "Tactician", "Daniel"),
            new CharacterSceneInfo("tanker", "Tanker", "KimSeonggwang")
        };

        private static readonly Color[] AccentColors =
        {
            new Color(0.26f, 0.42f, 0.92f),
            new Color(0.46f, 0.28f, 0.74f),
            new Color(0.17f, 0.64f, 0.54f),
            new Color(0.7f, 0.38f, 0.18f),
            new Color(0.74f, 0.18f, 0.32f),
            new Color(0.24f, 0.56f, 0.26f),
            new Color(0.22f, 0.34f, 0.82f),
            new Color(0.58f, 0.26f, 0.8f),
            new Color(0.58f, 0.6f, 0.66f),
            new Color(0.88f, 0.62f, 0.18f)
        };

        [MenuItem("MainHub/Character Select/Rebuild Sample Scene")]
        public static void RebuildSampleScene()
        {
            EnsureCharacterSceneFolder();
            Scene scene = System.IO.File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            DestroyRootObject("Main Camera");
            DestroyRootObject("Character Select Canvas");
            DestroyRootObject("EventSystem");

            MainHub_SceneCameraController cameraController = CreateSceneCamera(Color.black, 5f, new Vector3(0f, 0f, -10f), Quaternion.identity);
            CreateEventSystem();
            MainHub_BgmController bgmController = CreateMainHubBgmController();
            CreateBgmToggleCanvas(bgmController);
            CreateCharacterSelectCanvas(cameraController);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("MainHub/Character Select/Rebuild Main Menu And Character Scenes")]
        public static void RebuildMainMenuAndCharacterScenes()
        {
            RebuildMainLobbyScene();
            RebuildSampleScene();
            RebuildCharacterScenes();
            RebuildWorkspaceScenes();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("MainHub/Character Select/Rebuild Main Lobby Only")]
        public static void RebuildMainLobbyAndBuildSettingsOnly()
        {
            EnsureMainGameSceneFolder();
            EnsureMainWorkspaceSceneFolder();
            EnsureCharacterSceneFolder();
            RebuildMainLobbyScene();
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
        }

        public static void RebuildMainLobbyScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject mainLobbyGroup = new GameObject("MainLobbyGroup");
            Transform mainLobbyTransform = mainLobbyGroup.transform;

            MainHub_SceneCameraController cameraController = CreateMainLobbyCamera(mainLobbyTransform);
            CreateEventSystem(mainLobbyTransform);
            MainHub_BgmController bgmController = CreateMainHubBgmController(mainLobbyTransform);
            CreateMainLobbyCanvas(cameraController, bgmController, mainLobbyTransform);
            CreateMainLobbyCharacterRoster(mainLobbyTransform);
            CreateMainLobbyPlayerEntry();
            EditorSceneManager.SaveScene(scene, GetScenePath(MainLobbySceneName));
        }

        public static void RebuildCharacterScenes()
        {
            EnsureCharacterSceneFolder();
            foreach (CharacterSceneInfo sceneInfo in CharacterScenes)
            {
                CreateCharacterScene(sceneInfo);
            }
        }

        public static void RebuildWorkspaceScenes()
        {
            EnsureMainGameSceneFolder();
            EnsureMainWorkspaceSceneFolder();
            EnsureCharacterSceneFolder();
            CreateWorkspaceScene(SharedWorkspaceSceneName, "Cinderkeep Team Workspace", "WorkspaceRoot_Shared");
            CreateWorkspaceScene(MainBuildWorkspaceSceneName, "Game Work Review Room", "WorkspaceRoot_MainBuild");
            CreateWorkspaceScene(PersonalWorkspaceSceneName, "Personal Workspace Room", "WorkspaceRoot_Personal");
            Debug.Log("MainHub_SceneBuilder: playable game scene rebuild skipped because Cinderkeep gameplay scripts are disabled.");
        }

        public static void ValidateSampleScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            MainHub_CharacterSelectCanvas canvas = GetFirstComponentInOpenScene<MainHub_CharacterSelectCanvas>(true);
            if (canvas == null)
            {
                throw new System.InvalidOperationException("Character Select Canvas is missing from SampleScene.");
            }

            ValidateCameraAndUIManager(SceneManager.GetActiveScene().name);

            MainHub_CharacterSlot[] slots = canvas.GetComponentsInChildren<MainHub_CharacterSlot>(true);
            if (slots.Length != DefaultCharacters.Length)
            {
                throw new System.InvalidOperationException($"Expected {DefaultCharacters.Length} character slots, but found {slots.Length}.");
            }

            foreach (MainHub_CharacterSlot slot in slots)
            {
                if (slot.Entry == null || !slot.Entry.HasId)
                {
                    throw new System.InvalidOperationException($"{slot.name} has no character entry.");
                }
            }

            Debug.Log($"MainHub_SceneBuilder: {slots.Length} character slots validated in SampleScene.");
        }

        public static void ValidateCharacterScenes()
        {
            foreach (CharacterSceneInfo sceneInfo in CharacterScenes)
            {
                string path = GetScenePath(sceneInfo.SceneName);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                MainHub_SceneProfile profile = GetFirstComponentInOpenScene<MainHub_SceneProfile>(true);

                if (profile == null)
                {
                    throw new System.InvalidOperationException($"{scene.name} has no MainHub_SceneProfile.");
                }

                if (profile.CharacterName != sceneInfo.SceneName)
                {
                    throw new System.InvalidOperationException($"{scene.name} profile name is {profile.CharacterName}, expected {sceneInfo.SceneName}.");
                }

                if (profile.OwnerEnglishName != sceneInfo.OwnerEnglishName)
                {
                    throw new System.InvalidOperationException($"{scene.name} owner name is {profile.OwnerEnglishName}, expected {sceneInfo.OwnerEnglishName}.");
                }

                ValidateCameraAndUIManager(scene.name);

                if (GetTransformByNameIncludingInactive("Discard_Work_And_Reset_Button") == null)
                {
                    throw new System.InvalidOperationException($"{scene.name} has no personal workspace reset button.");
                }
            }

            Debug.Log($"MainHub_SceneBuilder: {CharacterScenes.Length} character scenes validated.");
        }

        public static void ValidateMainLobbyAndWorkspaceScenes()
        {
            Scene lobbyScene = EditorSceneManager.OpenScene(GetScenePath(MainLobbySceneName), OpenSceneMode.Single);
            ValidateCameraAndUIManager(lobbyScene.name);
            MainHub_SceneLoadButton[] lobbyButtons = GetComponentsInOpenScene<MainHub_SceneLoadButton>(true);
            int expectedLoadButtonCount = (CharacterScenes.Length * 2) + 5;
            if (lobbyButtons.Length != expectedLoadButtonCount)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} expected {expectedLoadButtonCount} load buttons, but found {lobbyButtons.Length}.");
            }

            if (GetRootGameObjectByName(lobbyScene, "Player") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no root Player entry.");
            }

            if (GetTransformByNameIncludingInactive("!! Play Our Game (V1) Button") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no V1 play button.");
            }

            if (GetTransformByNameIncludingInactive("Go_ReadMe_Button") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no ReadMe folder button.");
            }

            if (GetTransformByNameIncludingInactive("Character_Workplace_Quick_Access") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no character workplace quick access panel.");
            }

            if (GetTransformByNameIncludingInactive("Finish Work Button") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no visible finish work button.");
            }

            if (GetTransformByNameIncludingInactive("Button_BgmToggle") == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no BGM toggle button.");
            }

            if (GetFirstComponentInOpenScene<MainHub_BgmController>(true) == null)
            {
                throw new System.InvalidOperationException($"{lobbyScene.name} has no MainHub_BgmController.");
            }

            ValidateWorkspaceScene(SharedWorkspaceSceneName, "WorkspaceRoot_Shared");
            ValidateWorkspaceScene(MainBuildWorkspaceSceneName, "WorkspaceRoot_MainBuild");
            ValidateWorkspaceScene(PersonalWorkspaceSceneName, "WorkspaceRoot_Personal");
            Debug.Log("MainHub_SceneBuilder: playable game scene validation skipped because Cinderkeep gameplay scripts are disabled.");
            Debug.Log("MainHub_SceneBuilder: main lobby and workspace scenes validated.");
        }

        public static void RebuildAndValidateRequestedSetup()
        {
            RebuildMainMenuAndCharacterScenes();
            ValidateRequestedSetup();
        }

        public static void ValidateRequestedSetup()
        {
            ValidateBuildStartScene();
            ValidateSampleScene();
            ValidateCharacterDetailPanel();
            ValidateCharacterScenes();
            ValidateMainLobbyAndWorkspaceScenes();
            ValidateSharedCinderkeepWorkspacePreview();
            Debug.Log("MainHub_SceneBuilder: requested setup validated.");
        }

        private static void DestroyRootObject(string name)
        {
            GameObject target = GetRootGameObjectByName(SceneManager.GetActiveScene(), name);
            if (target != null)
            {
                Object.DestroyImmediate(target);
            }
        }

        private static void CreateEventSystem(Transform parent = null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            SetParent(eventSystemObject, parent);
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static MainHub_SceneCameraController CreateMainLobbyCamera(Transform parent = null)
        {
            return CreateSceneCamera(Color.black, 5f, new Vector3(0f, 0f, -10f), Quaternion.identity, parent);
        }

        private static void CreateMainLobbyCharacterRoster(Transform parent = null)
        {
            GameObject rosterRoot = new GameObject("CharacterSceneObjects");
            SetParent(rosterRoot, parent);

            for (int i = 0; i < CharacterScenes.Length; i++)
            {
                CharacterSceneInfo sceneInfo = CharacterScenes[i];
                GameObject characterObject = new GameObject(sceneInfo.SceneName);
                characterObject.transform.SetParent(rosterRoot.transform, false);
                characterObject.transform.localPosition = new Vector3(0f, 0f, i * 0.1f);

                MainHub_SceneLoadButton loader = characterObject.AddComponent<MainHub_SceneLoadButton>();
                loader.SetSceneName(sceneInfo.SceneName);
                EditorUtility.SetDirty(loader);
            }
        }

        private static void CreateCharacterSelectCanvas(MainHub_SceneCameraController cameraController)
        {
            GameObject canvasObject = new GameObject(
                "Character Select Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainHub_CharacterSelectCanvas),
                typeof(MainHub_UIManager));

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            ConfigureUIManager(canvasObject, cameraController);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Black Background", root, new Color(0.01f, 0.01f, 0.014f, 1f));
            Stretch(background.rectTransform);

            CreateHeader(root);
            CreateBackToMenuButton(root);
            CreateCharacterSlots(root);
            MainHub_CharacterDetailPanel detailPanel = CreateDetailPanel(root);

            MainHub_CharacterSelectCanvas controller = canvasObject.GetComponent<MainHub_CharacterSelectCanvas>();
            controller.SetDetailPanel(detailPanel);
            EditorUtility.SetDirty(controller);
        }

        private static void CreateMainLobbyCanvas(MainHub_SceneCameraController cameraController, MainHub_BgmController bgmController, Transform parent = null)
        {
            EnsureLobbyArtSprites();

            GameObject canvasObject = new GameObject(
                "MainLobbyCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainHub_UIManager));
            SetParent(canvasObject, parent);

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            ConfigureUIManager(canvasObject, cameraController);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Main_Lobby_Background_Image", root, Color.white);
            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LobbyBackgroundSpritePath);
            background.type = Image.Type.Simple;
            background.preserveAspect = false;
            Stretch(background.rectTransform);

            Image darkWash = CreateImage("Main_Lobby_Dark_Wash", root, new Color(0f, 0f, 0f, 0.42f));
            Stretch(darkWash.rectTransform);

            RectTransform header = CreateRect("Lobby Header", root);
            header.anchorMin = new Vector2(0.06f, 0.75f);
            header.anchorMax = new Vector2(0.45f, 0.92f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            Text title = CreateText("Lobby Title", header, "OO_DONG Cinderkeep Hub", 50, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.rectTransform.anchorMin = new Vector2(0f, 0.38f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            Text subtitle = CreateText("Lobby Subtitle", header, "팀 허브: 작업 공간, 캐릭터, 게임 실행 관리", 20, new Color(0.9f, 0.78f, 0.45f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            subtitle.rectTransform.anchorMin = new Vector2(0f, 0f);
            subtitle.rectTransform.anchorMax = new Vector2(1f, 0.28f);
            subtitle.rectTransform.offsetMin = Vector2.zero;
            subtitle.rectTransform.offsetMax = Vector2.zero;

            RectTransform menu = CreateRect("Main Menu Buttons", root);
            menu.anchorMin = new Vector2(0.07f, 0.16f);
            menu.anchorMax = new Vector2(0.43f, 0.68f);
            menu.offsetMin = Vector2.zero;
            menu.offsetMax = Vector2.zero;

            Image menuBack = CreateImage("Main Menu Buttons Background", menu, new Color(0.025f, 0.03f, 0.04f, 0.88f));
            Stretch(menuBack.rectTransform);

            Outline menuOutline = menu.gameObject.AddComponent<Outline>();
            menuOutline.effectColor = new Color(0.88f, 0.62f, 0.18f, 0.5f);
            menuOutline.effectDistance = new Vector2(3f, -3f);

            VerticalLayoutGroup layout = menu.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateLobbyLoadButton(menu, "!! Play Our Game (V1)", PlayableGameSceneName, new Color(0.18f, 0.5f, 0.32f, 1f), Color.white);
            CreateLobbyLoadButton(menu, "Select My Character", "SampleScene", new Color(0.92f, 0.7f, 0.18f, 1f), new Color(0.055f, 0.06f, 0.07f, 1f));
            CreateOpenReadMeFolderButton(menu);
            CreateLobbyLoadButton(menu, "공용 작업 공간", PlayableGameSceneName, new Color(0.14f, 0.2f, 0.28f, 1f), Color.white);
            CreateRememberedPersonalWorkspaceButton(menu);
            CreateLobbyLoadButton(menu, "게임 작업 공간", MainBuildWorkspaceSceneName, new Color(0.5f, 0.08f, 0.08f, 1f), Color.white);

            CreateBgmToggleCanvas(bgmController, parent);

            RectTransform finishButton = CreateButton(
                "Finish Work Button",
                menu,
                "작업 종료",
                24,
                new Color(0.24f, 0.25f, 0.29f, 1f),
                new Color(0.86f, 0.88f, 0.94f, 1f));
            AddButtonFrame(finishButton, new Color(1f, 1f, 1f, 0.15f));

            RectTransform accent = CreateRect("Lobby Accent Line", root);
            accent.anchorMin = new Vector2(0.455f, 0.14f);
            accent.anchorMax = new Vector2(0.466f, 0.86f);
            accent.offsetMin = Vector2.zero;
            accent.offsetMax = Vector2.zero;
            Image accentImage = accent.gameObject.AddComponent<Image>();
            accentImage.color = new Color(0.92f, 0.7f, 0.18f, 0.75f);

            CreateCharacterWorkplaceButtons(root);
        }

        private static void CreateMainLobbyPlayerEntry()
        {
            GameObject playerObject = new GameObject(
                "Player",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainHub_SceneLoadButton));

            RectTransform root = playerObject.GetComponent<RectTransform>();
            Stretch(root);

            Canvas canvas = playerObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = playerObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            MainHub_SceneLoadButton loader = playerObject.GetComponent<MainHub_SceneLoadButton>();
            loader.SetSceneName(PlayableGameSceneName);

            RectTransform buttonRoot = CreateButton(
                "Player_Click_Area",
                root,
                "Player\n게임 작업 시작",
                24,
                new Color(0.07f, 0.34f, 0.29f, 0.96f),
                Color.white);

            buttonRoot.anchorMin = new Vector2(0.07f, 0.69f);
            buttonRoot.anchorMax = new Vector2(0.43f, 0.745f);
            buttonRoot.offsetMin = Vector2.zero;
            buttonRoot.offsetMax = Vector2.zero;
            AddButtonFrame(buttonRoot, new Color(0.7f, 1f, 0.88f, 0.85f));

            Shadow shadow = buttonRoot.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            shadow.effectDistance = new Vector2(4f, -4f);

            Text label = buttonRoot.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.gameObject.name = "Text_PlayerEntry";
            }

            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, loader.LoadScene);
            EditorUtility.SetDirty(loader);
        }

        private static MainHub_BgmController CreateMainHubBgmController(Transform parent = null)
        {
            GameObject bgmObject = new GameObject(
                "MainHub_BGM_Controller",
                typeof(AudioSource),
                typeof(MainHub_BgmController));
            SetParent(bgmObject, parent);

            AudioSource audioSource = bgmObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = BgmVolume;

            MainHub_BgmController bgmController = bgmObject.GetComponent<MainHub_BgmController>();
            bgmController.SetReferences(audioSource, LoadMainHubBgmPlaylist(), BgmVolume);
            EditorUtility.SetDirty(audioSource);
            EditorUtility.SetDirty(bgmController);
            return bgmController;
        }

        private static AudioClip[] LoadMainHubBgmPlaylist()
        {
            List<AudioClip> clips = new List<AudioClip>();
            AddBgmClip(clips, BgmClipPath);
            AddBgmClip(clips, BgmClipPath2);
            return clips.ToArray();
        }

        private static void AddBgmClip(List<AudioClip> clips, string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        private static void CreateBgmToggleCanvas(MainHub_BgmController bgmController, Transform parent = null)
        {
            GameObject canvasObject = new GameObject(
                "MainHub_BgmToggleCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            SetParent(canvasObject, parent);

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CreateBgmToggleButton(root, bgmController);
        }

        private static void CreateBgmToggleButton(RectTransform parent, MainHub_BgmController bgmController)
        {
            RectTransform buttonRoot = CreateButton(
                "Button_BgmToggle",
                parent,
                "BGM ON",
                26,
                new Color(0.02f, 0.08f, 0.14f, 0.96f),
                Color.white);

            buttonRoot.anchorMin = new Vector2(1f, 1f);
            buttonRoot.anchorMax = new Vector2(1f, 1f);
            buttonRoot.pivot = new Vector2(1f, 1f);
            buttonRoot.anchoredPosition = new Vector2(-34f, -28f);
            buttonRoot.sizeDelta = new Vector2(172f, 52f);
            AddButtonFrame(buttonRoot, new Color(0.78f, 0.9f, 1f, 0.78f));

            Shadow shadow = buttonRoot.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
            shadow.effectDistance = new Vector2(4f, -4f);

            Button button = buttonRoot.GetComponent<Button>();
            Text label = buttonRoot.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.gameObject.name = "Text_BgmToggle";
                label.text = "\u266a BGM ON";
            }

            MainHub_BgmToggleButton toggleButton = buttonRoot.gameObject.AddComponent<MainHub_BgmToggleButton>();
            toggleButton.SetReferences(button, label, bgmController);
            EditorUtility.SetDirty(toggleButton);
        }

        private static void EnsureLobbyArtSprites()
        {
            ConfigureTextureAsSprite(LobbyBackgroundSpritePath);
            AssetDatabase.ImportAsset(KoreanFallbackFontPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureTextureAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
        }

        private static void CreateCharacterWorkplaceButtons(RectTransform root)
        {
            RectTransform panel = CreateRect("Character_Workplace_Quick_Access", root);
            panel.anchorMin = new Vector2(0.52f, 0.12f);
            panel.anchorMax = new Vector2(0.94f, 0.86f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            Image panelBack = CreateImage("Character_Workplace_Background", panel, new Color(0.018f, 0.022f, 0.032f, 0.9f));
            Stretch(panelBack.rectTransform);

            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.84f, 0.58f, 0.16f, 0.5f);
            outline.effectDistance = new Vector2(3f, -3f);

            Text title = CreateText("Character_Workplace_Title", panel, "개인 작업공간 바로가기", 28, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.rectTransform.anchorMin = new Vector2(0.055f, 0.91f);
            title.rectTransform.anchorMax = new Vector2(0.95f, 0.98f);
            title.rectTransform.offsetMin = Vector2.zero;
            title.rectTransform.offsetMax = Vector2.zero;

            RectTransform gridRoot = CreateRect("Character_Workplace_Button_Grid", panel);
            gridRoot.anchorMin = new Vector2(0.055f, 0.04f);
            gridRoot.anchorMax = new Vector2(0.945f, 0.88f);
            gridRoot.offsetMin = Vector2.zero;
            gridRoot.offsetMax = Vector2.zero;

            GridLayoutGroup grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(320f, 58f);
            grid.spacing = new Vector2(14f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;

            foreach (CharacterSceneInfo sceneInfo in CharacterScenes)
            {
                RectTransform buttonRoot = CreateButton(
                    $"{sceneInfo.HierarchyName}_Button",
                    gridRoot,
                    sceneInfo.HierarchyName,
                    17,
                    new Color(0.09f, 0.105f, 0.135f, 1f),
                    Color.white);
                AddButtonFrame(buttonRoot, new Color(0.86f, 0.62f, 0.18f, 0.5f));

                MainHub_SceneLoadButton loader = buttonRoot.gameObject.AddComponent<MainHub_SceneLoadButton>();
                loader.SetSceneName(sceneInfo.SceneName);
                UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, loader.LoadScene);
                EditorUtility.SetDirty(loader);
            }
        }

        private static void CreateHeader(RectTransform root)
        {
            RectTransform header = CreateRect("Header", root);
            header.anchorMin = new Vector2(0.04f, 0.89f);
            header.anchorMax = new Vector2(0.96f, 0.965f);
            header.offsetMin = Vector2.zero;
            header.offsetMax = Vector2.zero;

            Text title = CreateText("Title", header, "Select My Character", 46, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(title.rectTransform);
        }

        private static void CreateBackToMenuButton(RectTransform root)
        {
            RectTransform backButton = CreateButton(
                "Back To Menu Button",
                root,
                "Back To Menu",
                22,
                new Color(0.92f, 0.7f, 0.18f, 1f),
                new Color(0.055f, 0.06f, 0.07f, 1f));
            backButton.anchorMin = new Vector2(0.79f, 0.9f);
            backButton.anchorMax = new Vector2(0.96f, 0.96f);
            backButton.offsetMin = Vector2.zero;
            backButton.offsetMax = Vector2.zero;

            MainHub_SceneLoadButton backLoader = backButton.gameObject.AddComponent<MainHub_SceneLoadButton>();
            backLoader.SetSceneName(MainLobbySceneName);
            UnityEventTools.AddPersistentListener(backButton.GetComponent<Button>().onClick, backLoader.LoadScene);
            EditorUtility.SetDirty(backLoader);
        }

        private static MainHub_MenuCharacterPanel CreateMainMenu(RectTransform root)
        {
            GameObject controllerObject = new GameObject(
                "Main Menu Controller",
                typeof(RectTransform),
                typeof(MainHub_MenuCharacterPanel));
            controllerObject.transform.SetParent(root, false);
            RectTransform controllerRect = controllerObject.GetComponent<RectTransform>();
            controllerRect.anchorMin = Vector2.zero;
            controllerRect.anchorMax = Vector2.zero;
            controllerRect.sizeDelta = Vector2.zero;

            RectTransform panel = CreateRect("Personal Work Rooms Panel", root);
            panel.anchorMin = new Vector2(0.47f, 0.15f);
            panel.anchorMax = new Vector2(0.72f, 0.72f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            Image panelBack = CreateImage("Panel Background", panel, new Color(0.02f, 0.025f, 0.035f, 0.94f));
            Stretch(panelBack.rectTransform);

            Text panelTitle = CreateText("Panel Title", panel, "Personal Work Rooms", 26, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            panelTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.88f);
            panelTitle.rectTransform.anchorMax = new Vector2(0.94f, 0.97f);
            panelTitle.rectTransform.offsetMin = Vector2.zero;
            panelTitle.rectTransform.offsetMax = Vector2.zero;

            RectTransform list = CreateRect("Personal Work Room List", panel);
            list.anchorMin = new Vector2(0.05f, 0.05f);
            list.anchorMax = new Vector2(0.95f, 0.86f);
            list.offsetMin = Vector2.zero;
            list.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            foreach (CharacterSceneInfo sceneInfo in CharacterScenes)
            {
                RectTransform sceneButton = CreateButton(
                    sceneInfo.SceneName,
                    list,
                    sceneInfo.SceneName,
                    21,
                    new Color(0.12f, 0.14f, 0.18f, 1f),
                    Color.white);
                LayoutElement layoutElement = sceneButton.gameObject.AddComponent<LayoutElement>();
                layoutElement.minHeight = 46f;
                layoutElement.preferredHeight = 52f;

                MainHub_SceneLoadButton loader = sceneButton.gameObject.AddComponent<MainHub_SceneLoadButton>();
                loader.SetSceneName(sceneInfo.SceneName);
                UnityEventTools.AddPersistentListener(sceneButton.GetComponent<Button>().onClick, loader.LoadScene);
                EditorUtility.SetDirty(loader);
            }

            MainHub_MenuCharacterPanel menu = controllerObject.GetComponent<MainHub_MenuCharacterPanel>();
            menu.SetCharacterScenePanel(panel.gameObject);
            panel.gameObject.SetActive(false);
            EditorUtility.SetDirty(menu);
            return menu;
        }

        private static void CreateCharacterSlots(RectTransform root)
        {
            RectTransform slotsRoot = CreateRect("Character Slots", root);
            slotsRoot.anchorMin = new Vector2(0.03f, 0.36f);
            slotsRoot.anchorMax = new Vector2(0.97f, 0.86f);
            slotsRoot.offsetMin = Vector2.zero;
            slotsRoot.offsetMax = Vector2.zero;

            HorizontalLayoutGroup layout = slotsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            for (int i = 0; i < DefaultCharacters.Length; i++)
            {
                CreateCharacterSlot(slotsRoot, DefaultCharacters[i], i);
            }
        }

        private static void CreateCharacterSlot(RectTransform parent, MainHub_CharacterEntry entry, int index)
        {
            GameObject card = new GameObject(
                $"{index + 1:00}_{entry.EnglishRole}_{entry.OwnerName}_Character",
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline),
                typeof(LayoutElement),
                typeof(MainHub_CharacterSlot));

            card.transform.SetParent(parent, false);

            RectTransform rect = card.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(170f, 500f);

            LayoutElement layout = card.GetComponent<LayoutElement>();
            layout.minWidth = 112f;
            layout.preferredWidth = 170f;
            layout.flexibleWidth = 1f;
            layout.minHeight = 420f;

            Image frame = card.GetComponent<Image>();
            frame.color = new Color(0.08f, 0.085f, 0.1f, 1f);

            Outline outline = card.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.14f);
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform portraitRoot = CreateRect("Portrait", rect);
            portraitRoot.anchorMin = new Vector2(0.045f, 0.19f);
            portraitRoot.anchorMax = new Vector2(0.955f, 0.965f);
            portraitRoot.offsetMin = Vector2.zero;
            portraitRoot.offsetMax = Vector2.zero;

            Image portrait = CreateImage("Portrait Image", portraitRoot, AccentColors[index]);
            portrait.preserveAspect = true;
            Stretch(portrait.rectTransform);

            Image hoverWash = CreateImage("Hover Highlight", portraitRoot, new Color(1f, 1f, 1f, 0f));
            Stretch(hoverWash.rectTransform);

            Text placeholder = CreateText("Portrait Placeholder", portraitRoot, entry.EnglishRole, 18, new Color(1f, 1f, 1f, 0.78f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(placeholder.rectTransform, 8f, 8f);

            RectTransform labelRoot = CreateRect("Label", rect);
            labelRoot.anchorMin = new Vector2(0.045f, 0.035f);
            labelRoot.anchorMax = new Vector2(0.955f, 0.17f);
            labelRoot.offsetMin = Vector2.zero;
            labelRoot.offsetMax = Vector2.zero;

            Image labelBack = CreateImage("Label Back", labelRoot, new Color(0f, 0f, 0f, 0.72f));
            Stretch(labelBack.rectTransform);

            Text classText = CreateText("Class Label", labelRoot, entry.EnglishRole, 21, Color.white, TextAnchor.UpperCenter, FontStyle.Bold);
            classText.rectTransform.anchorMin = new Vector2(0f, 0.46f);
            classText.rectTransform.anchorMax = Vector2.one;
            classText.rectTransform.offsetMin = Vector2.zero;
            classText.rectTransform.offsetMax = Vector2.zero;

            Text ownerText = CreateText("Owner Name", labelRoot, entry.OwnerName, 16, new Color(0.86f, 0.88f, 0.92f), TextAnchor.LowerCenter, FontStyle.Normal);
            ownerText.rectTransform.anchorMin = Vector2.zero;
            ownerText.rectTransform.anchorMax = new Vector2(1f, 0.52f);
            ownerText.rectTransform.offsetMin = Vector2.zero;
            ownerText.rectTransform.offsetMax = Vector2.zero;

            MainHub_CharacterSlot slot = card.GetComponent<MainHub_CharacterSlot>();
            slot.SetEntry(entry);
            slot.SetAccentColor(AccentColors[index]);
            slot.SetViewReferences(frame, outline, rect, portrait, placeholder, hoverWash, labelBack);
            EditorUtility.SetDirty(slot);
        }

        private static MainHub_CharacterDetailPanel CreateDetailPanel(RectTransform root)
        {
            GameObject panelObject = new GameObject(
                "White Detail Panel",
                typeof(RectTransform),
                typeof(MainHub_CharacterDetailPanel));

            panelObject.transform.SetParent(root, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0.18f, 0.055f);
            panel.anchorMax = new Vector2(0.82f, 0.31f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            Image panelImage = CreateImage("Panel Background", panel, new Color(0.965f, 0.968f, 0.955f, 1f));
            Stretch(panelImage.rectTransform);

            Text detailTitle = CreateText("Detail Title", panel, string.Empty, 30, new Color(0.055f, 0.06f, 0.07f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            detailTitle.rectTransform.anchorMin = new Vector2(0.04f, 0.7f);
            detailTitle.rectTransform.anchorMax = new Vector2(0.96f, 0.94f);
            detailTitle.rectTransform.offsetMin = Vector2.zero;
            detailTitle.rectTransform.offsetMax = Vector2.zero;

            Text detailRole = CreateText("Detail Role", panel, string.Empty, 20, new Color(0.27f, 0.29f, 0.33f), TextAnchor.MiddleLeft, FontStyle.Normal);
            detailRole.rectTransform.anchorMin = new Vector2(0.04f, 0.54f);
            detailRole.rectTransform.anchorMax = new Vector2(0.34f, 0.7f);
            detailRole.rectTransform.offsetMin = Vector2.zero;
            detailRole.rectTransform.offsetMax = Vector2.zero;

            Text nameLabel = CreateText("Name Label", panel, "Member:", 18, new Color(0.24f, 0.25f, 0.28f), TextAnchor.MiddleLeft, FontStyle.Bold);
            nameLabel.rectTransform.anchorMin = new Vector2(0.04f, 0.37f);
            nameLabel.rectTransform.anchorMax = new Vector2(0.13f, 0.51f);
            nameLabel.rectTransform.offsetMin = Vector2.zero;
            nameLabel.rectTransform.offsetMax = Vector2.zero;

            InputField ownerInput = CreateInput("Owner Name Input", panel, "Member", 20);
            RectTransform ownerRect = ownerInput.GetComponent<RectTransform>();
            ownerRect.anchorMin = new Vector2(0.14f, 0.35f);
            ownerRect.anchorMax = new Vector2(0.34f, 0.51f);
            ownerRect.offsetMin = Vector2.zero;
            ownerRect.offsetMax = Vector2.zero;

            Text descriptionLabel = CreateText("Description Label", panel, "Role Note:", 18, new Color(0.24f, 0.25f, 0.28f), TextAnchor.MiddleLeft, FontStyle.Bold);
            descriptionLabel.rectTransform.anchorMin = new Vector2(0.38f, 0.5f);
            descriptionLabel.rectTransform.anchorMax = new Vector2(0.96f, 0.66f);
            descriptionLabel.rectTransform.offsetMin = Vector2.zero;
            descriptionLabel.rectTransform.offsetMax = Vector2.zero;

            InputField introInput = CreateInput("Introduction Input", panel, "Role Note", 18);
            introInput.lineType = InputField.LineType.MultiLineNewline;
            introInput.textComponent.alignment = TextAnchor.UpperLeft;
            RectTransform introRect = introInput.GetComponent<RectTransform>();
            introRect.anchorMin = new Vector2(0.38f, 0.12f);
            introRect.anchorMax = new Vector2(0.96f, 0.5f);
            introRect.offsetMin = Vector2.zero;
            introRect.offsetMax = Vector2.zero;

            Text rememberedStatus = CreateText("Remembered Character Status", panel, "기억된 캐릭터: 없음", 17, new Color(0.18f, 0.22f, 0.24f), TextAnchor.MiddleLeft, FontStyle.Bold);
            rememberedStatus.rectTransform.anchorMin = new Vector2(0.04f, 0.305f);
            rememberedStatus.rectTransform.anchorMax = new Vector2(0.34f, 0.35f);
            rememberedStatus.rectTransform.offsetMin = Vector2.zero;
            rememberedStatus.rectTransform.offsetMax = Vector2.zero;

            RectTransform personalButton = CreateButton(
                "Open Personal Workspace Button",
                panel,
                "Open Personal Workspace",
                18,
                new Color(0.055f, 0.2f, 0.16f, 1f),
                Color.white);
            personalButton.anchorMin = new Vector2(0.04f, 0.12f);
            personalButton.anchorMax = new Vector2(0.34f, 0.3f);
            personalButton.offsetMin = Vector2.zero;
            personalButton.offsetMax = Vector2.zero;

            MainHub_SceneLoadButton personalLoader = personalButton.gameObject.AddComponent<MainHub_SceneLoadButton>();
            UnityEventTools.AddPersistentListener(personalButton.GetComponent<Button>().onClick, personalLoader.LoadScene);

            RectTransform rememberButton = CreateButton(
                "Remember My Character Button",
                panel,
                "내 캐릭터로 기억",
                16,
                new Color(0.18f, 0.34f, 0.52f, 1f),
                Color.white);
            rememberButton.anchorMin = new Vector2(0.04f, 0.025f);
            rememberButton.anchorMax = new Vector2(0.185f, 0.105f);
            rememberButton.offsetMin = Vector2.zero;
            rememberButton.offsetMax = Vector2.zero;

            RectTransform cancelButton = CreateButton(
                "Cancel Remembered Character Button",
                panel,
                "선택 취소",
                16,
                new Color(0.42f, 0.08f, 0.08f, 1f),
                Color.white);
            cancelButton.anchorMin = new Vector2(0.195f, 0.025f);
            cancelButton.anchorMax = new Vector2(0.34f, 0.105f);
            cancelButton.offsetMin = Vector2.zero;
            cancelButton.offsetMax = Vector2.zero;

            MainHub_CharacterDetailPanel detailPanel = panelObject.GetComponent<MainHub_CharacterDetailPanel>();
            detailPanel.SetViewReferences(
                detailTitle,
                detailRole,
                ownerInput,
                introInput,
                personalLoader,
                rememberButton.GetComponent<Button>(),
                cancelButton.GetComponent<Button>(),
                rememberedStatus);
            panelObject.SetActive(false);
            EditorUtility.SetDirty(personalLoader);
            EditorUtility.SetDirty(detailPanel);
            return detailPanel;
        }

        private static InputField CreateInput(string name, RectTransform parent, string placeholderText, int fontSize)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            root.transform.SetParent(parent, false);

            Image background = root.GetComponent<Image>();
            background.color = Color.white;

            InputField input = root.GetComponent<InputField>();
            input.targetGraphic = background;

            RectTransform textRoot = CreateRect("Text", root.GetComponent<RectTransform>());
            Stretch(textRoot, 12f, 8f);
            Text text = CreateText("Input Text", textRoot, string.Empty, fontSize, new Color(0.055f, 0.06f, 0.07f, 1f), TextAnchor.MiddleLeft, FontStyle.Normal);
            Stretch(text.rectTransform);
            input.textComponent = text;

            RectTransform placeholderRoot = CreateRect("Placeholder", root.GetComponent<RectTransform>());
            Stretch(placeholderRoot, 12f, 8f);
            Text placeholder = CreateText("Placeholder Text", placeholderRoot, placeholderText, fontSize, new Color(0.45f, 0.46f, 0.5f, 0.8f), TextAnchor.MiddleLeft, FontStyle.Italic);
            Stretch(placeholder.rectTransform);
            input.placeholder = placeholder;

            return input;
        }

        private static RectTransform CreateButton(
            string name,
            RectTransform parent,
            string label,
            int fontSize,
            Color backgroundColor,
            Color textColor)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            Text text = CreateText("Text", buttonObject.GetComponent<RectTransform>(), label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform, 8f, 4f);

            return buttonObject.GetComponent<RectTransform>();
        }

        private static void CreateLobbyLoadButton(RectTransform parent, string label, string sceneName)
        {
            CreateLobbyLoadButton(
                parent,
                label,
                sceneName,
                new Color(0.92f, 0.7f, 0.18f, 1f),
                new Color(0.055f, 0.06f, 0.07f, 1f));
        }

        private static void CreateLobbyLoadButton(
            RectTransform parent,
            string label,
            string sceneName,
            Color backgroundColor,
            Color textColor,
            string[] editorAssetPathsToOpen = null)
        {
            RectTransform buttonRoot = CreateButton(
                $"{label} Button",
                parent,
                label,
                28,
                backgroundColor,
                textColor);

            LayoutElement layoutElement = buttonRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 54f;
            layoutElement.preferredHeight = 60f;

            MainHub_SceneLoadButton loader = buttonRoot.gameObject.AddComponent<MainHub_SceneLoadButton>();
            loader.SetSceneName(sceneName);
            loader.SetEditorAssetPathsToOpen(editorAssetPathsToOpen);
            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, loader.LoadScene);
            EditorUtility.SetDirty(loader);
        }

        private static void CreateRememberedPersonalWorkspaceButton(RectTransform parent)
        {
            RectTransform buttonRoot = CreateButton(
                "개인 작업 공간 Button",
                parent,
                "개인 작업 공간",
                28,
                new Color(0.3f, 0.34f, 0.76f, 1f),
                Color.white);

            LayoutElement layoutElement = buttonRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 54f;
            layoutElement.preferredHeight = 60f;

            MainHub_RememberedCharacterWorkspaceButton rememberedButton = buttonRoot.gameObject.AddComponent<MainHub_RememberedCharacterWorkspaceButton>();
            rememberedButton.SetFallbackSceneName(PersonalWorkspaceSceneName);
            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, rememberedButton.LoadRememberedWorkspace);
            EditorUtility.SetDirty(rememberedButton);
        }

        private static void CreateOpenReadMeFolderButton(RectTransform parent)
        {
            RectTransform buttonRoot = CreateButton(
                "Go_ReadMe_Button",
                parent,
                "Go -> ReadMe",
                24,
                new Color(0.09f, 0.12f, 0.16f, 1f),
                Color.white);

            AddButtonFrame(buttonRoot, new Color(0.9f, 0.64f, 0.2f, 0.45f));

            LayoutElement layoutElement = buttonRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 54f;
            layoutElement.preferredHeight = 60f;

            MainHub_ReadMeFolderOpenButton opener = buttonRoot.gameObject.AddComponent<MainHub_ReadMeFolderOpenButton>();
            opener.SetFolderRelativePath(ReadMeFolderPath);
            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, opener.OpenFolder);
            EditorUtility.SetDirty(opener);
        }

        private static void CreateLobbyPanelToggleButton(RectTransform parent, string label, MainHub_MenuCharacterPanel menuPanel)
        {
            RectTransform buttonRoot = CreateButton(
                $"{label} Button",
                parent,
                label,
                28,
                new Color(0.3f, 0.34f, 0.76f, 1f),
                Color.white);

            LayoutElement layoutElement = buttonRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 68f;
            layoutElement.preferredHeight = 78f;

            if (menuPanel != null)
            {
                UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, menuPanel.ToggleCharacterScenePanel);
            }
        }

        private static void CreateCharacterScene(CharacterSceneInfo sceneInfo)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();
            MainHub_BgmController bgmController = CreateMainHubBgmController();
            CreateBgmToggleCanvas(bgmController);
            MainHub_SceneCameraController cameraController = CreateSceneCamera(new Color(0.012f, 0.014f, 0.02f), 5f, new Vector3(0f, 0f, -10f), Quaternion.identity);

            GameObject workspaceObject = new GameObject(
                $"Workspace_{sceneInfo.HierarchyName}",
                typeof(MainHub_SceneProfile));
            MainHub_SceneProfile profile = workspaceObject.GetComponent<MainHub_SceneProfile>();
            profile.SetProfile(sceneInfo.SceneName, sceneInfo.RoleName, sceneInfo.OwnerEnglishName);
            EditorUtility.SetDirty(profile);
            CreatePersonalLobbyHierarchy(workspaceObject.transform, sceneInfo);

            GameObject canvasObject = new GameObject(
                $"Canvas_{sceneInfo.HierarchyName}",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainHub_UIManager));
            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            ConfigureUIManager(canvasObject, cameraController);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Black Background", root, new Color(0.012f, 0.014f, 0.02f, 1f));
            Stretch(background.rectTransform);

            Image lobbyPanel = CreateImage("Personal_Work_Lobby_Backplate", root, new Color(0.05f, 0.06f, 0.075f, 0.88f));
            lobbyPanel.rectTransform.anchorMin = new Vector2(0.035f, 0.055f);
            lobbyPanel.rectTransform.anchorMax = new Vector2(0.585f, 0.94f);
            lobbyPanel.rectTransform.offsetMin = Vector2.zero;
            lobbyPanel.rectTransform.offsetMax = Vector2.zero;
            AddButtonFrame(lobbyPanel.rectTransform, new Color(0.9f, 0.64f, 0.2f, 0.26f));

            Image accentLine = CreateImage("Personal_Work_Lobby_Accent_Line", root, new Color(0.88f, 0.62f, 0.18f, 0.92f));
            accentLine.rectTransform.anchorMin = new Vector2(0.595f, 0.12f);
            accentLine.rectTransform.anchorMax = new Vector2(0.603f, 0.9f);
            accentLine.rectTransform.offsetMin = Vector2.zero;
            accentLine.rectTransform.offsetMax = Vector2.zero;

            Image portrait = CreateImage("Character Portrait", root, Color.white);
            portrait.sprite = Resources.Load<Sprite>($"CharacterPortraits/{sceneInfo.ResourceId}");
            portrait.preserveAspect = true;
            portrait.rectTransform.anchorMin = new Vector2(0.62f, 0.04f);
            portrait.rectTransform.anchorMax = new Vector2(0.98f, 0.96f);
            portrait.rectTransform.offsetMin = Vector2.zero;
            portrait.rectTransform.offsetMax = Vector2.zero;

            RectTransform titleRoot = CreateRect("Title Area", root);
            titleRoot.anchorMin = new Vector2(0.06f, 0.74f);
            titleRoot.anchorMax = new Vector2(0.56f, 0.91f);
            titleRoot.offsetMin = Vector2.zero;
            titleRoot.offsetMax = Vector2.zero;

            Text title = CreateText("Character Name", titleRoot, sceneInfo.HierarchyName, 54, Color.white, TextAnchor.LowerLeft, FontStyle.Bold);
            Stretch(title.rectTransform);

            Text role = CreateText("Role Name", titleRoot, sceneInfo.RoleName, 28, new Color(0.9f, 0.72f, 0.28f, 1f), TextAnchor.UpperLeft, FontStyle.Normal);
            role.rectTransform.anchorMin = new Vector2(0f, 0f);
            role.rectTransform.anchorMax = new Vector2(1f, 0.42f);
            role.rectTransform.offsetMin = Vector2.zero;
            role.rectTransform.offsetMax = Vector2.zero;

            MainHub_WorkProgressProfile progressProfile = workspaceObject.AddComponent<MainHub_WorkProgressProfile>();
            RectTransform profilePanel = CreateDeveloperProgressPanel(root, sceneInfo, progressProfile);

            RectTransform workRoot = CreateRect($"WorkRoot_{sceneInfo.HierarchyName}", root);
            workRoot.anchorMin = new Vector2(0.06f, 0.33f);
            workRoot.anchorMax = new Vector2(0.56f, 0.52f);
            workRoot.offsetMin = Vector2.zero;
            workRoot.offsetMax = Vector2.zero;

            Image workBack = CreateImage($"{sceneInfo.HierarchyName}_WorkRoot_Background", workRoot, new Color(0.08f, 0.11f, 0.13f, 0.92f));
            Stretch(workBack.rectTransform);
            AddButtonFrame(workRoot, new Color(0.26f, 0.72f, 0.82f, 0.28f));

            Text workText = CreateText("Work Root Label", workRoot, "개인 작업물을 배치하는 영역입니다.\nHierarchy의 Custom_Decoration_Root 아래에 작업물을 넣어주세요.", 24, new Color(0.88f, 0.94f, 0.96f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(workText.rectTransform, 22f, 18f);

            RectTransform buttonPanel = CreateRect("Personal_Workspace_Link_Buttons", root);
            buttonPanel.anchorMin = new Vector2(0.06f, 0.055f);
            buttonPanel.anchorMax = new Vector2(0.56f, 0.305f);
            buttonPanel.offsetMin = Vector2.zero;
            buttonPanel.offsetMax = Vector2.zero;

            CreateCharacterSceneLoadButton(
                "Go_To_Build_Check_Scene_Button",
                buttonPanel,
                "게임 작업공간으로 이동",
                MainBuildWorkspaceSceneName,
                new Vector2(0f, 0.68f),
                new Vector2(0.49f, 1f),
                new Color(0.72f, 0.16f, 0.14f, 1f),
                Color.white);

            CreateCharacterSceneLoadButton(
                "Go_To_Playable_Game_Scene_Button",
                buttonPanel,
                "게임 테스트 씬으로 이동",
                PlayableGameSceneName,
                new Vector2(0.51f, 0.68f),
                new Vector2(1f, 1f),
                new Color(0.15f, 0.48f, 0.31f, 1f),
                Color.white);

            CreateCharacterSceneLoadButton(
                "Back_To_Main_Hub_Button",
                buttonPanel,
                "메인 허브로 돌아가기",
                MainLobbySceneName,
                new Vector2(0f, 0.34f),
                new Vector2(0.49f, 0.62f),
                new Color(0.9f, 0.64f, 0.2f, 1f),
                new Color(0.055f, 0.06f, 0.07f, 1f));

            CreateCharacterSceneLoadButton(
                "Back_To_Character_Select_Button",
                buttonPanel,
                "캐릭터 선택",
                "SampleScene",
                new Vector2(0.51f, 0.34f),
                new Vector2(1f, 0.62f),
                new Color(0.16f, 0.2f, 0.28f, 1f),
                Color.white);

            CreateWorkspaceResetButton(
                buttonPanel,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.25f));

            RectTransform backButton = CreateButton(
                "Back To Main Menu Button",
                root,
                "Back To Hub",
                24,
                new Color(0.92f, 0.7f, 0.18f, 1f),
                new Color(0.055f, 0.06f, 0.07f, 1f));
            backButton.anchorMin = new Vector2(0.73f, 0.04f);
            backButton.anchorMax = new Vector2(0.93f, 0.105f);
            backButton.offsetMin = Vector2.zero;
            backButton.offsetMax = Vector2.zero;

            MainHub_SceneLoadButton backLoader = backButton.gameObject.AddComponent<MainHub_SceneLoadButton>();
            backLoader.SetSceneName(MainLobbySceneName);
            UnityEventTools.AddPersistentListener(backButton.GetComponent<Button>().onClick, backLoader.LoadScene);
            EditorUtility.SetDirty(backLoader);
            EditorUtility.SetDirty(progressProfile);

            EditorSceneManager.SaveScene(scene, GetScenePath(sceneInfo.SceneName));
        }

        private static void CreatePersonalLobbyHierarchy(Transform workspaceRoot, CharacterSceneInfo sceneInfo)
        {
            GameObject lobbyRoot = new GameObject($"Personal_Lobby_{sceneInfo.HierarchyName}");
            lobbyRoot.transform.SetParent(workspaceRoot, false);

            GameObject decorationRoot = new GameObject($"Custom_Decoration_Root_{sceneInfo.HierarchyName}");
            decorationRoot.transform.SetParent(lobbyRoot.transform, false);

            GameObject workInProgressRoot = new GameObject($"Work_In_Progress_Root_{sceneInfo.HierarchyName}");
            workInProgressRoot.transform.SetParent(lobbyRoot.transform, false);

            GameObject reviewLinkRoot = new GameObject("Commit_Before_Review_Link_Target");
            reviewLinkRoot.transform.SetParent(lobbyRoot.transform, false);

            GameObject moodAnchor = new GameObject("Temporary_Unified_Lobby_Deco_Anchor");
            moodAnchor.transform.SetParent(decorationRoot.transform, false);
        }

        private static RectTransform CreateDeveloperProgressPanel(
            RectTransform root,
            CharacterSceneInfo sceneInfo,
            MainHub_WorkProgressProfile progressProfile)
        {
            RectTransform panel = CreateRect("Developer_Progress_Profile_Panel", root);
            panel.anchorMin = new Vector2(0.06f, 0.55f);
            panel.anchorMax = new Vector2(0.56f, 0.71f);
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;

            Image panelBack = CreateImage("Developer_Progress_Background", panel, new Color(0.075f, 0.095f, 0.12f, 0.94f));
            Stretch(panelBack.rectTransform);
            AddButtonFrame(panel, new Color(0.9f, 0.64f, 0.2f, 0.34f));

            Text profileTitle = CreateText("Developer_Profile_Title", panel, $"{sceneInfo.RoleName} {sceneInfo.OwnerEnglishName}", 24, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            profileTitle.rectTransform.anchorMin = new Vector2(0.04f, 0.58f);
            profileTitle.rectTransform.anchorMax = new Vector2(0.58f, 0.92f);
            profileTitle.rectTransform.offsetMin = Vector2.zero;
            profileTitle.rectTransform.offsetMax = Vector2.zero;

            Text levelText = CreateText("Developer_Level_Text", panel, "DEV LEVEL 01", 24, new Color(0.95f, 0.72f, 0.25f, 1f), TextAnchor.MiddleRight, FontStyle.Bold);
            levelText.rectTransform.anchorMin = new Vector2(0.6f, 0.58f);
            levelText.rectTransform.anchorMax = new Vector2(0.96f, 0.92f);
            levelText.rectTransform.offsetMin = Vector2.zero;
            levelText.rectTransform.offsetMax = Vector2.zero;

            Text statusText = CreateText("Developer_Status_Text", panel, "작업 시간 기록 중", 18, new Color(0.78f, 0.86f, 0.88f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            statusText.rectTransform.anchorMin = new Vector2(0.04f, 0.31f);
            statusText.rectTransform.anchorMax = new Vector2(0.96f, 0.55f);
            statusText.rectTransform.offsetMin = Vector2.zero;
            statusText.rectTransform.offsetMax = Vector2.zero;

            Image expBack = CreateImage("Developer_EXP_Background", panel, new Color(0f, 0f, 0f, 0.48f));
            expBack.rectTransform.anchorMin = new Vector2(0.04f, 0.12f);
            expBack.rectTransform.anchorMax = new Vector2(0.78f, 0.27f);
            expBack.rectTransform.offsetMin = Vector2.zero;
            expBack.rectTransform.offsetMax = Vector2.zero;

            Image expFill = CreateImage("Developer_EXP_Fill", expBack.rectTransform, new Color(0.15f, 0.62f, 0.82f, 0.95f));
            Stretch(expFill.rectTransform);
            expFill.type = Image.Type.Filled;
            expFill.fillMethod = Image.FillMethod.Horizontal;
            expFill.fillAmount = 0f;

            Text expText = CreateText("Developer_EXP_Text", panel, "EXP 000 / 100", 16, Color.white, TextAnchor.MiddleRight, FontStyle.Bold);
            expText.rectTransform.anchorMin = new Vector2(0.8f, 0.07f);
            expText.rectTransform.anchorMax = new Vector2(0.96f, 0.31f);
            expText.rectTransform.offsetMin = Vector2.zero;
            expText.rectTransform.offsetMax = Vector2.zero;

            progressProfile.SetProfile(sceneInfo.SceneName, sceneInfo.RoleName, sceneInfo.OwnerEnglishName);
            progressProfile.SetViewReferences(profileTitle, levelText, statusText, expText, expFill);
            return panel;
        }

        private static void CreateCharacterSceneLoadButton(
            string objectName,
            RectTransform parent,
            string label,
            string sceneName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color backgroundColor,
            Color textColor)
        {
            RectTransform buttonRoot = CreateButton(objectName, parent, label, 21, backgroundColor, textColor);
            buttonRoot.anchorMin = anchorMin;
            buttonRoot.anchorMax = anchorMax;
            buttonRoot.offsetMin = Vector2.zero;
            buttonRoot.offsetMax = Vector2.zero;
            AddButtonFrame(buttonRoot, new Color(1f, 1f, 1f, 0.18f));

            MainHub_SceneLoadButton loader = buttonRoot.gameObject.AddComponent<MainHub_SceneLoadButton>();
            loader.SetSceneName(sceneName);
            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, loader.LoadScene);
            EditorUtility.SetDirty(loader);
        }

        private static void CreateWorkspaceScene(string sceneName, string title, string workspaceRootName)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();
            MainHub_BgmController bgmController = CreateMainHubBgmController();
            CreateBgmToggleCanvas(bgmController);
            MainHub_SceneCameraController cameraController = CreateWorkspaceCamera();

            GameObject workspaceRoot = new GameObject(workspaceRootName);

            if (sceneName == SharedWorkspaceSceneName)
            {
                CreateCinderkeepWorkspacePreview(workspaceRoot.transform);
            }

            if (sceneName == MainBuildWorkspaceSceneName)
            {
                CreateMainBuildWorkspaceMarkers(workspaceRoot.transform);
            }

            GameObject canvasObject = new GameObject(
                $"Canvas_{sceneName}",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(MainHub_UIManager));
            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);
            ConfigureUIManager(canvasObject, cameraController);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Workspace Background", root, new Color(0.012f, 0.014f, 0.02f, 1f));
            Stretch(background.rectTransform);

            RectTransform titleRoot = CreateRect("Workspace Title Area", root);
            titleRoot.anchorMin = new Vector2(0.06f, 0.68f);
            titleRoot.anchorMax = new Vector2(0.75f, 0.88f);
            titleRoot.offsetMin = Vector2.zero;
            titleRoot.offsetMax = Vector2.zero;

            Text titleText = CreateText("Workspace Title", titleRoot, title, 54, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            Stretch(titleText.rectTransform);

            RectTransform guideRoot = CreateRect("Workspace Guide", root);
            guideRoot.anchorMin = new Vector2(0.06f, 0.28f);
            guideRoot.anchorMax = new Vector2(0.56f, 0.58f);
            guideRoot.offsetMin = Vector2.zero;
            guideRoot.offsetMax = Vector2.zero;

            Image guideBack = CreateImage("Workspace Guide Background", guideRoot, new Color(1f, 1f, 1f, 0.055f));
            Stretch(guideBack.rectTransform);

            string guideText = GetWorkspaceGuideText(sceneName, workspaceRoot.name);
            Text guide = CreateText("Workspace Guide Text", guideRoot, guideText, 28, new Color(0.88f, 0.9f, 0.94f, 1f), TextAnchor.MiddleCenter, FontStyle.Normal);
            Stretch(guide.rectTransform, 20f, 20f);

            RectTransform backButton = CreateButton(
                "Back To Lobby Button",
                root,
                "Back To Lobby",
                24,
                new Color(0.92f, 0.7f, 0.18f, 1f),
                new Color(0.055f, 0.06f, 0.07f, 1f));
            backButton.anchorMin = new Vector2(0.06f, 0.1f);
            backButton.anchorMax = new Vector2(0.27f, 0.17f);
            backButton.offsetMin = Vector2.zero;
            backButton.offsetMax = Vector2.zero;

            MainHub_SceneLoadButton backLoader = backButton.gameObject.AddComponent<MainHub_SceneLoadButton>();
            backLoader.SetSceneName(MainLobbySceneName);
            UnityEventTools.AddPersistentListener(backButton.GetComponent<Button>().onClick, backLoader.LoadScene);
            EditorUtility.SetDirty(backLoader);

            if (sceneName == PersonalWorkspaceSceneName)
            {
                CreateWorkspaceResetButton(
                    root,
                    new Vector2(0.29f, 0.1f),
                    new Vector2(0.54f, 0.17f));
            }

            EditorSceneManager.SaveScene(scene, GetScenePath(sceneName));
        }

        private static void CreateWorkspaceResetButton(
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            RectTransform buttonRoot = CreateButton(
                "Discard_Work_And_Reset_Button",
                parent,
                "작업한 내용 폐기하기",
                21,
                new Color(0.56f, 0.08f, 0.08f, 1f),
                Color.white);
            buttonRoot.anchorMin = anchorMin;
            buttonRoot.anchorMax = anchorMax;
            buttonRoot.offsetMin = Vector2.zero;
            buttonRoot.offsetMax = Vector2.zero;
            AddButtonFrame(buttonRoot, new Color(1f, 1f, 1f, 0.2f));

            MainHub_WorkspaceResetButton resetButton = buttonRoot.gameObject.AddComponent<MainHub_WorkspaceResetButton>();
            UnityEventTools.AddPersistentListener(buttonRoot.GetComponent<Button>().onClick, resetButton.ResetCurrentWorkspace);
            EditorUtility.SetDirty(resetButton);
        }

        private static MainHub_SceneCameraController CreateWorkspaceCamera()
        {
            return CreateSceneCamera(new Color(0.08f, 0.1f, 0.14f), 10f, new Vector3(0f, 0f, -10f), Quaternion.identity);
        }

        private static MainHub_SceneCameraController CreateSceneCamera(Color backgroundColor, float orthographicSize, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(MainHub_SceneCameraController));
            SetParent(cameraObject, parent);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.transform.position = position;
            camera.transform.rotation = rotation;

            MainHub_SceneCameraController cameraController = cameraObject.GetComponent<MainHub_SceneCameraController>();
            cameraController.SetCamera(camera);
            EditorUtility.SetDirty(cameraController);
            return cameraController;
        }

        private static void SetParent(GameObject targetObject, Transform parent)
        {
            if (targetObject == null || parent == null)
            {
                return;
            }

            targetObject.transform.SetParent(parent, false);
            targetObject.transform.localPosition = Vector3.zero;
            targetObject.transform.localRotation = Quaternion.identity;
            targetObject.transform.localScale = Vector3.one;
        }

        private static void ConfigureUIManager(GameObject canvasObject, MainHub_SceneCameraController cameraController)
        {
            MainHub_UIManager uiManager = canvasObject.GetComponent<MainHub_UIManager>();
            uiManager.SetCameraController(cameraController);
            EditorUtility.SetDirty(uiManager);
        }

        private static void CreateMainBuildWorkspaceMarkers(Transform workspaceRoot)
        {
            string[] markerNames =
            {
                "Commit_Ready_Area",
                "Push_Ready_Area",
                "Merge_Review_Area",
                "Build_Check_Area",
                "Completed_Feature_Slots"
            };

            for (int i = 0; i < markerNames.Length; i++)
            {
                GameObject marker = new GameObject(markerNames[i]);
                marker.transform.SetParent(workspaceRoot, false);
                marker.transform.localPosition = new Vector3((i - 2) * 2f, 0f, 0f);
            }
        }

        private static void CreateCinderkeepWorkspacePreview(Transform workspaceRoot)
        {
            Camera camera = GetFirstComponentInOpenScene<Camera>(true);
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camera = cameraObject.GetComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 24f, -22f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 13f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.18f, 0.12f, 1f);

            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreatePrimitiveActor(
                "Cinderkeep_Green_Ground",
                PrimitiveType.Cube,
                workspaceRoot,
                Vector3.zero,
                new Vector3(24f, 0.18f, 18f),
                new Color(0.13f, 0.42f, 0.18f, 1f),
                "Ground",
                "Green Field",
                "Placeholder arena for the Cinderkeep shared workspace.");

            CreatePrimitiveActor(
                "Player_Green_Dot",
                PrimitiveType.Sphere,
                workspaceRoot,
                new Vector3(0f, 0.8f, -1.5f),
                Vector3.one * 0.8f,
                new Color(0.2f, 1f, 0.32f, 1f),
                "Player",
                "Green Player Dot",
                "Player placeholder for first-person movement and survival tests.");

            CreatePrimitiveActor(
                "Base_Core_Object",
                PrimitiveType.Cylinder,
                workspaceRoot,
                new Vector3(0f, 0.75f, 1.5f),
                new Vector3(1.4f, 0.7f, 1.4f),
                new Color(0.7f, 0.95f, 0.92f, 1f),
                "BaseCore",
                "Base Core",
                "Night enemies can target this core, matching the base-defense plan.");

            CreatePrimitiveActor("Mineral_Blue_Box_01", PrimitiveType.Cube, workspaceRoot, new Vector3(-5f, 0.55f, 3f), Vector3.one, new Color(0.1f, 0.38f, 1f, 1f), "Mineral", "Blue Mineral", "Mineable resource placeholder.");
            CreatePrimitiveActor("Mineral_Blue_Box_02", PrimitiveType.Cube, workspaceRoot, new Vector3(4.5f, 0.55f, 4.2f), Vector3.one, new Color(0.1f, 0.38f, 1f, 1f), "Mineral", "Blue Mineral", "Mineable resource placeholder.");
            CreatePrimitiveActor("Chest_Yellow_Box_01", PrimitiveType.Cube, workspaceRoot, new Vector3(-3.5f, 0.55f, -5f), Vector3.one, new Color(1f, 0.82f, 0.08f, 1f), "Chest", "Yellow Chest", "Random growth placeholder for attack speed, move speed, jump, critical, or lifesteal.");
            CreatePrimitiveActor("Chest_Yellow_Box_02", PrimitiveType.Cube, workspaceRoot, new Vector3(5.5f, 0.55f, -3.8f), Vector3.one, new Color(1f, 0.82f, 0.08f, 1f), "Chest", "Yellow Chest", "Random growth placeholder for replayable build choices.");
            CreatePrimitiveActor("Enemy_Red_Dot_PlayerChaser", PrimitiveType.Sphere, workspaceRoot, new Vector3(-7f, 0.65f, 0.5f), Vector3.one * 0.75f, new Color(1f, 0.1f, 0.08f, 1f), "Enemy", "Player Chaser", "Red enemy dot that represents player-targeting wave pressure.");
            CreatePrimitiveActor("Enemy_Red_Dot_BaseBreaker", PrimitiveType.Sphere, workspaceRoot, new Vector3(7f, 0.65f, 0.8f), Vector3.one * 0.9f, new Color(1f, 0.1f, 0.08f, 1f), "Enemy", "Base Breaker", "Red enemy dot that represents base-targeting night pressure.");
        }

        private static string GetWorkspaceGuideText(string sceneName, string workspaceRootName)
        {
            if (sceneName == SharedWorkspaceSceneName)
            {
                return "Cinderkeep team workspace: organize shared tasks, member-owned objects, and game ideas before they move into the real game scene.";
            }

            if (sceneName == MainBuildWorkspaceSceneName)
            {
                return "Game work review room: collect finished work here before commit, push, merge, or build checks.";
            }

            return $"Add work objects under {workspaceRootName}.";
        }

        private static GameObject CreatePrimitiveActor(
            string name,
            PrimitiveType primitiveType,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color,
            string actorType,
            string displayName,
            string description)
        {
            GameObject actor = GameObject.CreatePrimitive(primitiveType);
            actor.name = name;
            actor.transform.SetParent(parent, false);
            actor.transform.localPosition = position;
            actor.transform.localScale = scale;

            Renderer renderer = actor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreatePreviewMaterial(color);
            }

            MainHub_WorkspaceActor workspaceActor = actor.AddComponent<MainHub_WorkspaceActor>();
            workspaceActor.SetProfile(actorType, displayName, description);
            EditorUtility.SetDirty(workspaceActor);
            return actor;
        }

        private static Material CreatePreviewMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(GetScenePath(MainLobbySceneName), true),
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(GetScenePath(PlayableGameSceneName), true),
                new EditorBuildSettingsScene(GetScenePath(SharedWorkspaceSceneName), true),
                new EditorBuildSettingsScene(GetScenePath(MainBuildWorkspaceSceneName), true),
                new EditorBuildSettingsScene(GetScenePath(PersonalWorkspaceSceneName), true)
            };

            foreach (CharacterSceneInfo sceneInfo in CharacterScenes)
            {
                scenes.Add(new EditorBuildSettingsScene(GetScenePath(sceneInfo.SceneName), true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static string GetScenePath(string sceneName)
        {
            if (sceneName == MainLobbySceneName)
            {
                return $"{SceneFolder}/{sceneName}.unity";
            }

            if (sceneName == PlayableGameSceneName)
            {
                return $"{MainGameSceneFolder}/{sceneName}.unity";
            }

            if (sceneName == SharedWorkspaceSceneName || sceneName == MainBuildWorkspaceSceneName)
            {
                return $"{MainWorkspaceSceneFolder}/{sceneName}.unity";
            }

            return $"{CharacterSceneFolder}/{sceneName}.unity";
        }

        private static void EnsureMainGameSceneFolder()
        {
            if (AssetDatabase.IsValidFolder(MainGameSceneFolder))
            {
                return;
            }

            EnsureSceneFolder();
            AssetDatabase.CreateFolder(SceneFolder, "MainGame");
        }

        private static void EnsureMainWorkspaceSceneFolder()
        {
            if (AssetDatabase.IsValidFolder(MainWorkspaceSceneFolder))
            {
                return;
            }

            EnsureSceneFolder();
            AssetDatabase.CreateFolder(SceneFolder, "MainWorkspace");
        }

        private static void EnsureCharacterSceneFolder()
        {
            if (AssetDatabase.IsValidFolder(CharacterSceneFolder))
            {
                return;
            }

            EnsureSceneFolder();
            AssetDatabase.CreateFolder(SceneFolder, "CharacterScenes");
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static void ValidateWorkspaceScene(string sceneName, string workspaceRootName)
        {
            EditorSceneManager.OpenScene(GetScenePath(sceneName), OpenSceneMode.Single);
            ValidateCameraAndUIManager(sceneName);
            if (GetRootGameObjectByName(SceneManager.GetActiveScene(), workspaceRootName) == null)
            {
                throw new System.InvalidOperationException($"{sceneName} has no {workspaceRootName}.");
            }

            if (sceneName == PersonalWorkspaceSceneName && GetTransformByNameIncludingInactive("Discard_Work_And_Reset_Button") == null)
            {
                throw new System.InvalidOperationException($"{sceneName} has no workspace reset button.");
            }
        }

        private static void ValidateCameraAndUIManager(string sceneName)
        {
            MainHub_SceneCameraController cameraController = GetFirstComponentInOpenScene<MainHub_SceneCameraController>(true);
            if (cameraController == null)
            {
                throw new System.InvalidOperationException($"{sceneName} has no MainHub_SceneCameraController.");
            }

            MainHub_UIManager uiManager = GetFirstComponentInOpenScene<MainHub_UIManager>(true);
            if (uiManager == null)
            {
                throw new System.InvalidOperationException($"{sceneName} has no MainHub_UIManager.");
            }

            if (uiManager.CameraController == null)
            {
                throw new System.InvalidOperationException($"{sceneName} MainHub_UIManager has no camera controller.");
            }
        }

        private static void ValidateBuildStartScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length == 0 || scenes[0].path != GetScenePath(MainLobbySceneName))
            {
                string firstScene = scenes.Length == 0 ? "none" : scenes[0].path;
                throw new System.InvalidOperationException($"First build scene is {firstScene}, expected {GetScenePath(MainLobbySceneName)}.");
            }
        }

        private static void ValidateCharacterDetailPanel()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            MainHub_CharacterDetailPanel detailPanel = GetFirstComponentInOpenScene<MainHub_CharacterDetailPanel>(true);
            if (detailPanel == null)
            {
                throw new System.InvalidOperationException("Character detail panel is missing.");
            }

            if (GetTransformByNameIncludingInactive("Open Personal Workspace Button") == null)
            {
                throw new System.InvalidOperationException("Open Personal Workspace Button is missing.");
            }

            if (GetTransformByNameIncludingInactive("Name Label") == null || GetTransformByNameIncludingInactive("Description Label") == null)
            {
                throw new System.InvalidOperationException("Name or Description label is missing.");
            }
        }

        private static void ValidateSharedCinderkeepWorkspacePreview()
        {
            EditorSceneManager.OpenScene(GetScenePath(SharedWorkspaceSceneName), OpenSceneMode.Single);
            string[] requiredObjects =
            {
                "Cinderkeep_Green_Ground",
                "Player_Green_Dot",
                "Base_Core_Object",
                "Mineral_Blue_Box_01",
                "Chest_Yellow_Box_01",
                "Enemy_Red_Dot_PlayerChaser",
                "Enemy_Red_Dot_BaseBreaker"
            };

            foreach (string objectName in requiredObjects)
            {
                GameObject actor = GetGameObjectByNameIncludingInactive(objectName);
                if (actor == null)
                {
                    throw new System.InvalidOperationException($"{objectName} is missing from shared workspace.");
                }

                if (actor.GetComponent<MainHub_WorkspaceActor>() == null)
                {
                    throw new System.InvalidOperationException($"{objectName} has no MainHub_WorkspaceActor.");
                }
            }
        }

        private static GameObject GetGameObjectByNameIncludingInactive(string objectName)
        {
            Transform transform = GetTransformByNameIncludingInactive(objectName);
            if (transform == null)
            {
                return null;
            }

            return transform.gameObject;
        }

        private static GameObject GetRootGameObjectByName(Scene scene, string objectName)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name == objectName)
                {
                    return rootObjects[i];
                }
            }

            return null;
        }

        private static TComponent GetFirstComponentInOpenScene<TComponent>(bool includeInactive)
            where TComponent : Component
        {
            TComponent[] components = GetComponentsInOpenScene<TComponent>(includeInactive);
            if (components.Length == 0)
            {
                return null;
            }

            return components[0];
        }

        private static TComponent[] GetComponentsInOpenScene<TComponent>(bool includeInactive)
            where TComponent : Component
        {
            List<TComponent> components = new List<TComponent>();
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();

            for (int i = 0; i < rootObjects.Length; i++)
            {
                TComponent[] childComponents = rootObjects[i].GetComponentsInChildren<TComponent>(includeInactive);
                for (int j = 0; j < childComponents.Length; j++)
                {
                    components.Add(childComponents[j]);
                }
            }

            return components.ToArray();
        }

        private static Transform GetTransformByNameIncludingInactive(string objectName)
        {
            Scene scene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();

            for (int i = 0; i < rootObjects.Length; i++)
            {
                Transform result = GetChildTransformByName(rootObjects[i].transform, objectName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static Transform GetChildTransformByName(Transform root, string objectName)
        {
            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform result = GetChildTransformByName(child, objectName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child.GetComponent<RectTransform>();
        }

        private static Image CreateImage(string name, RectTransform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void AddButtonFrame(RectTransform buttonRoot, Color color)
        {
            Outline outline = buttonRoot.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static Text CreateText(string name, RectTransform parent, string value, int size, Color color, TextAnchor alignment, FontStyle style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = GetDefaultFont();
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(10, size - 8);
            text.resizeTextMaxSize = size;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void Stretch(RectTransform rect, float horizontalInset = 0f, float verticalInset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(horizontalInset, verticalInset);
            rect.offsetMax = new Vector2(-horizontalInset, -verticalInset);
        }

        private static Font GetDefaultFont()
        {
            Font configuredKoreanFont = FindFontAsset("ChosunCentennial");
            if (configuredKoreanFont != null)
            {
                return configuredKoreanFont;
            }

            Font fallbackKoreanFont = AssetDatabase.LoadAssetAtPath<Font>(KoreanFallbackFontPath);
            if (fallbackKoreanFont != null)
            {
                return fallbackKoreanFont;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Font FindFontAsset(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:Font");
            foreach (string guid in guids)
            {
                Font font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null)
                {
                    return font;
                }
            }

            return null;
        }

        private readonly struct CharacterSceneInfo
        {
            public CharacterSceneInfo(string resourceId, string roleName, string ownerEnglishName)
            {
                ResourceId = resourceId;
                RoleName = roleName;
                OwnerEnglishName = ownerEnglishName;
            }

            public string ResourceId { get; }
            public string RoleName { get; }
            public string SceneName
            {
                get
                {
                    return HierarchyName;
                }
            }

            public string OwnerEnglishName { get; }

            public string HierarchyName
            {
                get
                {
                    return RoleName + "_" + OwnerEnglishName;
                }
            }
        }
    }
}
