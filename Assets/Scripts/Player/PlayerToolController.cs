using Cinderkeep.Gameplay;
using UnityEngine;
using System;

// 1인칭 플레이어의 1~7 퀵슬롯 입력과 현재 장착 도구/음식 사용을 처리합니다.
// 도구 수치는 tools.json에서 읽고, 비도구 아이템은 장비/음식 흐름으로 넘깁니다.
public sealed class PlayerToolController : MonoBehaviour
{
    public const string HandStoneToolDataId = "hand_stone";
    public static event Action<string, float> FoodEatenGlobal;

    private static readonly KeyCode[] QuickSlotKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7
    };

    [Header("Tool Keys")]
    [Tooltip("도끼를 장착하는 입력 키입니다.")]
    [SerializeField] private KeyCode _axeKey = KeyCode.Alpha1;
    [Tooltip("곡괭이를 장착하는 입력 키입니다.")]
    [SerializeField] private KeyCode _pickaxeKey = KeyCode.Alpha2;
    [Tooltip("맨손으로 전환하는 입력 키입니다.")]
    [SerializeField] private KeyCode _emptyHandKey = KeyCode.Alpha3;

    [Header("Current Tool")]
    [Tooltip("현재 플레이어가 장착한 도구입니다.")]
    [SerializeField] private GatherToolType _currentToolType = GatherToolType.Axe;

    [Header("Tool Data")]
    [Tooltip("true이면 tools.json의 도구 데이터를 현재 장착 도구 정보로 사용합니다.")]
    [SerializeField] private bool _useToolData = true;
    [Tooltip("도끼 장착 시 사용할 tools.json의 _id입니다.")]
    [SerializeField] private string _axeToolDataId = "stone_axe";
    [Tooltip("곡괭이 장착 시 사용할 tools.json의 _id입니다.")]
    [SerializeField] private string _pickaxeToolDataId = "stone_pickaxe";
    [Tooltip("맨손 장착 시 사용할 tools.json의 _id입니다.")]
    [SerializeField] private string _emptyHandToolDataId = "hand";
    [Tooltip("현재 장착 중인 tools.json의 _id입니다. 플레이 중 디버깅용으로 확인합니다.")]
    [SerializeField] private string _currentToolDataId = "stone_axe";

    public GatherToolType CurrentToolType
    {
        get
        {
            return _currentToolType;
        }
    }

    public string CurrentToolDataId
    {
        get
        {
            return _currentToolDataId;
        }
    }

    public int CurrentToolTier
    {
        get
        {
            ToolData toolData = GetCurrentToolData();
            if (toolData == null)
            {
                return GetFallbackToolTier();
            }

            return toolData.Tier;
        }
    }

    private void Start()
    {
        EquipQuickSlot(0);
    }

    private void Update()
    {
        ReadToolInput();
    }

    public void EquipTool(GatherToolType toolType)
    {
        _currentToolType = toolType;
        SetCurrentToolDataIdByType(toolType);
    }

    public void EquipToolData(string toolDataId)
    {
        if (string.IsNullOrEmpty(toolDataId))
        {
            EquipTool(GatherToolType.None);
            return;
        }

        _currentToolDataId = toolDataId;
        ToolData toolData = GetCurrentToolData();
        _currentToolType = ResolveToolType(toolData, toolDataId);
    }

    public bool HasRequiredTool(GatherToolType requiredToolType)
    {
        if (requiredToolType == GatherToolType.None)
        {
            return true;
        }

        return _currentToolType == requiredToolType;
    }

    public ToolData GetCurrentToolData()
    {
        if (_useToolData == false)
        {
            return null;
        }

        if (GameManager.Inst == null)
        {
            return null;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        if (gameDataManager == null)
        {
            return null;
        }

        return gameDataManager.GetTool(_currentToolDataId);
    }

    private void ReadToolInput()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            return;
        }

        if (TryReadQuickSlotInput())
        {
            return;
        }

        ReadLegacyToolInputIfInventoryIsMissing();
    }

    private bool TryReadQuickSlotInput()
    {
        for (int i = 0; i < QuickSlotKeys.Length; i++)
        {
            if (CinderkeepInput.WasKeyPressedThisFrame(QuickSlotKeys[i]) == false)
            {
                continue;
            }

            EquipQuickSlot(i);
            return true;
        }

        return false;
    }

    private void EquipQuickSlot(int slotIndex)
    {
        PlayerInventoryModel inventoryModel = GetInventoryModel();
        if (inventoryModel == null)
        {
            EquipLegacySlotFallback(slotIndex);
            return;
        }

        InventoryItemModel itemModel = inventoryModel.GetQuickSlotItem(slotIndex);
        if (itemModel == null || itemModel.IsEmpty)
        {
            EquipTool(GatherToolType.None);
            return;
        }

        if (itemModel.ItemType != InventoryItemType.Tool)
        {
            TryUseNonToolQuickSlot(itemModel, slotIndex, inventoryModel);
            EquipTool(GatherToolType.None);
            return;
        }

        EquipToolData(itemModel.ItemId);
    }

    private bool TryUseNonToolQuickSlot(InventoryItemModel itemModel, int slotIndex, PlayerInventoryModel inventoryModel)
    {
        if (itemModel == null || itemModel.IsEmpty || GameManager.Inst == null)
        {
            return false;
        }

        if (itemModel.ItemType == InventoryItemType.Food)
        {
            return TryEatFoodQuickSlot(itemModel, slotIndex, inventoryModel);
        }

        PlayerEquipmentModel equipmentModel = GameManager.Inst.PlayerEquipmentModel;
        if (equipmentModel == null)
        {
            return false;
        }

        if (itemModel.ItemType == InventoryItemType.Weapon)
        {
            return equipmentModel.TryEquipItem(itemModel, EquipmentSlotType.Weapon);
        }

        if (itemModel.ItemType == InventoryItemType.Helmet)
        {
            return equipmentModel.TryEquipItem(itemModel, EquipmentSlotType.Helmet);
        }

        if (itemModel.ItemType == InventoryItemType.Armor)
        {
            return equipmentModel.TryEquipItem(itemModel, EquipmentSlotType.Armor);
        }

        if (itemModel.ItemType == InventoryItemType.Boots)
        {
            return equipmentModel.TryEquipItem(itemModel, EquipmentSlotType.Boots);
        }

        return false;
    }

    private bool TryEatFoodQuickSlot(InventoryItemModel itemModel, int slotIndex, PlayerInventoryModel inventoryModel)
    {
        if (inventoryModel == null || FoodItemIds.IsFoodItem(itemModel.ItemId) == false)
        {
            return false;
        }

        PlayerStatus playerStatus = FindFirstObjectByType<PlayerStatus>();
        if (playerStatus == null)
        {
            return false;
        }

        float restoreAmount = FoodItemIds.GetSatietyRestoreAmount(itemModel.ItemId);
        if (restoreAmount <= 0f)
        {
            return false;
        }

        playerStatus.EatFood(restoreAmount);
        inventoryModel.TryConsumeQuickSlotItem(slotIndex, 1);
        NotifyFoodEaten(itemModel.ItemId, restoreAmount);
        return true;
    }

    private void NotifyFoodEaten(string itemId, float restoreAmount)
    {
        if (FoodEatenGlobal == null || restoreAmount <= 0f)
        {
            return;
        }

        FoodEatenGlobal(itemId, restoreAmount);
    }

    private PlayerInventoryModel GetInventoryModel()
    {
        if (GameManager.Inst == null)
        {
            return null;
        }

        return GameManager.Inst.PlayerInventoryModel;
    }

    private void ReadLegacyToolInputIfInventoryIsMissing()
    {
        if (GetInventoryModel() != null)
        {
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_axeKey))
        {
            EquipTool(GatherToolType.Axe);
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_pickaxeKey))
        {
            EquipTool(GatherToolType.Pickaxe);
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_emptyHandKey))
        {
            EquipTool(GatherToolType.None);
        }
    }

    private void EquipLegacySlotFallback(int slotIndex)
    {
        if (slotIndex == 0)
        {
            EquipTool(GatherToolType.Axe);
            return;
        }

        if (slotIndex == 1)
        {
            EquipTool(GatherToolType.Pickaxe);
            return;
        }

        EquipTool(GatherToolType.None);
    }

    private void SetCurrentToolDataIdByType(GatherToolType toolType)
    {
        if (toolType == GatherToolType.Axe)
        {
            _currentToolDataId = _axeToolDataId;
            return;
        }

        if (toolType == GatherToolType.Pickaxe)
        {
            _currentToolDataId = _pickaxeToolDataId;
            return;
        }

        _currentToolDataId = _emptyHandToolDataId;
    }

    private int GetFallbackToolTier()
    {
        if (_currentToolType == GatherToolType.None)
        {
            return 0;
        }

        return 1;
    }

    private GatherToolType ResolveToolType(ToolData toolData, string toolDataId)
    {
        if (toolData != null)
        {
            GatherToolType parsedToolType;
            if (System.Enum.TryParse(toolData.ToolType, true, out parsedToolType))
            {
                return parsedToolType;
            }
        }

        if (toolDataId == HandStoneToolDataId)
        {
            return GatherToolType.Axe;
        }

        if (toolDataId.Contains("pickaxe"))
        {
            return GatherToolType.Pickaxe;
        }

        if (toolDataId.Contains("axe"))
        {
            return GatherToolType.Axe;
        }

        return GatherToolType.None;
    }
}
