using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 세팅")]
    [SerializeField] private float _interactionDistance = 3f; // 상호작용 거리
    [SerializeField] private LayerMask _interactionLayerMask; // 상호작용 레이어 마스크
    [SerializeField] private Transform _camera; // 플레이어 카메라
}
