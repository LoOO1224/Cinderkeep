using UnityEngine;
using UnityEngine.UI;

// 1인칭 화면 중앙의 기본 조준점을 표시합니다.
// 평상시에는 흰색 십자이고, E 상호작용이나 좌클릭 입력 순간에는 짧게 빨간색으로 반응합니다.
public sealed class CrosshairFeedbackHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_CrosshairFeedbackHud_5_48";

    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _feedbackColor = new Color(1f, 0.12f, 0.08f, 1f);
    [SerializeField] private float _feedbackSeconds = 0.12f;

    private readonly Image[] _crosshairImages = new Image[2];
    private float _feedbackUntilTime;

    public static CrosshairFeedbackHud EnsureSceneHud()
    {
        CrosshairFeedbackHud existing = FindFirstObjectByType<CrosshairFeedbackHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[CrosshairFeedbackHud] 씬에 배치된 HUD가 없어 런타임 생성을 건너뜁니다.");
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

        GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform));
        rootObject.transform.SetParent(hudRoot, false);
        ConfigureRoot(rootObject.GetComponent<RectTransform>());

        CrosshairFeedbackHud hud = rootObject.AddComponent<CrosshairFeedbackHud>();
        hud.CreateCrosshair(rootObject.transform);
        hud.SetColor(hud._normalColor);
        return hud;
    }

    private static void ConfigureRoot(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(34f, 34f);
    }

    private void Update()
    {
        bool shouldShow = CinderkeepInput.IsGameplayInputBlocked() == false;
        SetVisible(shouldShow);
        if (shouldShow == false)
        {
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(KeyCode.E) || CinderkeepInput.WasLeftMousePressedThisFrame())
        {
            _feedbackUntilTime = Time.unscaledTime + Mathf.Max(0.01f, _feedbackSeconds);
        }

        SetColor(Time.unscaledTime < _feedbackUntilTime ? _feedbackColor : _normalColor);
    }

    private void CreateCrosshair(Transform root)
    {
        _crosshairImages[0] = CreateLine(root, "Image_CrosshairHorizontal", new Vector2(18f, 2f));
        _crosshairImages[1] = CreateLine(root, "Image_CrosshairVertical", new Vector2(2f, 18f));
    }

    private Image CreateLine(Transform root, string objectName, Vector2 size)
    {
        GameObject lineObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        lineObject.transform.SetParent(root, false);

        RectTransform rectTransform = lineObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;

        Image image = lineObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private void SetVisible(bool isVisible)
    {
        for (int i = 0; i < _crosshairImages.Length; i++)
        {
            if (_crosshairImages[i] != null)
            {
                _crosshairImages[i].enabled = isVisible;
            }
        }
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < _crosshairImages.Length; i++)
        {
            if (_crosshairImages[i] != null)
            {
                _crosshairImages[i].color = color;
            }
        }
    }
}
