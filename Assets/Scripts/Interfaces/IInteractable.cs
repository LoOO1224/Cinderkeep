using UnityEngine;

// 플레이어, 몬스터, 오브젝트가 공통으로 부를 수 있는 상호작용 규칙입니다.
// 각 오브젝트는 이 인터페이스를 구현해서 자기 역할만 처리합니다.
public interface IInteractable
{
    bool CanInteract(GameObject gameObjectInteractor);

    void Interact(GameObject gameObjectInteractor);
}
