using Cinderkeep.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 화면 중앙 근처에 현재 바라보는 대상의 가능한 행동을 짧게 표시합니다.
// 실제 상호작용은 PlayerInteraction, PlayerBuild, PlayerToolUse가 처리하고 이 HUD는 안내만 담당합니다.
public sealed class InteractionPromptHud : MonoBehaviour
{
    private const string CanvasName = "Canvas_GameHUD";
    private const string HudRootName = "Panel_HUDRoot";
    private const string HudObjectName = "Panel_InteractionPromptHud";
    private const float PromptDistance = 5.5f;

    [SerializeField] private TMP_Text _promptText;
    [SerializeField] private Image _backgroundImage;

    private Transform _cameraTransform;
    private PlayerToolController _toolController;

    public static InteractionPromptHud EnsureSceneHud()
    {
        InteractionPromptHud existing = FindFirstObjectByType<InteractionPromptHud>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
        }

        if (HudRuntimeCreationPolicy.IsRuntimeCreationEnabled == false)
        {
            global::CinderkeepLog.Verbose("[InteractionPromptHud] 씬에 배치된 HUD가 없어 런타임 생성을 건너뜁니다.");
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

        InteractionPromptHud promptHud = rootObject.AddComponent<InteractionPromptHud>();
        promptHud._backgroundImage = rootObject.GetComponent<Image>();
        promptHud._promptText = CreatePromptText(rootObject.transform);
        promptHud.SetVisible(false);
        return promptHud;
    }

    private static void ConfigureRoot(RectTransform rectTransform, Image backgroundImage)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, -88f);
        rectTransform.sizeDelta = new Vector2(420f, 42f);

        backgroundImage.color = new Color(0.02f, 0.035f, 0.05f, 0.78f);
        backgroundImage.raycastTarget = false;
    }

    private static TMP_Text CreatePromptText(Transform parent)
    {
        GameObject textObject = new GameObject("Text_InteractionPrompt", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);

        TMP_Text promptText = textObject.AddComponent<TextMeshProUGUI>();
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.fontSize = 18f;
        promptText.fontStyle = FontStyles.Bold;
        promptText.color = Color.white;
        promptText.raycastTarget = false;
        promptText.textWrappingMode = TextWrappingModes.NoWrap;
        return promptText;
    }

    private void Update()
    {
        RefreshPrompt();
    }

    private void RefreshPrompt()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            SetVisible(false);
            return;
        }

        ConnectRuntimeReferences();
        if (_cameraTransform == null)
        {
            SetVisible(false);
            return;
        }

        RaycastHit hitInfo;
        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        if (Physics.Raycast(ray, out hitInfo, PromptDistance, ~0, QueryTriggerInteraction.Collide) == false)
        {
            SetVisible(false);
            return;
        }

        string promptText = ResolvePrompt(hitInfo.collider);
        if (string.IsNullOrEmpty(promptText))
        {
            SetVisible(false);
            return;
        }

        SetText(promptText);
        SetVisible(true);
    }

    private string ResolvePrompt(Collider targetCollider)
    {
        if (targetCollider == null)
        {
            return string.Empty;
        }

        HandStonePickup handStonePickup = targetCollider.GetComponentInParent<HandStonePickup>();
        if (handStonePickup != null)
        {
            return "E: 손돌 줍기";
        }

        CraftingStationInteractable stationInteractable = targetCollider.GetComponentInParent<CraftingStationInteractable>();
        if (stationInteractable != null)
        {
            FurnaceStation furnaceStation = stationInteractable.GetComponent<FurnaceStation>();
            if (furnaceStation != null)
            {
                return "E: 용광로 열기";
            }

            CraftingStation craftingStation = stationInteractable.GetComponent<CraftingStation>();
            return "E: " + ResolveCraftingStationName(craftingStation) + " 열기";
        }

        BuildingSpot buildingSpot = targetCollider.GetComponentInParent<BuildingSpot>();
        if (buildingSpot != null)
        {
            return "E 길게: " + ResolveBuildingName(buildingSpot) + " 건축";
        }

        ResourceNode resourceNode = targetCollider.GetComponentInParent<ResourceNode>();
        if (resourceNode != null && CanShowGatherPrompt(resourceNode))
        {
            return "좌클릭: " + ResolveResourceName(resourceNode) + " 채집";
        }

        return string.Empty;
    }

    private bool CanShowGatherPrompt(ResourceNode resourceNode)
    {
        if (resourceNode == null)
        {
            return false;
        }

        return _toolController != null && _toolController.CurrentToolType != GatherToolType.None;
    }

    private string ResolveCraftingStationName(CraftingStation craftingStation)
    {
        if (craftingStation == null)
        {
            return "제작대";
        }

        string stationType = craftingStation.StationType;
        if (string.IsNullOrEmpty(stationType))
        {
            return "제작대";
        }

        if (stationType.ToLowerInvariant().Contains("furnace"))
        {
            return "용광로";
        }

        return "제작대";
    }

    private string ResolveBuildingName(BuildingSpot buildingSpot)
    {
        if (buildingSpot == null)
        {
            return "건축물";
        }

        if (GameManager.Inst != null && GameManager.Inst.GetGameDataManager() != null)
        {
            BuildingData buildingData = GameManager.Inst.GetGameDataManager().GetBuilding(buildingSpot.BuildingDataId);
            if (buildingData != null && string.IsNullOrEmpty(buildingData.DisplayName) == false)
            {
                return buildingData.DisplayName;
            }
        }

        return string.IsNullOrEmpty(buildingSpot.BuildingDataId) ? "건축물" : buildingSpot.BuildingDataId;
    }

    private string ResolveResourceName(ResourceNode resourceNode)
    {
        if (resourceNode == null)
        {
            return "자원";
        }

        string resourceId = resourceNode.ResourceId;
        if (string.IsNullOrEmpty(resourceId))
        {
            return "자원";
        }

        return UiItemDisplayFormatter.GetItemName(resourceId, InventoryItemType.Resource);
    }

    private void ConnectRuntimeReferences()
    {
        if (_cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
        }

        if (_toolController == null)
        {
            _toolController = FindFirstObjectByType<PlayerToolController>();
        }
    }

    private void SetText(string promptText)
    {
        if (_promptText != null)
        {
            _promptText.text = promptText;
        }
    }

    private void SetVisible(bool isVisible)
    {
        if (_promptText != null)
        {
            _promptText.enabled = isVisible;
        }

        if (_backgroundImage != null)
        {
            _backgroundImage.enabled = isVisible;
        }
    }
}
