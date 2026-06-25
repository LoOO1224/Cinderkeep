using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 에디터에서 씬 세팅, Check 리포트, 팀 작업 인수인계를 빠르게 처리하는 개발 도구입니다.
// 런타임 빌드에는 포함되지 않으며, 반복되는 수동 연결과 Check 작업을 줄이는 데 사용합니다.
public static class CinderkeepCinderHeartHudSceneBuilder
{
    private const string GameScenePath = "Assets/Scenes/MainGame/Cinderkeep_Game.unity";
    private const string FontPath = "Assets/Fonts/NotoSansKR-Medium SDF3.asset";

    [MenuItem("Cinderkeep/Setup CinderHeart HUD")]
    public static void SetupCinderHeartHUD()
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        Canvas canvas = GetComponentInScene<Canvas>("Canvas_GameHUD");
        CinderHeart cinderHeart = GetComponentInScene<CinderHeart>("CinderHeart");

        if (canvas == null || cinderHeart == null)
        {
            Debug.LogWarning("CinderkeepCinderHeartHudSceneBuilder: Canvas_GameHUD 또는 CinderHeart를 찾지 못했습니다.");
            return;
        }

        bool isRootCreated;
        RectTransform root = GetOrCreateRectTransform(canvas.transform, "Panel_CinderHeartHUD", out isRootCreated);

        CanvasGroup canvasGroup = GetOrAddComponent<CanvasGroup>(root.gameObject);
        Image backgroundImage = GetOrAddComponent<Image>(root.gameObject);
        backgroundImage.raycastTarget = false;

        if (isRootCreated)
        {
            SetupRoot(root);
            backgroundImage.color = new Color(0.03f, 0.04f, 0.05f, 0.78f);
        }

        Slider slider = CreateHealthSlider(root, isRootCreated);
        TMP_Text healthText = CreateHealthText(root, isRootCreated);

        CinderHeartHUD cinderHeartHUD = GetOrAddComponent<CinderHeartHUD>(root.gameObject);
        SerializedObject serializedObject = new SerializedObject(cinderHeartHUD);
        serializedObject.FindProperty("_targetCinderHeart").objectReferenceValue = cinderHeart;
        serializedObject.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        serializedObject.FindProperty("_healthSlider").objectReferenceValue = slider;
        serializedObject.FindProperty("_healthText").objectReferenceValue = healthText;
        serializedObject.FindProperty("_isAlwaysVisible").boolValue = true;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        CreateEnemyTargetHUD(canvas.transform);

        EditorUtility.SetDirty(root.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CinderkeepCinderHeartHudSceneBuilder: CinderHeart HUD setup finished.");
    }

    private static void SetupRoot(RectTransform root)
    {
        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = new Vector2(0f, -18f);
        root.sizeDelta = new Vector2(440f, 58f);
        root.localScale = Vector3.one;
    }

    private static Slider CreateHealthSlider(RectTransform parent, bool shouldApplyDefaultLayout)
    {
        bool isSliderCreated;
        RectTransform sliderRoot = GetOrCreateRectTransform(parent, "Slider_CinderHeartHealth", out isSliderCreated);

        if (shouldApplyDefaultLayout || isSliderCreated)
        {
            sliderRoot.anchorMin = new Vector2(0.04f, 0.16f);
            sliderRoot.anchorMax = new Vector2(0.96f, 0.46f);
            sliderRoot.offsetMin = Vector2.zero;
            sliderRoot.offsetMax = Vector2.zero;
        }

        Image backgroundImage = GetOrAddComponent<Image>(sliderRoot.gameObject);
        backgroundImage.raycastTarget = false;

        if (shouldApplyDefaultLayout || isSliderCreated)
        {
            backgroundImage.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        }

        bool isFillAreaCreated;
        RectTransform fillArea = GetOrCreateRectTransform(sliderRoot, "FillArea", out isFillAreaCreated);
        if (shouldApplyDefaultLayout || isFillAreaCreated)
        {
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(2f, 2f);
            fillArea.offsetMax = new Vector2(-2f, -2f);
        }

        bool isFillCreated;
        RectTransform fill = GetOrCreateRectTransform(fillArea, "Fill", out isFillCreated);
        if (shouldApplyDefaultLayout || isFillCreated)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        Image fillImage = GetOrAddComponent<Image>(fill.gameObject);
        fillImage.raycastTarget = false;

        if (shouldApplyDefaultLayout || isFillCreated)
        {
            fillImage.color = new Color(0.86f, 0.12f, 0.08f, 1f);
        }

        Slider slider = GetOrAddComponent<Slider>(sliderRoot.gameObject);
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 500f;
        slider.value = 500f;
        slider.fillRect = fill;
        slider.targetGraphic = fillImage;

        return slider;
    }

    private static TMP_Text CreateHealthText(RectTransform parent, bool shouldApplyDefaultLayout)
    {
        bool isTextCreated;
        RectTransform textRoot = GetOrCreateRectTransform(parent, "Text_CinderHeartHealth", out isTextCreated);

        if (shouldApplyDefaultLayout || isTextCreated)
        {
            textRoot.anchorMin = new Vector2(0f, 0.5f);
            textRoot.anchorMax = Vector2.one;
            textRoot.offsetMin = new Vector2(8f, 0f);
            textRoot.offsetMax = new Vector2(-8f, -2f);
        }

        TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textRoot.gameObject);
        text.text = "CinderHeart HP 500 / 500";
        text.raycastTarget = false;

        if (shouldApplyDefaultLayout || isTextCreated)
        {
            text.fontSize = 20f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        return text;
    }

