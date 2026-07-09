using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 다운 상태를 명확히 알려주는 HUD입니다.
// 실제 사망/부활 판정은 PlayerStatus가 담당하고, 이 클래스는 안내 표시만 담당합니다.
public sealed class PlayerDownHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_PlayerDownHud";

    private const string TitleText = "플레이어 다운";
    private const string BodyText = "CinderHeart가 버티면 다음 보상에 부활 선택지가 등장합니다.";

    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;

    private bool _isVisible;

    public static PlayerDownHud EnsureSceneHud()
    {
        PlayerDownHud existing = FindFirstObjectByType<PlayerDownHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
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

        GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        rootObject.transform.SetParent(hudRoot, false);
        rootObject.transform.SetAsLastSibling();

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.18f);
        rootRect.anchorMax = new Vector2(0.5f, 0.18f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(560f, 112f);
        rootRect.anchoredPosition = Vector2.zero;

        Image background = rootObject.GetComponent<Image>();
        background.raycastTarget = false;
        background.color = new Color(0.02f, 0.02f, 0.025f, 0.84f);

        CanvasGroup canvasGroup = rootObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        TMP_Text titleText = CreateText(
            rootObject.transform,
            "Text_PlayerDownTitle",
            TitleText,
            30f,
            new Vector2(0f, 22f),
            new Vector2(520f, 42f));

        TMP_Text bodyText = CreateText(
            rootObject.transform,
            "Text_PlayerDownBody",
            BodyText,
            18f,
            new Vector2(0f, -24f),
            new Vector2(520f, 42f));

        PlayerDownHud hud = rootObject.AddComponent<PlayerDownHud>();
        hud._canvasGroup = canvasGroup;
        hud._titleText = titleText;
        hud._bodyText = bodyText;
        hud.RefreshText();
        hud.SetVisible(false);
        return hud;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string objectName,
        string text,
        float fontSize,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.textWrappingMode = TextWrappingModes.Normal;
        tmpText.raycastTarget = false;
        return tmpText;
    }

    private void OnEnable()
    {
        PlayerStatus.PlayerDiedGlobal += HandlePlayerDied;
        RefreshText();
    }

    private void OnDisable()
    {
        PlayerStatus.PlayerDiedGlobal -= HandlePlayerDied;
    }

    private void Update()
    {
        if (_isVisible == false)
        {
            return;
        }

        PlayerStatus playerStatus = FindFirstObjectByType<PlayerStatus>();
        if (playerStatus == null || playerStatus.IsDead() == false)
        {
            SetVisible(false);
        }
    }

    private void HandlePlayerDied()
    {
        RefreshText();
        SetVisible(true);
    }

    private void RefreshText()
    {
        if (_titleText != null)
        {
            _titleText.text = TitleText;
        }

        if (_bodyText != null)
        {
            _bodyText.text = BodyText;
        }
    }

    private void SetVisible(bool isVisible)
    {
        if (_canvasGroup == null)
        {
            return;
        }

        _isVisible = isVisible;
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
