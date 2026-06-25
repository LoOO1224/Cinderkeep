using UnityEngine;
using UnityEngine.Serialization;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// 1인칭 마우스 시점 회전을 담당하는 컴포넌트입니다.
// 이동은 PlayerMovement, 점프는 PlayerJump가 담당하므로 이 클래스는 회전만 처리합니다.
public sealed class PlayerView : MonoBehaviour
{
    [Header("View Settings")]
    [FormerlySerializedAs("mouseSensitivity")]
    [Tooltip("마우스 시점 회전 민감도입니다. 값이 클수록 더 빠르게 회전합니다.")]
    [SerializeField] private float _mouseSensitivity = 285f;

    [Header("Connected Objects")]
    [FormerlySerializedAs("playerBody")]
    [FormerlySerializedAs("Transform_PlayerBody")]
    [Tooltip("좌우 회전이 적용될 플레이어 몸체 Transform입니다.")]
    [SerializeField] private Transform _playerBody;
    [FormerlySerializedAs("Transform_Camera")]
    [Tooltip("상하 회전이 적용될 1인칭 카메라 Transform입니다.")]
    [SerializeField] private Transform _cameraTransform;

    private float _xRotation;

    private void Start()
    {
        ResolveReferences();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            return;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            RotateView();
        }
    }

    private void ResolveReferences()
    {
        if (_playerBody == null)
        {
            _playerBody = transform;
        }

        if (_cameraTransform != null)
        {
            return;
        }

        Camera camera = GetComponentInChildren<Camera>(true);
        if (camera != null)
        {
            _cameraTransform = camera.transform;
        }
    }

    private void RotateView()
    {
        if (_cameraTransform == null || _playerBody == null)
        {
            return;
        }

        Vector2 mouseAxis = CinderkeepInput.GetMouseAxis();
        float mouseX = mouseAxis.x * _mouseSensitivity * Time.deltaTime;
        float mouseY = mouseAxis.y * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -45f, 45f);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        _playerBody.Rotate(Vector3.up * mouseX);
    }
}
