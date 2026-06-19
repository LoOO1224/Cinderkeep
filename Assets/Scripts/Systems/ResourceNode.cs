using Cinderkeep.Gameplay;
using UnityEngine;

// 나무, 돌, 광석처럼 플레이어가 얻을 수 있는 자원 오브젝트입니다.
// E 입력은 줍기용, 좌클릭은 도끼/곡괭이 채집용으로 분리합니다.
public sealed class ResourceNode : MonoBehaviour, IInteractable
{
    [Header("Resource Data")]
    [Tooltip("지급할 자원 ID입니다. PlayerModel의 ResourceWood, ResourceStone 같은 값과 맞춰 사용합니다.")]
    [SerializeField] private string _resourceId = PlayerModel.ResourceStone;
    [Tooltip("한 번 채집하거나 주웠을 때 지급할 자원 수량입니다.")]
    [SerializeField] private int _amount = 1;
    [Tooltip("채집에 필요한 도구입니다. None이면 E로 줍는 자원입니다.")]
    [SerializeField] private GatherToolType _requiredToolType = GatherToolType.None;
    [Tooltip("채집 후 이 오브젝트를 비활성화할지 결정합니다.")]
    [SerializeField] private bool _disableAfterGather = true;
    [Tooltip("현재 이 자원을 채집하거나 주울 수 있는지 결정합니다.")]
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
