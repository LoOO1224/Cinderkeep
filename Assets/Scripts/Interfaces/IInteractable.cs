using UnityEngine;

// 런타임 시스템이 공유하는 최소 계약입니다.
// 인터페이스는 작게 유지해 구현체 책임이 섞이지 않게 합니다.
// 플레이어가 E키로 상호작용할 수 있는 오브젝트의 공통 규칙입니다.
// 나무, 돌, 제작대, 문처럼 각 오브젝트가 자기 역할에 맞게 구현합니다.
public interface IInteractable
{
    bool CanInteract(GameObject gameObjectInteractor);

    void Interact(GameObject gameObjectInteractor);
}
