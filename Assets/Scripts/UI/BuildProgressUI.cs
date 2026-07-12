using TMPro;
using UnityEngine;
using UnityEngine.UI;

// E 홀드 건축 진행도를 HUD에 표시하는 전용 UI입니다.
// 씬 연결이 비어 있어도 Canvas_GameHUD 아래에 최소 진행바를 자동 생성합니다.
public sealed class BuildProgressUI : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string ProgressRootName = "Panel_BuildProgress";

    [Header("Root")]
    [SerializeField] private GameObject _rootObject;

    [Header("Text UI")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _progressText;

    [Header("Progress UI")]
    [SerializeField] private Image _progressFillImage;

    private float _durationSeconds = 1f;

    private void Awake()
    {
        Close();
    }

    public static BuildProgressUI EnsureSceneUI()
    {
        BuildProgressUI existing = FindFirstObjectByType<BuildProgressUI>(FindObjectsInactive.Include);
        if (existing != null)
        {
            return existing;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[BuildProgressUI] 씬에 배치된 UI가 없어 런타임 생성을 건너뜁니다.");
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

        GameObject rootObject = CreateRoot(hudRoot);
        BuildProgressUI progressUI = rootObject.AddComponent<BuildProgressUI>();
        progressUI.SetReferences(
            rootObject,
            rootObject.transform.Find("Text_BuildTitle").GetComponent<TMP_Text>(),
            rootObject.transform.Find("Text_BuildProgress").GetComponent<TMP_Text>(),
            rootObject.transform.Find("Image_BuildProgressBar/Image_BuildProgressFill").GetComponent<Image>());
        progressUI.Close();
        return progressUI;
    }

    public void SetReferences(GameObject rootObject, TMP_Text titleText, TMP_Text progressText, Image progressFillImage)
    {
        _rootObject = rootObject;
        _titleText = titleText;
        _progressText = progressText;
        _progressFillImage = progressFillImage;
    }

    public void Open(string buildingName, float durationSeconds)
    {
        _durationSeconds = Mathf.Max(0.1f, durationSeconds);
        SetVisible(true);
        RefreshTitle(buildingName);
        SetProgress(0f);
    }

    public void SetProgress(float progress01)
    {
        float progress = Mathf.Clamp01(progress01);
        if (_progressFillImage != null)
        {
            _progressFillImage.fillAmount = progress;
        }

        if (_progressText == null)
        {
            return;
        }

        float remainSeconds = Mathf.Max(0f, _durationSeconds * (1f - progress));
        _progressText.text = "건축 진행 " + Mathf.RoundToInt(progress * 100f) + "% / 남은 시간 " + remainSeconds.ToString("F1") + "초";
    }

    public void Close()
    {
        SetProgress(0f);
        SetVisible(false);
    }

    private static GameObject CreateRoot(Transform hudRoot)
    {
        GameObject rootObject = new GameObject(ProgressRootName, typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(hudRoot, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, -76f);
        rootRect.sizeDelta = new Vector2(360f, 64f);

        Image backgroundImage = rootObject.GetComponent<Image>();
        backgroundImage.color = new Color(0.04f, 0.06f, 0.08f, 0.76f);
        backgroundImage.raycastTarget = false;

        TMP_Text titleText = CreateText(rootObject.transform, "Text_BuildTitle", new Vector2(0f, -8f), 18f);
        titleText.alignment = TextAlignmentOptions.Center;

        TMP_Text progressText = CreateText(rootObject.transform, "Text_BuildProgress", new Vector2(0f, -38f), 14f);
        progressText.alignment = TextAlignmentOptions.Center;

        GameObject barObject = new GameObject("Image_BuildProgressBar", typeof(RectTransform), typeof(Image));
        barObject.transform.SetParent(rootObject.transform, false);
        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0f);
        barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 8f);
        barRect.sizeDelta = new Vector2(320f, 10f);

        Image barBackground = barObject.GetComponent<Image>();
        barBackground.color = new Color(0.12f, 0.14f, 0.16f, 0.9f);
        barBackground.raycastTarget = false;

        GameObject fillObject = new GameObject("Image_BuildProgressFill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(barObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.88f, 0.58f, 0.22f, 0.95f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.raycastTarget = false;

        return rootObject;
    }

    private static TMP_Text CreateText(Transform parent, string objectName, Vector2 anchoredPosition, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = new Vector2(320f, 24f);

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void RefreshTitle(string buildingName)
    {
        if (_titleText == null)
        {
            return;
        }

        _titleText.text = string.IsNullOrEmpty(buildingName)
            ? "건축 진행"
            : buildingName + " 건축 진행";
    }

    private void SetVisible(bool isVisible)
    {
        GameObject targetObject = _rootObject == null ? gameObject : _rootObject;
        if (targetObject == null)
        {
            return;
        }

        targetObject.SetActive(isVisible);
    }
}
