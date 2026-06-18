using UnityEngine;

// 1인칭 카메라 앞에 현재 장착한 도구를 보여주는 View 전용 컴포넌트입니다.
// 실제 채집 판정은 PlayerInteraction이 담당하고, 이 클래스는 화면 표시와 짧은 휘두르기만 담당합니다.
public sealed class FirstPersonToolView : MonoBehaviour
{
    [Header("Connected Components")]
    [SerializeField] private PlayerToolController PlayerToolController_PlayerToolController;

    [Header("Tool View Objects")]
    [SerializeField] private GameObject GameObject_AxeView;
    [SerializeField] private GameObject GameObject_PickaxeView;
    [SerializeField] private GameObject GameObject_HandView;

    [Header("Simple Swing")]
    [SerializeField] private float _swingAngle = 22f;
    [SerializeField] private float _swingSpeed = 18f;

    private Transform _currentToolTransform;
    private Quaternion _defaultRotation;
    private float _swingTime;
    private bool _isSwinging;

    private void Start()
    {
        ConnectComponents();
        RefreshToolView();
    }

    private void Update()
    {
        RefreshToolView();
        ReadSwingInput();
        UpdateSwing();
    }

    public void PlaySwing()
    {
        if (_currentToolTransform == null)
        {
            return;
        }

        _swingTime = 0f;
        _isSwinging = true;
    }

    private void ConnectComponents()
    {
        if (PlayerToolController_PlayerToolController != null)
        {
            return;
        }

        PlayerToolController_PlayerToolController = GetComponentInParent<PlayerToolController>();
    }

    private void RefreshToolView()
    {
        if (PlayerToolController_PlayerToolController == null)
        {
            SetToolActive(null);
            return;
        }

        GatherToolType currentToolType = PlayerToolController_PlayerToolController.CurrentToolType;

        if (currentToolType == GatherToolType.Axe)
        {
            SetToolActive(GameObject_AxeView);
            return;
        }

        if (currentToolType == GatherToolType.Pickaxe)
        {
            SetToolActive(GameObject_PickaxeView);
            return;
        }

        SetToolActive(GameObject_HandView);
    }

    private void SetToolActive(GameObject activeToolObject)
    {
        SetActive(GameObject_AxeView, activeToolObject == GameObject_AxeView);
        SetActive(GameObject_PickaxeView, activeToolObject == GameObject_PickaxeView);
        SetActive(GameObject_HandView, activeToolObject == GameObject_HandView);

        if (activeToolObject == null)
        {
            _currentToolTransform = null;
            return;
        }

        if (_currentToolTransform == activeToolObject.transform)
        {
            return;
        }

        _currentToolTransform = activeToolObject.transform;
        _defaultRotation = _currentToolTransform.localRotation;
    }

    private void SetActive(GameObject targetObject, bool isActive)
    {
        if (targetObject == null)
        {
            return;
        }

        if (targetObject.activeSelf == isActive)
        {
            return;
        }

        targetObject.SetActive(isActive);
    }

    private void ReadSwingInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
        {
            PlaySwing();
        }
    }

    private void UpdateSwing()
    {
        if (_isSwinging == false)
        {
            return;
        }

        if (_currentToolTransform == null)
        {
            _isSwinging = false;
            return;
        }

        _swingTime += Time.deltaTime * _swingSpeed;
        float swingRate = Mathf.Sin(_swingTime);
        float currentAngle = swingRate * _swingAngle;

        _currentToolTransform.localRotation = _defaultRotation * Quaternion.Euler(currentAngle, 0f, 0f);

        if (_swingTime >= Mathf.PI)
        {
            _currentToolTransform.localRotation = _defaultRotation;
            _isSwinging = false;
        }
    }
}
