using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using UnityEngine;

// 나무, 돌, 광석처럼 플레이어가 자원을 얻는 오브젝트입니다.
// E 입력으로 줍는 자원과 좌클릭 도구 채집 자원을 모두 처리합니다.
public sealed class ResourceNode : MonoBehaviour, IInteractable
{
    [Header("Resource Data")]
    [Tooltip("harvest_nodes.json의 _id입니다. 비워두면 기존 Inspector 값으로 알맞은 데이터를 찾아봅니다.")]
    [SerializeField] private string _harvestNodeDataId = "";
    [Tooltip("true이면 harvest_nodes.json 값을 우선 적용합니다. 데이터가 없으면 아래 Inspector 값을 fallback으로 사용합니다.")]
    [SerializeField] private bool _useHarvestNodeData = true;
    [Tooltip("지급할 자원 ID입니다. PlayerModel의 ResourceWood, ResourceStone 같은 값과 맞춰 사용합니다.")]
    [SerializeField] private string _resourceId = PlayerModel.ResourceStone;
    [Tooltip("한 번 채집하거나 주웠을 때 지급할 자원 수량입니다.")]
    [SerializeField] private int _amount = 1;
    [Tooltip("채집에 필요한 도구입니다. None이면 E로 줍는 자원입니다.")]
    [SerializeField] private GatherToolType _requiredToolType = GatherToolType.None;
    [Tooltip("채집에 필요한 도구 티어입니다. 현재는 데이터 확인용이며, 도구 티어 시스템 연결 뒤 판정에 사용합니다.")]
    [SerializeField] private int _requiredToolTier;
    [Tooltip("채집 후 이 오브젝트를 비활성화할지 결정합니다.")]
    [SerializeField] private bool _disableAfterGather = true;
    [Tooltip("현재 이 자원을 채집하거나 주울 수 있는지 결정합니다.")]
    [SerializeField] private bool _canInteract = true;

    private bool _hasAppliedHarvestNodeData;
    private bool _hasWarnedMissingHarvestNodeData;

    public string HarvestNodeDataId
    {
        get
        {
            return _harvestNodeDataId;
        }
    }

    public string ResourceId
    {
        get
        {
            ApplyHarvestNodeDataIfPossible();
            return _resourceId;
        }
    }

    public int Amount
    {
        get
        {
            ApplyHarvestNodeDataIfPossible();
            return _amount;
        }
    }

    public GatherToolType RequiredToolType
    {
        get
        {
            ApplyHarvestNodeDataIfPossible();
            return _requiredToolType;
        }
    }

    public int RequiredToolTier
    {
        get
        {
            ApplyHarvestNodeDataIfPossible();
            return _requiredToolTier;
        }
    }

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        ApplyHarvestNodeDataIfPossible();

        if (_canInteract == false)
        {
            return false;
        }

        if (gameObjectInteractor == null)
        {
            return false;
        }

        return _requiredToolType == GatherToolType.None;
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        if (CanInteract(gameObjectInteractor) == false)
        {
            return;
        }

