using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 행동 실패나 짧은 상태 변화를 화면에 알려주는 공용 HUD 메시지입니다.
// 게임 규칙을 판단하지 않고, 다른 시스템이 넘겨준 문구를 잠깐 표시하는 역할만 합니다.
public sealed class GameplayFeedbackHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_GameplayFeedbackHud";

    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private float _visibleSeconds = 1.45f;

    private static GameplayFeedbackHud _instance;
    private float _hideAtTime;

    public static GameplayFeedbackHud EnsureSceneHud()
    {
        if (_instance != null)
        {
            _instance.gameObject.SetActive(true);
            return _instance;
        }

        GameplayFeedbackHud existing = FindFirstObjectByType<GameplayFeedbackHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            _instance = existing;
            _instance.gameObject.SetActive(true);
            return _instance;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[GameplayFeedbackHud] 씬에 배치된 HUD가 없어 런타임 생성을 건너뜁니다.");
            return null;
        }

        GameObject canvasObject = GameObject.Find(CanvasName);
        if (canvasObject == null)
        {
            return null;
        }

        Transform hudRoot = canvasObject.transform.Find(HudRootName);
        if (hudRoot == null)
        {
            GameObject hudRootObject = new GameObject(HudRootName, typeof(RectTransform));
            hudRootObject.transform.SetParent(canvasObject.transform, false);
            hudRoot = hudRootObject.transform;
        }

        GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(hudRoot, false);
        ConfigureRoot(rootObject.GetComponent<RectTransform>(), rootObject.GetComponent<Image>());

        GameplayFeedbackHud hud = rootObject.AddComponent<GameplayFeedbackHud>();
        hud._backgroundImage = rootObject.GetComponent<Image>();
        hud._messageText = CreateMessageText(rootObject.transform);
        hud.SetVisible(false);
        _instance = hud;
        return hud;
    }

    public static void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        GameplayFeedbackHud hud = EnsureSceneHud();
        if (hud == null)
        {
            return;
        }

        hud.Show(message);
    }

    private static void ConfigureRoot(RectTransform rectTransform, Image backgroundImage)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 72f);
        rectTransform.sizeDelta = new Vector2(480f, 46f);

        backgroundImage.color = new Color(0.04f, 0.035f, 0.025f, 0.86f);
        backgroundImage.raycastTarget = false;
    }

    private static TMP_Text CreateMessageText(Transform parent)
    {
        GameObject textObject = new GameObject("Text_GameplayFeedback", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 5f);
        textRect.offsetMax = new Vector2(-12f, -5f);

        TMP_Text messageText = textObject.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 20f;
        messageText.fontStyle = FontStyles.Bold;
        messageText.color = new Color(1f, 0.88f, 0.45f, 1f);
        messageText.raycastTarget = false;
        messageText.textWrappingMode = TextWrappingModes.Normal;
        return messageText;
    }

    private void Awake()
    {
        _instance = this;
        SetVisible(false);
    }

    private void Update()
    {
        if (Time.unscaledTime >= _hideAtTime)
        {
            SetVisible(false);
        }
    }

    private void Show(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }

        _hideAtTime = Time.unscaledTime + Mathf.Max(0.1f, _visibleSeconds);
        SetVisible(true);
    }

    private void SetVisible(bool isVisible)
    {
        if (_messageText != null)
        {
            _messageText.enabled = isVisible;
        }

        if (_backgroundImage != null)
        {
            _backgroundImage.enabled = isVisible;
        }
    }
}
