using UnityEngine;

// 플레이 상태를 화면에 표시하거나 사용자의 UI 요청을 전달합니다.
// UI는 규칙을 소유하지 않고 모델을 읽고 시스템에 요청을 보내는 계층으로 유지합니다.
// 플레이어 사망 후 CinderHeart를 바라보는 관전 시점을 담당합니다.
// PlayerStatus는 죽음/부활만 판단하고, 카메라 전환과 복귀는 이 컴포넌트가 맡습니다.
public sealed class DeathCinderHeartView : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera _targetCamera;

    [Header("Target")]
    [SerializeField] private Transform _cinderHeartTarget;

    [Header("View")]
    [SerializeField] private Vector3 _viewOffset = new Vector3(0f, 5f, -8f);
    [SerializeField] private float _lookHeight = 1.6f;
    [SerializeField] private float _orbitSpeed = 18f;

    private Vector3 _originalLocalPosition;
    private Quaternion _originalLocalRotation;
    private bool _hasOriginalPose;
    private bool _isShowingDeathView;
    private float _orbitAngle;

    private void Update()
    {
        if (_isShowingDeathView == false)
        {
            return;
        }

        RefreshCinderHeartView(Time.deltaTime);
    }

    public void ShowCinderHeartView()
    {
        if (_targetCamera == null)
        {
            return;
        }

        if (_cinderHeartTarget == null)
        {
            return;
        }

        CacheOriginalPoseIfNeeded();
        _isShowingDeathView = true;
        _orbitAngle = 0f;
        RefreshCinderHeartView(0f);
    }

    public void HideCinderHeartView()
    {
        if (_targetCamera == null)
        {
            _isShowingDeathView = false;
            return;
        }

        if (_hasOriginalPose)
        {
            Transform cameraTransform = _targetCamera.transform;
            cameraTransform.localPosition = _originalLocalPosition;
            cameraTransform.localRotation = _originalLocalRotation;
        }

        _isShowingDeathView = false;
    }

    public void SetCamera(Camera targetCamera)
    {
        _targetCamera = targetCamera;
    }

    public void SetCinderHeartTarget(Transform cinderHeartTarget)
    {
        _cinderHeartTarget = cinderHeartTarget;
    }

    private Vector3 GetLookPosition()
    {
        Vector3 lookPosition = _cinderHeartTarget.position;
        lookPosition.y += _lookHeight;
        return lookPosition;
    }

    private void RefreshCinderHeartView(float deltaTime)
    {
        if (_targetCamera == null || _cinderHeartTarget == null)
        {
            return;
        }

        _orbitAngle += _orbitSpeed * deltaTime;
        Vector3 rotatedOffset = Quaternion.Euler(0f, _orbitAngle, 0f) * _viewOffset;
        Transform cameraTransform = _targetCamera.transform;
        cameraTransform.position = _cinderHeartTarget.position + rotatedOffset;
        cameraTransform.LookAt(GetLookPosition());
    }

    private void CacheOriginalPoseIfNeeded()
    {
        if (_hasOriginalPose || _targetCamera == null)
        {
            return;
        }

        Transform cameraTransform = _targetCamera.transform;
        _originalLocalPosition = cameraTransform.localPosition;
        _originalLocalRotation = cameraTransform.localRotation;
        _hasOriginalPose = true;
    }
}