        GiveResourceToPlayer(_amount);
        ProcessGathered();
    }

    public bool TryGatherWithTool(GameObject gameObjectInteractor, GatherToolType toolType)
    {
        return TryGatherWithTool(gameObjectInteractor, toolType, null);
    }

    public bool TryGatherWithTool(GameObject gameObjectInteractor, GatherToolType toolType, ToolData toolData)
    {
        ApplyHarvestNodeDataIfPossible();

        if (CanGatherWithTool(gameObjectInteractor, toolType) == false)
        {
            return false;
        }

        GiveResourceToPlayer(GetGatherAmount(toolData));
        ProcessGathered();
        return true;
    }

    private bool CanGatherWithTool(GameObject gameObjectInteractor, GatherToolType toolType)
    {
        ApplyHarvestNodeDataIfPossible();

        if (_canInteract == false)
        {
            return false;
        }

        if (gameObjectInteractor == null)
        {
            return false;
        }

        if (_requiredToolType == GatherToolType.None)
        {
            return false;
        }

        return _requiredToolType == toolType;
    }

    public float GetToolGatherMultiplier(ToolData toolData)
    {
        if (toolData == null)
        {
            return 1f;
        }

        ApplyHarvestNodeDataIfPossible();

        if (_resourceId == PlayerModel.ResourceWood)
        {
            return GetSafeMultiplier(toolData.WoodGatherMultiplier);
        }

        if (_resourceId == PlayerModel.ResourceStone)
        {
            return GetSafeMultiplier(toolData.StoneGatherMultiplier);
        }

        if (_resourceId == PlayerModel.ResourceIron || _resourceId == PlayerModel.ResourceIronOre)
        {
            return GetSafeMultiplier(toolData.IronGatherMultiplier);
        }

        if (_resourceId == PlayerModel.ResourceGold || _resourceId == PlayerModel.ResourceGoldOre)
        {
            return GetSafeMultiplier(toolData.GoldGatherMultiplier);
        }

        if (_resourceId == PlayerModel.ResourceAdamantium || _resourceId == PlayerModel.ResourceAdamantiumOre)
        {
            return GetSafeMultiplier(toolData.AdamantiumGatherMultiplier);
        }

        return 1f;
    }

    private int GetGatherAmount(ToolData toolData)
    {
        float multiplier = GetToolGatherMultiplier(toolData);
        float adjustedAmount = _amount * multiplier;
        int roundedAmount = Mathf.RoundToInt(adjustedAmount);
        return Mathf.Max(1, roundedAmount);
    }

    private float GetSafeMultiplier(float multiplier)
    {
        if (multiplier <= 0f)
        {
            return 1f;
        }

        return multiplier;
    }

    private void GiveResourceToPlayer(int amount)
    {
        ApplyHarvestNodeDataIfPossible();

        if (GameManager.Inst == null)
        {
            Debug.LogWarning("ResourceNode: GameManager가 없어 자원을 지급하지 못했습니다.");
            return;
        }

        GameManager.Inst.PlayerModel.AddResource(_resourceId, amount);
        Debug.Log("ResourceNode: " + _resourceId + " +" + amount);
    }

    private void ProcessGathered()
    {
        _canInteract = false;

        if (_disableAfterGather == false)
        {
            return;
        }

        gameObject.SetActive(false);
    }

    private void ApplyHarvestNodeDataIfPossible()
    {
        if (_useHarvestNodeData == false)
        {
            return;
        }

        if (_hasAppliedHarvestNodeData)
        {
            return;
        }

        if (GameManager.Inst == null)
        {
            return;
        }

        GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
        if (gameDataManager == null)
        {
            return;
        }

        HarvestNodeData harvestNodeData = GetHarvestNodeData(gameDataManager);
        if (harvestNodeData == null)
        {
            WarnMissingHarvestNodeData();
            return;
        }

        ApplyHarvestNodeData(harvestNodeData);
    }

    private HarvestNodeData GetHarvestNodeData(GameDataManager gameDataManager)
    {
        if (string.IsNullOrEmpty(_harvestNodeDataId) == false)
        {
            return gameDataManager.GetHarvestNode(_harvestNodeDataId);
        }

        return FindMatchingHarvestNodeData(gameDataManager);
    }

    private HarvestNodeData FindMatchingHarvestNodeData(GameDataManager gameDataManager)
    {
        foreach (KeyValuePair<string, HarvestNodeData> dataPair in gameDataManager.HarvestNodeDataList)
        {
            HarvestNodeData harvestNodeData = dataPair.Value;
            if (harvestNodeData == null)
            {
                continue;
            }

            if (IsMatchingHarvestNodeData(harvestNodeData))
            {
                return harvestNodeData;
            }
        }

        return null;
    }

    private bool IsMatchingHarvestNodeData(HarvestNodeData harvestNodeData)
    {
        if (harvestNodeData.ResourceId != _resourceId)
        {
            return false;
        }

        GatherToolType dataToolType = ConvertToolType(harvestNodeData.RequiredToolType, _requiredToolType);
        return dataToolType == _requiredToolType;
    }

    private void ApplyHarvestNodeData(HarvestNodeData harvestNodeData)
    {
        if (string.IsNullOrEmpty(harvestNodeData.Id) == false)
        {
            _harvestNodeDataId = harvestNodeData.Id;
        }

        if (string.IsNullOrEmpty(harvestNodeData.ResourceId) == false)
        {
            _resourceId = harvestNodeData.ResourceId;
        }

        if (harvestNodeData.GatherAmount > 0)
        {
            _amount = harvestNodeData.GatherAmount;
        }

        _requiredToolType = ConvertToolType(harvestNodeData.RequiredToolType, _requiredToolType);
        _requiredToolTier = harvestNodeData.RequiredToolTier;
        _hasAppliedHarvestNodeData = true;
    }

    private GatherToolType ConvertToolType(string toolTypeText, GatherToolType fallbackToolType)
    {
        if (string.IsNullOrEmpty(toolTypeText))
        {
            return fallbackToolType;
        }

        GatherToolType parsedToolType;
        if (Enum.TryParse(toolTypeText, true, out parsedToolType))
        {
            return parsedToolType;
        }

        return fallbackToolType;
    }

    private void WarnMissingHarvestNodeData()
    {
        if (_hasWarnedMissingHarvestNodeData)
        {
            return;
        }

        if (string.IsNullOrEmpty(_harvestNodeDataId))
        {
            return;
        }

        _hasWarnedMissingHarvestNodeData = true;
        Debug.LogWarning("ResourceNode: harvest node data was not found. fallback values will be used. id=" + _harvestNodeDataId);
    }
}
