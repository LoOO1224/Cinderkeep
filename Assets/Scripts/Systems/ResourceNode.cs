using Cinderkeep.Gameplay;
using UnityEngine;

// 나무, 돌, 광석처럼 플레이어가 얻을 수 있는 자원 오브젝트입니다.
// E 입력은 줍기용, 좌클릭은 도끼/곡괭이 채집용으로 분리합니다.
public sealed class ResourceNode : MonoBehaviour, IInteractable
{
    [Header("Resource Data")]
    [SerializeField] private string _resourceId = PlayerModel.ResourceStone;
    [SerializeField] private int _amount = 1;
    [SerializeField] private GatherToolType _requiredToolType = GatherToolType.None;
    [SerializeField] private bool _disableAfterGather = true;
    [SerializeField] private bool _canInteract = true;

    public string ResourceId
    {
        get
        {
            return _resourceId;
        }
    }

    public int Amount
    {
        get
        {
            return _amount;
        }
    }

    public GatherToolType RequiredToolType
    {
        get
        {
            return _requiredToolType;
        }
    }

    public bool CanInteract(GameObject gameObjectInteractor)
    {
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

        GiveResourceToPlayer();
        ProcessGathered();
    }

    public bool TryGatherWithTool(GameObject gameObjectInteractor, GatherToolType toolType)
    {
        if (CanGatherWithTool(gameObjectInteractor, toolType) == false)
        {
            return false;
        }

        GiveResourceToPlayer();
        ProcessGathered();
        return true;
    }

    private bool CanGatherWithTool(GameObject gameObjectInteractor, GatherToolType toolType)
    {
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

    private void GiveResourceToPlayer()
    {
        if (GameManager.Inst == null)
        {
            Debug.LogWarning("ResourceNode: GameManager가 없어 자원을 지급하지 못했습니다.");
            return;
        }

        GameManager.Inst.PlayerModel.AddResource(_resourceId, _amount);
        Debug.Log("ResourceNode: " + _resourceId + " +" + _amount);
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
}
