using UnityEngine;
using UnityEngine.Serialization;

// 1인칭 마우스 시점 회전을 담당하는 컴포넌트입니다.
// 이동은 PlayerMovement, 점프는 PlayerJump가 담당하므로 이 클래스는 회전만 처리합니다.
public sealed class PlayerView : MonoBehaviour
{
    [Header("View Settings")]
    [FormerlySerializedAs("mouseSensitivity")]
    [SerializeField] private float _mouseSensitivity = 285f;

    [Header("Connected Objects")]
    [FormerlySerializedAs("playerBody")]
    [FormerlySerializedAs("Transform_PlayerBody")]
    [FormerlySerializedAs("_playerBody")]
    [SerializeField] private Transform Transform_PlayerBody;
    [FormerlySerializedAs("Transform_Camera")]
    [FormerlySerializedAs("_cameraTransform")]
    [SerializeField] private Transform Transform_Camera;

    private float _xRotation;

    private void Start()
    {
        ResolveReferences();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        RotateView();
    }

    private void ResolveReferences()
    {
        if (Transform_PlayerBody == null)
        {
            Transform_PlayerBody = transform;
        }

        if (Transform_Camera != null)
        {
            return;
        }

        Camera camera = GetComponentInChildren<Camera>(true);
        if (camera != null)
        {
            Transform_Camera = camera.transform;
        }
    }

    private void RotateView()
    {
        if (Transform_Camera == null || Transform_PlayerBody == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -45f, 45f);

        Transform_Camera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        Transform_PlayerBody.Rotate(Vector3.up * mouseX);
    }
}