    private static void CreateEnemyTargetHUD(Transform canvasTransform)
    {
        bool isRootCreated;
        RectTransform root = GetOrCreateRectTransform(canvasTransform, "Panel_EnemyTargetHUD", out isRootCreated);

        CanvasGroup canvasGroup = GetOrAddComponent<CanvasGroup>(root.gameObject);
        Image backgroundImage = GetOrAddComponent<Image>(root.gameObject);
        backgroundImage.raycastTarget = false;

        if (isRootCreated)
        {
            SetupEnemyTargetRoot(root);
            backgroundImage.color = new Color(0.03f, 0.04f, 0.05f, 0.72f);
        }

        Slider slider = CreateEnemyHealthSlider(root, isRootCreated);
        TMP_Text healthText = CreateEnemyHealthText(root, isRootCreated);

        EnemyTargetHUD enemyTargetHUD = GetOrAddComponent<EnemyTargetHUD>(root.gameObject);
        SerializedObject serializedObject = new SerializedObject(enemyTargetHUD);
        serializedObject.FindProperty("_canvasGroup").objectReferenceValue = canvasGroup;
        serializedObject.FindProperty("_healthSlider").objectReferenceValue = slider;
        serializedObject.FindProperty("_healthText").objectReferenceValue = healthText;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetupEnemyTargetRoot(RectTransform root)
    {
        root.anchorMin = new Vector2(0.5f, 1f);
        root.anchorMax = new Vector2(0.5f, 1f);
        root.pivot = new Vector2(0.5f, 1f);
        root.anchoredPosition = new Vector2(0f, -84f);
        root.sizeDelta = new Vector2(320f, 44f);
        root.localScale = Vector3.one;
    }

    private static Slider CreateEnemyHealthSlider(RectTransform parent, bool shouldApplyDefaultLayout)
    {
        bool isSliderCreated;
        RectTransform sliderRoot = GetOrCreateRectTransform(parent, "Slider_EnemyTargetHealth", out isSliderCreated);

        if (shouldApplyDefaultLayout || isSliderCreated)
        {
            sliderRoot.anchorMin = new Vector2(0.06f, 0.12f);
            sliderRoot.anchorMax = new Vector2(0.94f, 0.46f);
            sliderRoot.offsetMin = Vector2.zero;
            sliderRoot.offsetMax = Vector2.zero;
        }

        Image backgroundImage = GetOrAddComponent<Image>(sliderRoot.gameObject);
        backgroundImage.raycastTarget = false;

        if (shouldApplyDefaultLayout || isSliderCreated)
        {
            backgroundImage.color = new Color(0.10f, 0.10f, 0.10f, 1f);
        }

        bool isFillAreaCreated;
        RectTransform fillArea = GetOrCreateRectTransform(sliderRoot, "FillArea", out isFillAreaCreated);
        if (shouldApplyDefaultLayout || isFillAreaCreated)
        {
            fillArea.anchorMin = Vector2.zero;
            fillArea.anchorMax = Vector2.one;
            fillArea.offsetMin = new Vector2(2f, 2f);
            fillArea.offsetMax = new Vector2(-2f, -2f);
        }

        bool isFillCreated;
        RectTransform fill = GetOrCreateRectTransform(fillArea, "Fill", out isFillCreated);
        if (shouldApplyDefaultLayout || isFillCreated)
        {
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = Vector2.one;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }

        Image fillImage = GetOrAddComponent<Image>(fill.gameObject);
        fillImage.raycastTarget = false;

        if (shouldApplyDefaultLayout || isFillCreated)
        {
            fillImage.color = new Color(0.86f, 0.34f, 0.08f, 1f);
        }

        Slider slider = GetOrAddComponent<Slider>(sliderRoot.gameObject);
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
        slider.fillRect = fill;
        slider.targetGraphic = fillImage;

        return slider;
    }

    private static TMP_Text CreateEnemyHealthText(RectTransform parent, bool shouldApplyDefaultLayout)
    {
        bool isTextCreated;
        RectTransform textRoot = GetOrCreateRectTransform(parent, "Text_EnemyTargetHealth", out isTextCreated);

        if (shouldApplyDefaultLayout || isTextCreated)
        {
            textRoot.anchorMin = new Vector2(0f, 0.5f);
            textRoot.anchorMax = Vector2.one;
            textRoot.offsetMin = new Vector2(8f, 0f);
            textRoot.offsetMax = new Vector2(-8f, -2f);
        }

        TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textRoot.gameObject);
        text.text = "Enemy HP 100 / 100";
        text.raycastTarget = false;

        if (shouldApplyDefaultLayout || isTextCreated)
        {
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (fontAsset != null)
        {
            text.font = fontAsset;
        }

        return text;
    }

    private static RectTransform GetOrCreateRectTransform(Transform parent, string objectName, out bool isCreated)
    {
        Transform child = parent.Find(objectName);
        if (child != null)
        {
            isCreated = false;
            return child.GetComponent<RectTransform>();
        }

        GameObject childObject = new GameObject(objectName);
        childObject.transform.SetParent(parent);
        RectTransform rectTransform = childObject.AddComponent<RectTransform>();
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        isCreated = true;
        return rectTransform;
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
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            T component = GetComponentInChildrenByObjectName<T>(rootObjects[i], objectName);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static T GetComponentInChildrenByObjectName<T>(GameObject rootObject, string objectName)
        where T : Component
    {
        if (rootObject.name == objectName)
        {
            return rootObject.GetComponent<T>();
        }

        T[] components = rootObject.GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i].gameObject.name == objectName)
            {
                return components[i];
            }
        }

        return null;
    }
}
