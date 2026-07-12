using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 아침 보상 선택 뒤 30초 정비 시간을 좌측 상단에 표시하는 HUD입니다.
// 보상 선택 중에는 숨기고, 실제 정비 카운트가 흐를 때만 표시합니다.
public sealed class MorningPrepTimerHUD : MonoBehaviour
{
    private const string RootName = "Panel_MorningPrepTimerHUD";
    private const string CanvasName = "Canvas_GameHUD";
    private const string TitleText = "아침 정비 시간";
    private const string TimerPrefix = "다음 낮까지 ";
    private const string HintText = "제작 / 건축 / 장비 정리";

    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GameFlowController _gameFlowController;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _hintText;

    private GameRunModel _gameRunModel;
    private bool _isVisible;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureSceneHud();
    }

    public static MorningPrepTimerHUD EnsureSceneHud()
    {
        MorningPrepTimerHUD existing = FindFirstObjectByType<MorningPrepTimerHUD>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[MorningPrepTimerHUD] 씬에 배치된 HUD가 없어 런타임 생성을 건너뜁니다.");
            return null;
        }

        GameObject gameManagerObject = GameObject.Find("GameManager");
        if (gameManagerObject == null && SceneManager.GetActiveScene().name != "Cinderkeep_Game")
        {
            return null;
        }

        Canvas canvas = FindGameHudCanvas();
        if (canvas == null)
        {
            return null;
        }

        return CreateHud(canvas.transform, gameManagerObject);
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        EnsureSceneHud();
    }

    private static Canvas FindGameHudCanvas()
    {
        GameObject canvasObject = GameObject.Find(CanvasName);
        if (canvasObject != null && canvasObject.TryGetComponent(out Canvas namedCanvas))
        {
            return namedCanvas;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null && canvases[i].isRootCanvas)
            {
                return canvases[i];
            }
        }

        return null;
    }

    private static MorningPrepTimerHUD CreateHud(Transform canvasTransform, GameObject gameManagerObject)
    {
        GameObject rootObject = new GameObject(RootName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        rootObject.transform.SetParent(canvasTransform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.sizeDelta = new Vector2(320f, 92f);
        rootRect.anchoredPosition = new Vector2(20f, -22f);

        Image background = rootObject.GetComponent<Image>();
        background.color = new Color(0.02f, 0.06f, 0.09f, 0.82f);

        CanvasGroup canvasGroup = rootObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        TMP_Text titleText = CreateText(
            rootObject.transform,
            "Text_MorningPrepTitle",
            TitleText,
            18f,
            new Vector2(14f, -10f),
            new Vector2(292f, 24f));

        TMP_Text timerText = CreateText(
            rootObject.transform,
            "Text_MorningPrepTimer",
            TimerPrefix + "00:30",
            24f,
            new Vector2(14f, -38f),
            new Vector2(292f, 30f));

        TMP_Text hintText = CreateText(
            rootObject.transform,
            "Text_MorningPrepHint",
            HintText,
            14f,
            new Vector2(14f, -72f),
            new Vector2(292f, 20f));

        MorningPrepTimerHUD timerHud = rootObject.AddComponent<MorningPrepTimerHUD>();
        GameManager gameManager = null;
        if (gameManagerObject != null)
        {
            gameManagerObject.TryGetComponent(out gameManager);
        }

        GameFlowController gameFlowController = null;
        if (gameManager != null)
        {
            gameFlowController = gameManager.GetGameFlowController();
        }

        timerHud.Configure(gameManager, gameFlowController, canvasGroup, titleText, timerText, hintText);
        return timerHud;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string objectName,
        string text,
        float fontSize,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Left;
        tmpText.textWrappingMode = TextWrappingModes.NoWrap;
        return tmpText;
    }

    public void Configure(
        GameManager gameManager,
        GameFlowController gameFlowController,
        CanvasGroup canvasGroup,
        TMP_Text titleText,
        TMP_Text timerText,
        TMP_Text hintText)
    {
        _gameManager = gameManager;
        _gameFlowController = gameFlowController;
        _canvasGroup = canvasGroup;
        _titleText = titleText;
        _timerText = timerText;
        _hintText = hintText;
        ConnectGameRunModel();
        RefreshStaticText();
        SetVisible(false);
    }

    private void Awake()
    {
        ConnectGameRunModel();
        RefreshStaticText();
        SetVisible(false);
    }

    private void Update()
    {
        ConnectGameRunModel();
        RefreshHud();
    }

    private void ConnectGameRunModel()
    {
        if (_gameManager == null)
        {
            _gameManager = GameManager.Inst;
        }

        if (_gameManager == null)
        {
            return;
        }

        _gameRunModel = _gameManager.GameRunModel;
        if (_gameFlowController == null)
        {
            _gameFlowController = _gameManager.GetGameFlowController();
        }
    }

    private void RefreshStaticText()
    {
        if (_titleText != null)
        {
            _titleText.text = TitleText;
        }

        if (_hintText != null)
        {
            _hintText.text = HintText;
        }
    }

    private void RefreshHud()
    {
        bool shouldShow = ShouldShow();
        SetVisible(shouldShow);
        if (shouldShow == false || _timerText == null)
        {
            return;
        }

        _timerText.text = TimerPrefix + FormatTime(_gameRunModel.RemainingTime);
    }

    private bool ShouldShow()
    {
        if (_gameRunModel == null)
        {
            return false;
        }

        if (_gameRunModel.IsPlaying == false)
        {
            return false;
        }

        if (_gameRunModel.Phase != GameRunPhase.MorningReward)
        {
            return false;
        }

        if (_gameFlowController != null && _gameFlowController.IsWaitingForCinderHeartSkillSelection)
        {
            return false;
        }

        return _gameRunModel.RemainingTime > 0f;
    }

    private void SetVisible(bool isVisible)
    {
        if (_canvasGroup == null || _isVisible == isVisible)
        {
            return;
        }

        _isVisible = isVisible;
        _canvasGroup.alpha = isVisible ? 1f : 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private string FormatTime(float time)
    {
        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, time));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
