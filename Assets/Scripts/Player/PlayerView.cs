using UnityEngine;
using UnityEngine.Serialization;

public sealed class PlayerView : MonoBehaviour
{
    [FormerlySerializedAs("mouseSensitivity")]
    [SerializeField] private float _mouseSensitivity = 200f;
    [FormerlySerializedAs("playerBody")]
    [SerializeField] private Transform Transform_PlayerBody;
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

        if (Transform_Camera == null)
        {
            Camera camera = GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                Transform_Camera = camera.transform;
            }
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
