using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD 상단에 한 줄 목표를 표시하는 초반 튜토리얼 가이드입니다.
// 목표를 달성하면 짧게 취소선으로 완료 표시한 뒤 다음 목표로 넘어갑니다.
public sealed class HudTutorialGuide : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string GuideBackgroundName = "Image_GameGuideBackground";
    private const string GuideTextName = "Text_GameGuide";
    private const string GuideIconName = "Image_GameGuideIcon";

    private enum TutorialStep
    {
        PickHandStone,
        EquipHandStone,
        GatherWithHandStone,
        OpenCraftingLoop,
        Done
    }

    [Header("Connected UI")]
    [SerializeField] private TMP_Text _guideText;
    [SerializeField] private Image _guideIconImage;
    [SerializeField] private Image _guideBackgroundImage;

    [Header("Optional Sprites")]
    [Tooltip("GUI Pro 또는 임시 튜토리얼 기본 아이콘입니다. 비어 있으면 색상 박스로 표시합니다.")]
    [SerializeField] private Sprite _defaultGuideSprite;
    [Tooltip("목표 달성 순간에 표시할 완료 아이콘입니다. 비어 있으면 색상만 변경합니다.")]
    [SerializeField] private Sprite _completedGuideSprite;

    [Header("Colors")]
    [SerializeField] private Color _activeTextColor = Color.white;
    [SerializeField] private Color _completedTextColor = new Color(0.45f, 1f, 0.55f, 1f);
    [SerializeField] private Color _iconColor = new Color(1f, 0.9f, 0.42f, 0.95f);
    [SerializeField] private Color _completedIconColor = new Color(0.45f, 1f, 0.55f, 0.95f);

    [Header("Timing")]
    [SerializeField] private float _completedDisplaySeconds = 0.7f;

    private TutorialStep _currentStep = TutorialStep.PickHandStone;
    private TutorialStep _pendingNextStep = TutorialStep.PickHandStone;
    private PlayerModel _playerModel;
    private PlayerInventoryModel _inventoryModel;
    private PlayerToolController _playerToolController;
    private int _resourceBaseline;
    private float _completedUntilTime;
    private bool _isShowingCompletedStep;

    private void Start()
    {
        ConnectRuntimeReferences();
        SetResourceBaseline();
        RefreshGuideText();
    }

    private void Update()
    {
        ConnectRuntimeReferences();
        UpdateCompletedTransition();

        if (_isShowingCompletedStep)
        {
            return;
        }

        if (IsCurrentStepCompleted())
        {
            CompleteCurrentStep();
        }
    }

    public static HudTutorialGuide EnsureSceneGuide()
    {
        HudTutorialGuide existingGuide = FindFirstObjectByType<HudTutorialGuide>(FindObjectsInactive.Include);
        if (existingGuide != null)
        {
            existingGuide.gameObject.SetActive(true);
            return existingGuide;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[HudTutorialGuide] 씬에 배치된 가이드가 없어 런타임 생성을 건너뜁니다.");
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

        Image guideBackground = FindOrCreateGuideBackground(hudRoot);
        TMP_Text guideText = FindOrCreateGuideText(hudRoot);
        Image guideIcon = FindOrCreateGuideIcon(hudRoot);
        HudTutorialGuide guide = guideText.GetComponent<HudTutorialGuide>();
        if (guide == null)
        {
            guide = guideText.gameObject.AddComponent<HudTutorialGuide>();
        }

        guide.SetReferences(guideText, guideIcon, guideBackground);
        return guide;
    }

    public void SetReferences(TMP_Text guideText, Image guideIconImage)
    {
        SetReferences(guideText, guideIconImage, _guideBackgroundImage);
    }

    public void SetReferences(TMP_Text guideText, Image guideIconImage, Image guideBackgroundImage)
    {
        _guideText = guideText;
        _guideIconImage = guideIconImage;
        _guideBackgroundImage = guideBackgroundImage;
        RefreshBackground();
        RefreshGuideText();
    }

    private static Image FindOrCreateGuideBackground(Transform hudRoot)
    {
        Transform backgroundTransform = hudRoot.Find(GuideBackgroundName);
        GameObject backgroundObject = backgroundTransform == null
            ? new GameObject(GuideBackgroundName, typeof(RectTransform), typeof(Image))
            : backgroundTransform.gameObject;

        backgroundObject.transform.SetParent(hudRoot, false);
        backgroundObject.transform.SetAsFirstSibling();
        Image backgroundImage = backgroundObject.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = backgroundObject.AddComponent<Image>();
        }

        RectTransform rectTransform = backgroundImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(16f, -12f);
        rectTransform.sizeDelta = new Vector2(840f, 48f);

        backgroundImage.color = new Color(0.04f, 0.06f, 0.08f, 0.72f);
        backgroundImage.raycastTarget = false;
        return backgroundImage;
    }

    private static TMP_Text FindOrCreateGuideText(Transform hudRoot)
    {
        Transform guideTextTransform = hudRoot.Find(GuideTextName);
        if (guideTextTransform != null)
        {
            TMP_Text existingText = guideTextTransform.GetComponent<TMP_Text>();
            if (existingText != null)
            {
                return existingText;
            }
        }

        GameObject guideTextObject = guideTextTransform == null
            ? new GameObject(GuideTextName, typeof(RectTransform))
            : guideTextTransform.gameObject;

        guideTextObject.transform.SetParent(hudRoot, false);
        TMP_Text guideText = guideTextObject.GetComponent<TMP_Text>();
        if (guideText == null)
        {
            guideText = guideTextObject.AddComponent<TextMeshProUGUI>();
        }

        RectTransform rectTransform = guideText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(72f, -20f);
        rectTransform.sizeDelta = new Vector2(760f, 34f);

        guideText.fontSize = 20f;
        guideText.alignment = TextAlignmentOptions.Left;
        guideText.color = Color.white;
        guideText.raycastTarget = false;
        guideText.richText = true;
        return guideText;
    }

    private static Image FindOrCreateGuideIcon(Transform hudRoot)
    {
        Transform guideIconTransform = hudRoot.Find(GuideIconName);
        GameObject guideIconObject = guideIconTransform == null
            ? new GameObject(GuideIconName, typeof(RectTransform), typeof(Image))
            : guideIconTransform.gameObject;

        guideIconObject.transform.SetParent(hudRoot, false);
        Image guideIcon = guideIconObject.GetComponent<Image>();
        if (guideIcon == null)
        {
            guideIcon = guideIconObject.AddComponent<Image>();
        }

        RectTransform rectTransform = guideIcon.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -18f);
        rectTransform.sizeDelta = new Vector2(34f, 34f);

        guideIcon.color = new Color(1f, 0.9f, 0.42f, 0.95f);
        guideIcon.raycastTarget = false;
        return guideIcon;
    }

    private void ConnectRuntimeReferences()
    {
        if (GameManager.Inst != null)
        {
            _playerModel = GameManager.Inst.PlayerModel;
            _inventoryModel = GameManager.Inst.PlayerInventoryModel;
        }

        if (_playerToolController == null)
        {
            _playerToolController = FindFirstObjectByType<PlayerToolController>();
        }
    }

    private void UpdateCompletedTransition()
    {
        if (_isShowingCompletedStep == false || Time.unscaledTime < _completedUntilTime)
        {
            return;
        }

        _currentStep = _pendingNextStep;
        _isShowingCompletedStep = false;
        SetResourceBaseline();
        RefreshGuideText();
    }

    private bool IsCurrentStepCompleted()
    {
        switch (_currentStep)
        {
            case TutorialStep.PickHandStone:
                return HasHandStoneInQuickSlot() || IsHandStoneEquipped();
            case TutorialStep.EquipHandStone:
                return IsHandStoneEquipped();
            case TutorialStep.GatherWithHandStone:
                return GetBasicResourceTotal() > _resourceBaseline;
            case TutorialStep.OpenCraftingLoop:
                return true;
        }

        return false;
    }

    private void CompleteCurrentStep()
    {
        _pendingNextStep = GetNextStep(_currentStep);
        _isShowingCompletedStep = true;
        _completedUntilTime = Time.unscaledTime + _completedDisplaySeconds;
        RefreshCompletedGuideText();
    }

    private TutorialStep GetNextStep(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.PickHandStone:
                return TutorialStep.EquipHandStone;
            case TutorialStep.EquipHandStone:
                return TutorialStep.GatherWithHandStone;
            case TutorialStep.GatherWithHandStone:
                return TutorialStep.OpenCraftingLoop;
        }

        return TutorialStep.Done;
    }

    private bool HasHandStoneInQuickSlot()
    {
        if (_inventoryModel == null)
        {
            return false;
        }

        for (int i = 0; i < PlayerInventoryModel.QuickSlotCount; i++)
        {
            InventoryItemModel itemModel = _inventoryModel.GetQuickSlotItem(i);
            if (itemModel != null && itemModel.ItemId == PlayerToolController.HandStoneToolDataId)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsHandStoneEquipped()
    {
        return _playerToolController != null
            && _playerToolController.CurrentToolDataId == PlayerToolController.HandStoneToolDataId;
    }

    private int GetBasicResourceTotal()
    {
        if (_playerModel == null)
        {
            return 0;
        }

        return _playerModel.Wood + _playerModel.Stone;
    }

    private void SetResourceBaseline()
    {
        _resourceBaseline = GetBasicResourceTotal();
    }

    private void RefreshGuideText()
    {
        if (_guideText == null)
        {
            return;
        }

        _guideText.richText = true;
        _guideText.color = _activeTextColor;
        _guideText.text = GetStepText(_currentStep);
        RefreshBackground();
        RefreshIcon(false);
    }

    private void RefreshCompletedGuideText()
    {
        if (_guideText == null)
        {
            return;
        }

        _guideText.richText = true;
        _guideText.color = _completedTextColor;
        _guideText.text = "<s>" + GetStepText(_currentStep) + "</s>";
        RefreshBackground();
        RefreshIcon(true);
    }

    private void RefreshBackground()
    {
        if (_guideBackgroundImage == null)
        {
            return;
        }

        _guideBackgroundImage.color = new Color(0.04f, 0.06f, 0.08f, 0.72f);
        _guideBackgroundImage.raycastTarget = false;
    }

    private void RefreshIcon(bool isCompleted)
    {
        if (_guideIconImage == null)
        {
            return;
        }

        _guideIconImage.sprite = isCompleted ? _completedGuideSprite : _defaultGuideSprite;
        _guideIconImage.color = isCompleted ? _completedIconColor : _iconColor;
    }

    private string GetStepText(TutorialStep step)
    {
        if (step == TutorialStep.PickHandStone)
        {
            return "E키로 바닥의 손돌을 주우세요";
        }

        if (step == TutorialStep.EquipHandStone)
        {
            return "1~7번으로 손돌을 장착하세요";
        }

        if (step == TutorialStep.GatherWithHandStone)
        {
            return "좌클릭으로 나무와 돌을 채집하세요";
        }

        if (step == TutorialStep.OpenCraftingLoop)
        {
            return "자원을 모아 제작대와 건축 지점을 사용하세요";
        }

        switch (step)
        {
            case TutorialStep.PickHandStone:
                return "E키로 바닥의 손돌을 주우세요";
            case TutorialStep.EquipHandStone:
                return "1~7번으로 손돌을 장착하세요";
            case TutorialStep.GatherWithHandStone:
                return "좌클릭으로 나무나 기본 돌을 채집하세요";
            case TutorialStep.OpenCraftingLoop:
                return "자원을 모아 제작대와 건축 지점을 사용하세요";
        }

        return string.Empty;
    }
}
