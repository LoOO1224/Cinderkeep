using UnityEngine;

// 돌, 나무, 광석처럼 맵에 놓이는 자원 오브젝트의 기본 뼈대입니다.
// 지금은 실제 획득/파괴를 하지 않고, 나중에 인벤토리와 GameObjectManager에 연결할 자리만 열어둡니다.
public sealed class ResourceNode : MonoBehaviour, IInteractable
{
    [Header("자원 기본 정보")]
    [SerializeField] private string _resourceId = "stone";
    [SerializeField] private int _previewAmount = 1;
    [SerializeField] private bool _canInteract = true;

    public string ResourceId
    {
        get
        {
            return _resourceId;
        }
    }

    public int PreviewAmount
    {
        get
        {
            return _previewAmount;
        }
    }

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        if (_canInteract == false)
        {
            return false;
        }

        return gameObjectInteractor != null;
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        if (CanInteract(gameObjectInteractor) == false)
        {
            return;
        }

        RequestGather(gameObjectInteractor);
    }

    private void RequestGather(GameObject gameObjectInteractor)
    {
        // TODO: 인벤토리 구조가 들어오면 여기에서 자원 지급을 연결합니다.
        // TODO: GameObjectManager 제거 흐름이 정해지면 여기에서 자원 노드 제거를 요청합니다.
        // 현재 단계에서는 실제 게임 상태를 바꾸지 않습니다.
    }
}
