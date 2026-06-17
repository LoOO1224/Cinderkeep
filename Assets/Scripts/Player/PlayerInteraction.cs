using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 세팅")]
    [SerializeField] private float _interactionDistance = 3f; // 상호작용 거리
    [SerializeField] private LayerMask _interactionLayerMask; // 상호작용 레이어 마스크
    [SerializeField] private Transform Transform_Camera; // 플레이어 카메라


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    private void TryInteract() // 자원 상호작용용 임시 코드
    {
        Ray ray = new Ray(Transform_Camera.position, Transform_Camera.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, _interactionDistance, _interactionLayerMask))
        {
            GameObject hitObject = hitInfo.collider.gameObject;

            // 2. 비교할 타겟 레이어(Resources)의 번호를 int로 추출
            int resourceLayer = LayerMask.NameToLayer("Resources");

            if (hitObject.layer != resourceLayer)
            {
                // Default 레이어(벽, 바닥 등)에 맞았을 경우 파괴하지 않고 작업을 중단
                Debug.LogWarning($"[PlayerInteraction] 자원이 아닌 오브젝트를 타격, 무시합니다. 대상: {hitObject.name}");
                return;
            }

            Destroy(hitObject);
            Debug.Log($"자원 오브젝트 파괴 : {hitObject.name}");
        }
    }
}
