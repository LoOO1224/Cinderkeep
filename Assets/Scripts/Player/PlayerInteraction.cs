using UnityEngine;

// 플레이어가 바라보는 오브젝트와 상호작용하는 입구 역할만 담당합니다.
// 실제 채집, 획득, 파괴 처리는 대상 오브젝트의 전용 컴포넌트가 맡도록 분리합니다.
public sealed class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private LayerMask _interactionLayerMask;
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;
    [SerializeField] private Transform Transform_Camera;

    private void Start()
    {
        ConnectCameraIfNeeded();
    }

    private void Update()
    {
        ReadInteractionInput();
    }

    public void TryInteract()
    {
        IInteractable interactable = GetInteractableFromRay();

        if (interactable == null)
        {
            return;
        }

        if (interactable.CanInteract(gameObject) == false)
        {
            return;
        }

        interactable.Interact(gameObject);
    }

    private void ReadInteractionInput()
    {
        if (Input.GetKeyDown(_interactionKey))
        {
            TryInteract();
        }
    }

    private void ConnectCameraIfNeeded()
    {
        if (Transform_Camera != null)
        {
            return;
        }

        Camera camera = GetComponentInChildren<Camera>();

        if (camera == null)
        {
            return;
        }

        Transform_Camera = camera.transform;
    }

    private IInteractable GetInteractableFromRay()
    {
        if (Transform_Camera == null)
        {
            return null;
        }

        Ray ray = new Ray(Transform_Camera.position, Transform_Camera.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, _interactionDistance, _interactionLayerMask) == false)
        {
            return null;
        }

        return hitInfo.collider.GetComponentInParent<IInteractable>();
    }
}
