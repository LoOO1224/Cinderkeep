using UnityEngine;
using UnityEngine.Serialization;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// 플레이어가 바라보는 오브젝트와 상호작용하는 입구 컴포넌트입니다.
// 실제 채집, 제작, 문 열기는 대상 오브젝트의 IInteractable 구현체가 처리합니다.
public sealed class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("E 상호작용이 닿는 최대 거리입니다.")]
    [SerializeField] private float _interactionDistance = 3f;
    [Tooltip("E 상호작용으로 감지할 레이어입니다.")]
    [SerializeField] private LayerMask _interactionLayerMask;
    [Tooltip("상호작용 입력 키입니다.")]
    [SerializeField] private KeyCode _interactionKey = KeyCode.E;

    [Header("Connected Objects")]
    [FormerlySerializedAs("Transform_Camera")]
    [Tooltip("상호작용 Ray가 시작되는 카메라 Transform입니다. 비어 있으면 자식 카메라를 찾습니다.")]
    [SerializeField] private Transform _cameraTransform;

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
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            return;
        }

        if (CinderkeepInput.WasKeyPressedThisFrame(_interactionKey))
        {
            TryInteract();
        }
    }

    private void ConnectCameraIfNeeded()
    {
        if (_cameraTransform != null)
        {
            return;
        }

        Camera camera = GetComponentInChildren<Camera>();

        if (camera == null)
        {
            return;
        }

        _cameraTransform = camera.transform;
    }

    private IInteractable GetInteractableFromRay()
    {
        if (_cameraTransform == null)
        {
            return null;
        }

        Ray ray = new Ray(_cameraTransform.position, _cameraTransform.forward);
        RaycastHit hitInfo;

        int interactionMask = _interactionLayerMask.value == 0 ? ~0 : _interactionLayerMask.value;
        if (Physics.Raycast(ray, out hitInfo, _interactionDistance, interactionMask) == false)
        {
            return null;
        }

        return hitInfo.collider.GetComponentInParent<IInteractable>();
    }
}
