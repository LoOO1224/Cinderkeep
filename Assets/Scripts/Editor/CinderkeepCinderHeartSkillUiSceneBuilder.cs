using Cinderkeep.Gameplay;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
// Cinderkeep_Game 씬에 CinderHeart 아침 보상 선택 UI를 배치하는 에디터 도구입니다.
// 런타임에서는 미리 배치된 UI를 켜고 끄기만 합니다.
public static class CinderkeepCinderHeartSkillUiSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF3.asset";
    private const string CanvasName = "Canvas_GameHUD";

    [MenuItem("Cinderkeep/Main Game/Apply CinderHeart Skill UI")]
    public static void ApplyCinderHeartSkillUI()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        Canvas canvas = GetComponentInScene<Canvas>(CanvasName);
        UIManager uiManager = GetComponentInScene<UIManager>("UIManager");
        CinderHeart cinderHeart = GetComponentInScene<CinderHeart>("CinderHeart");
        PlayerStatus playerStatus = GetComponentInScene<PlayerStatus>("Player");
        if (canvas == null || uiManager == null || cinderHeart == null || playerStatus == null)
        {
            Debug.LogWarning("CinderkeepCinderHeartSkillUiSceneBuilder: 필요한 씬 오브젝트를 찾지 못했습니다.");
            return;
        }

        EnsureCanvasInput(canvas);
        EnsureEventSystem();

        bool shouldRebuild;
        RectTransform root = GetOrCreateSkillRoot(canvas.transform, out shouldRebuild);
        if (shouldRebuild == true)
        {
            ClearChildren(root);
            SetupRoot(root);
            BuildSkillContent(root, uiManager, cinderHeart, playerStatus);
        }
        else
        {
            SetupReferences(root, uiManager, cinderHeart, playerStatus);
        }

        root.gameObject.SetActive(false);
        EditorUtility.SetDirty(root.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CinderkeepCinderHeartSkillUiSceneBuilder: CinderHeart skill UI setup finished.");
    }

    public static void ApplyCinderHeartSkillUIFromCommandLine()
    {
        ApplyCinderHeartSkillUI();
    }

    private static void EnsureCanvasInput(Canvas canvas)
    {
        GraphicRaycaster raycaster = GetOrAddComponent<GraphicRaycaster>(canvas.gameObject);
        raycaster.ignoreReversedGraphics = true;
    }

    private static void EnsureEventSystem()
    {
        GameObject eventSystemObject = GetSceneObjectByName("EventSystem_GameUI");
        if (eventSystemObject == null)
        {
            eventSystemObject = new GameObject("EventSystem_GameUI");
        }

        GetOrAddComponent<EventSystem>(eventSystemObject);
        InputSystemUIInputModule inputModule = GetOrAddComponent<InputSystemUIInputModule>(eventSystemObject);
        inputModule.AssignDefaultActions();
        EditorUtility.SetDirty(eventSystemObject);
    }

    private static RectTransform GetOrCreateSkillRoot(Transform canvasTransform, out bool shouldRebuild)
    {
        Transform root = canvasTransform.Find("Panel_CinderHeartSkillSelection");
        if (root != null)
        {
            shouldRebuild = root.childCount <= 0;
            return root.GetComponent<RectTransform>();
        }

        GameObject rootObject = new GameObject("Panel_CinderHeartSkillSelection");
        rootObject.transform.SetParent(canvasTransform);
        RectTransform rectTransform = rootObject.AddComponent<RectTransform>();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        shouldRebuild = true;
        return rectTransform;
    }

    private static void SetupRoot(RectTransform root)
    {
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.pivot = new Vector2(0.5f, 0.5f);
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        root.localScale = Vector3.one;

        Image backgroundImage = GetOrAddComponent<Image>(root.gameObject);
        backgroundImage.color = new Color(0.01f, 0.015f, 0.02f, 0.78f);
        backgroundImage.raycastTarget = true;
    }

    private static void BuildSkillContent(
        RectTransform root,
        UIManager uiManager,
        CinderHeart cinderHeart,
        PlayerStatus playerStatus)
    {
        RectTransform contentPanel = CreatePanel(root, "Panel_SkillContent", new Color(0.08f, 0.09f, 0.10f, 0.96f));
        SetRect(contentPanel, new Vector2(0.5f, 0.5f), new Vector2(980f, 520f), Vector2.zero);

        TMP_Text titleText = CreateText(contentPanel, "Text_SkillSelectionTitle", "셋 중에 하나를 고르시오", 34f, TextAlignmentOptions.Center);
        SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(900f, 54f), new Vector2(0f, -46f));

        CinderHeartSkillOptionView[] optionViews = new CinderHeartSkillOptionView[3];
        optionViews[0] = CreateOptionView(contentPanel, "Button_CinderHeartSkill_Left", new Vector2(-310f, 20f));
        optionViews[1] = CreateOptionView(contentPanel, "Button_CinderHeartSkill_Center", new Vector2(0f, 20f));
        optionViews[2] = CreateOptionView(contentPanel, "Button_CinderHeartSkill_Right", new Vector2(310f, 20f));

        Button skipButton = CreateButton(contentPanel, "Button_SkipCinderHeartSkill", "스킬 고르지 않고 넘어가기", 22f);
        SetRect(skipButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(360f, 58f), new Vector2(0f, 58f));

        CinderHeartSkillApplier skillApplier = GetOrAddComponent<CinderHeartSkillApplier>(root.gameObject);
        SerializedObject serializedApplier = new SerializedObject(skillApplier);
        serializedApplier.FindProperty("_cinderHeart").objectReferenceValue = cinderHeart;
        serializedApplier.FindProperty("_playerStatus").objectReferenceValue = playerStatus;
        serializedApplier.ApplyModifiedPropertiesWithoutUndo();

        CinderHeartSkillSelectionUI skillSelectionUI = GetOrAddComponent<CinderHeartSkillSelectionUI>(root.gameObject);
        SerializedObject serializedUi = new SerializedObject(skillSelectionUI);
        serializedUi.FindProperty("_rootObject").objectReferenceValue = root.gameObject;
        serializedUi.FindProperty("_titleText").objectReferenceValue = titleText;
        serializedUi.FindProperty("_skipButton").objectReferenceValue = skipButton;
        serializedUi.FindProperty("_skillApplier").objectReferenceValue = skillApplier;
        SetObjectArray(serializedUi.FindProperty("_optionViews"), optionViews);
        serializedUi.ApplyModifiedPropertiesWithoutUndo();

        SetupUIManager(uiManager, skillSelectionUI);
    }

    private static CinderHeartSkillOptionView CreateOptionView(Transform parent, string objectName, Vector2 anchoredPosition)
    {
        Button button = CreateButton(parent, objectName, "", 20f);
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        SetRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(280f, 270f), anchoredPosition);

        TMP_Text nameText = CreateText(buttonRect, "Text_SkillName", "Skill", 23f, TextAlignmentOptions.Center);
        SetRect(nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(240f, 70f), new Vector2(0f, -48f));

        TMP_Text descriptionText = CreateText(buttonRect, "Text_SkillDescription", "Description", 18f, TextAlignmentOptions.Center);
        SetRect(descriptionText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(238f, 150f), new Vector2(0f, -24f));

        CinderHeartSkillOptionView optionView = GetOrAddComponent<CinderHeartSkillOptionView>(button.gameObject);
        SerializedObject serializedObject = new SerializedObject(optionView);
        serializedObject.FindProperty("_button").objectReferenceValue = button;
        serializedObject.FindProperty("_nameText").objectReferenceValue = nameText;
        serializedObject.FindProperty("_descriptionText").objectReferenceValue = descriptionText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return optionView;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, float fontSize)
    {
        RectTransform buttonRoot = CreatePanel(parent, objectName, new Color(0.15f, 0.18f, 0.22f, 0.98f));
        Button button = GetOrAddComponent<Button>(buttonRoot.gameObject);
        button.targetGraphic = buttonRoot.GetComponent<Image>();

        if (string.IsNullOrEmpty(label) == false)
        {
            TMP_Text labelText = CreateText(buttonRoot, "Text_Label", label, fontSize, TextAlignmentOptions.Center);
            SetRect(labelText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(330f, 46f), Vector2.zero);
        }

        return button;
    }

    private static RectTransform CreatePanel(Transform parent, string objectName, Color color)
    {
        GameObject panelObject = new GameObject(objectName);
        panelObject.transform.SetParent(parent);
        panelObject.transform.localRotation = Quaternion.identity;
        panelObject.transform.localScale = Vector3.one;

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        Image image = panelObject.AddComponent<Image>();
        image.color = color;
        return rectTransform;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent);
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.color = Color.white;
        tmpText.raycastTarget = false;
        ApplyTmpFont(tmpText);
        return tmpText;
    }

    private static void SetupReferences(
        RectTransform root,
        UIManager uiManager,
        CinderHeart cinderHeart,
        PlayerStatus playerStatus)
    {
        CinderHeartSkillSelectionUI skillSelectionUI = root.GetComponent<CinderHeartSkillSelectionUI>();
        CinderHeartSkillApplier skillApplier = root.GetComponent<CinderHeartSkillApplier>();
        if (skillApplier != null)
        {
            SerializedObject serializedApplier = new SerializedObject(skillApplier);
            serializedApplier.FindProperty("_cinderHeart").objectReferenceValue = cinderHeart;
            serializedApplier.FindProperty("_playerStatus").objectReferenceValue = playerStatus;
            serializedApplier.ApplyModifiedPropertiesWithoutUndo();
        }

        if (skillSelectionUI != null)
        {
            SetupUIManager(uiManager, skillSelectionUI);
        }
    }

    private static void SetupUIManager(UIManager uiManager, CinderHeartSkillSelectionUI skillSelectionUI)
    {
        SerializedObject serializedObject = new SerializedObject(uiManager);
        serializedObject.FindProperty("_cinderHeartSkillSelectionUI").objectReferenceValue = skillSelectionUI;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(uiManager);
    }

    private static void SetObjectArray<TComponent>(SerializedProperty arrayProperty, TComponent[] components)
        where TComponent : Object
    {
        arrayProperty.arraySize = components.Length;
        for (int i = 0; i < components.Length; i++)
        {
            SerializedProperty itemProperty = arrayProperty.GetArrayElementAtIndex(i);
            itemProperty.objectReferenceValue = components[i];
        }
    }

    private static void SetRect(RectTransform rectTransform, Vector2 anchor, Vector2 size, Vector2 anchoredPosition)
    {
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = anchor;
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
    }

    private static void ApplyTmpFont(TMP_Text text)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset == null)
        {
            return;
        }

        text.font = fontAsset;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static T GetOrAddComponent<T>(GameObject targetObject)
        where T : Component
    {
        T component = targetObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return targetObject.AddComponent<T>();
    }

    private static T GetComponentInScene<T>(string objectName)
        where T : Component
    {
        GameObject sceneObject = GetSceneObjectByName(objectName);
        if (sceneObject == null)
        {
            return null;
        }

        return sceneObject.GetComponent<T>();
    }

    private static GameObject GetSceneObjectByName(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            GameObject result = GetChildByName(rootObjects[i].transform, objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static GameObject GetChildByName(Transform rootTransform, string objectName)
    {
        if (rootTransform.name == objectName)
        {
            return rootTransform.gameObject;
        }

        for (int i = 0; i < rootTransform.childCount; i++)
        {
            GameObject result = GetChildByName(rootTransform.GetChild(i), objectName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
