using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 3일차 보스 등장 알림과 보스 HP를 보여주는 HUD입니다.
// 보스 체력 계산과 클리어 판정은 BossStatus와 GameFlowController가 담당합니다.
public sealed class BossEncounterHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_BossEncounterHud";
    private const string BossWarningText = "Frozen Golem 등장";
    private const string BossHpPrefix = "Frozen Golem HP ";

    [SerializeField] private CanvasGroup _alertCanvasGroup;
    [SerializeField] private CanvasGroup _hpCanvasGroup;
    [SerializeField] private TMP_Text _alertText;
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private Image _hpFillImage;
    [SerializeField] private float _alertSeconds = 3f;

    private static BossEncounterHud _instance;

    private BossStatus _trackedBossStatus;
    private float _alertUntilTime;
    private float _nextBossSearchTime;

    public static BossEncounterHud EnsureSceneHud()
    {
        if (_instance != null)
        {
            _instance.gameObject.SetActive(true);
            return _instance;
        }

        BossEncounterHud existing = FindFirstObjectByType<BossEncounterHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            _instance = existing;
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

        GameObject rootObject = new GameObject(HudObjectName, typeof(RectTransform));
        rootObject.transform.SetParent(hudRoot, false);
        rootObject.transform.SetAsLastSibling();

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(620f, 132f);
        rootRect.anchoredPosition = new Vector2(0f, -86f);

        BossEncounterHud hud = rootObject.AddComponent<BossEncounterHud>();
        hud.CreateAlertPanel(rootObject.transform);
        hud.CreateHpPanel(rootObject.transform);
        hud.SetAlertVisible(false);
        hud.SetHpVisible(false);
        _instance = hud;
        return hud;
    }

    private void OnEnable()
    {
        _instance = this;
        GameFlowController.BossApproachStartedGlobal += HandleBossApproachStarted;
        BossStatus.BossDamagedGlobal += HandleBossDamaged;
        BossStatus.BossDiedGlobal += HandleBossDied;
    }

    private void OnDisable()
    {
        GameFlowController.BossApproachStartedGlobal -= HandleBossApproachStarted;
        BossStatus.BossDamagedGlobal -= HandleBossDamaged;
        BossStatus.BossDiedGlobal -= HandleBossDied;

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Update()
    {
        RefreshTrackedBoss();
        RefreshHpPanel();
        RefreshAlert();
    }

    private void ShowWarning()
    {
        if (_alertText != null)
        {
            _alertText.text = BossWarningText;
        }

        _alertUntilTime = Time.unscaledTime + Mathf.Max(0.1f, _alertSeconds);
        SetAlertVisible(true);
    }

    private void HandleBossApproachStarted()
    {
        ShowWarning();
    }

    private void RefreshTrackedBoss()
    {
        if (IsBossAlive(_trackedBossStatus))
        {
            return;
        }

        if (Time.unscaledTime < _nextBossSearchTime)
        {
            return;
        }

        _nextBossSearchTime = Time.unscaledTime + 0.5f;
        _trackedBossStatus = FindActiveBoss();
    }

    private BossStatus FindActiveBoss()
    {
        BossStatus[] bossStatuses = FindObjectsByType<BossStatus>(FindObjectsSortMode.None);
        for (int i = 0; i < bossStatuses.Length; i++)
        {
            BossStatus bossStatus = bossStatuses[i];
            if (IsBossAlive(bossStatus))
            {
                return bossStatus;
            }
        }

        return null;
    }

    // HP 추적을 계속해도 되는 살아있는 보스인지 확인합니다.
    private bool IsBossAlive(BossStatus bossStatus)
    {
        if (bossStatus == null)
        {
            return false;
        }

        if (bossStatus.gameObject.activeInHierarchy == false)
        {
            return false;
        }

        return bossStatus.IsDead == false;
    }

    private void RefreshHpPanel()
    {
        if (_trackedBossStatus == null)
        {
            SetHpVisible(false);
            return;
        }

        float maxHealth = Mathf.Max(1f, _trackedBossStatus.GetMaxHealth());
        float currentHealth = Mathf.Clamp(_trackedBossStatus.GetCurrentHealth(), 0f, maxHealth);
        float healthRate = currentHealth / maxHealth;

        if (_hpText != null)
        {
            _hpText.text = BossHpPrefix + Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
        }

        if (_hpFillImage != null)
        {
            _hpFillImage.fillAmount = healthRate;
        }

        SetHpVisible(true);
    }

    private void RefreshAlert()
    {
        if (_alertCanvasGroup == null)
        {
            return;
        }

        if (Time.unscaledTime >= _alertUntilTime)
        {
            SetAlertVisible(false);
        }
    }

    private void HandleBossDamaged(BossStatus bossStatus)
    {
        if (bossStatus == null)
        {
            return;
        }

        _trackedBossStatus = bossStatus;
        RefreshHpPanel();
    }

    private void HandleBossDied(BossStatus bossStatus)
    {
        if (_trackedBossStatus == bossStatus)
        {
            _trackedBossStatus = null;
        }

        SetHpVisible(false);
    }

    private void CreateAlertPanel(Transform root)
    {
        GameObject panelObject = new GameObject("Panel_BossAlert", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panelObject.transform.SetParent(root, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(520f, 48f);
        panelRect.anchoredPosition = Vector2.zero;

        Image background = panelObject.GetComponent<Image>();
        background.raycastTarget = false;
        background.color = new Color(0.08f, 0.01f, 0.01f, 0.88f);

        _alertCanvasGroup = panelObject.GetComponent<CanvasGroup>();
        _alertCanvasGroup.interactable = false;
        _alertCanvasGroup.blocksRaycasts = false;

        _alertText = CreateText(
            panelObject.transform,
            "Text_BossAlert",
            BossWarningText,
            28f,
            Vector2.zero,
            new Vector2(490f, 42f));
    }

    private void CreateHpPanel(Transform root)
    {
        GameObject panelObject = new GameObject("Panel_BossHp", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panelObject.transform.SetParent(root, false);

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.sizeDelta = new Vector2(540f, 70f);
        panelRect.anchoredPosition = new Vector2(0f, -56f);

        Image background = panelObject.GetComponent<Image>();
        background.raycastTarget = false;
        background.color = new Color(0.02f, 0.02f, 0.025f, 0.84f);

        _hpCanvasGroup = panelObject.GetComponent<CanvasGroup>();
        _hpCanvasGroup.interactable = false;
        _hpCanvasGroup.blocksRaycasts = false;

        _hpText = CreateText(
            panelObject.transform,
            "Text_BossHp",
            BossHpPrefix + "0 / 0",
            18f,
            new Vector2(0f, 16f),
            new Vector2(500f, 26f));
        _hpFillImage = CreateHpFill(panelObject.transform);
    }

    private TMP_Text CreateText(
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
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        tmpText.raycastTarget = false;
        return tmpText;
    }

    private Image CreateHpFill(Transform parent)
    {
        GameObject backgroundObject = new GameObject("Image_BossHpBarBackground", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(parent, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0f);
        backgroundRect.pivot = new Vector2(0.5f, 0f);
        backgroundRect.sizeDelta = new Vector2(500f, 16f);
        backgroundRect.anchoredPosition = new Vector2(0f, 12f);

        Image background = backgroundObject.GetComponent<Image>();
        background.raycastTarget = false;
        background.color = new Color(0.12f, 0.02f, 0.02f, 0.95f);

        GameObject fillObject = new GameObject("Image_BossHpBarFill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.raycastTarget = false;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        fillImage.color = new Color(0.85f, 0.04f, 0.03f, 1f);
        return fillImage;
    }

    private void SetAlertVisible(bool isVisible)
    {
        if (_alertCanvasGroup != null)
        {
            _alertCanvasGroup.alpha = isVisible ? 1f : 0f;
        }
    }

    private void SetHpVisible(bool isVisible)
    {
        if (_hpCanvasGroup != null)
        {
            _hpCanvasGroup.alpha = isVisible ? 1f : 0f;
        }
    }
}
