using UnityEngine;
using UnityEngine.UI;

// CinderHeart가 피해를 받을 때 화면에 짧은 붉은 점멸을 띄워 위험 상태를 즉시 전달합니다.
public sealed class CinderHeartDamageFlashHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_CinderHeartDamageFlashHud";

    [SerializeField] private Color _flashColor = new Color(1f, 0.04f, 0.02f, 0.28f);
    [SerializeField] private float _fadeSeconds = 0.28f;

    private Image _flashImage;
    private float _flashAlpha;

    public static CinderHeartDamageFlashHud EnsureSceneHud()
    {
        CinderHeartDamageFlashHud existing = FindFirstObjectByType<CinderHeartDamageFlashHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            existing.transform.SetAsLastSibling();
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

        GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform), typeof(Image));
        rootObject.transform.SetParent(hudRoot, false);
        rootObject.transform.SetAsLastSibling();

        RectTransform rectTransform = rootObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        CinderHeartDamageFlashHud hud = rootObject.AddComponent<CinderHeartDamageFlashHud>();
        hud._flashImage = rootObject.GetComponent<Image>();
        hud._flashImage.raycastTarget = false;
        hud.SetAlpha(0f);
        return hud;
    }

    private void OnEnable()
    {
        CinderHeart.CinderHeartDamagedGlobal += HandleCinderHeartDamaged;
    }

    private void OnDisable()
    {
        CinderHeart.CinderHeartDamagedGlobal -= HandleCinderHeartDamaged;
    }

    private void Awake()
    {
        if (_flashImage == null)
        {
            _flashImage = GetComponent<Image>();
        }

        SetAlpha(0f);
    }

    private void Update()
    {
        if (_flashAlpha <= 0f)
        {
            return;
        }

        float fadeSpeed = 1f / Mathf.Max(0.05f, _fadeSeconds);
        _flashAlpha = Mathf.Max(0f, _flashAlpha - (Time.unscaledDeltaTime * fadeSpeed));
        SetAlpha(_flashAlpha);
    }

    private void HandleCinderHeartDamaged(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        _flashAlpha = 1f;
        SetAlpha(_flashAlpha);
    }

    private void SetAlpha(float alphaRate)
    {
        if (_flashImage == null)
        {
            return;
        }

        Color color = _flashColor;
        color.a *= Mathf.Clamp01(alphaRate);
        _flashImage.color = color;
    }
}
