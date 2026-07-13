using Cinderkeep.Gameplay;
using UnityEngine;

// 1인칭 카메라 앞에 현재 장착한 도구나 무기를 보여주는 표시 전용 컴포넌트입니다.
// 실제 채집/공격 판정은 PlayerToolUse와 PlayerAttack이 담당하고, 이 클래스는 손맛 피드백만 맡습니다.
public sealed class FirstPersonToolView : MonoBehaviour
{
    [Header("Connected Components")]
    [Tooltip("현재 퀵슬롯에서 선택된 도구 정보를 제공합니다.")]
    [SerializeField] private PlayerToolController _playerToolController;

    [Header("Tool View Objects")]
    [Tooltip("도끼류 도구를 들었을 때 보여줄 1인칭 오브젝트입니다.")]
    [SerializeField] private GameObject _axeView;
    [Tooltip("곡괭이류 도구를 들었을 때 보여줄 1인칭 오브젝트입니다.")]
    [SerializeField] private GameObject _pickaxeView;
    [Tooltip("손돌 또는 빈손 상태에서 보여줄 1인칭 오브젝트입니다.")]
    [SerializeField] private GameObject _handView;
    [Tooltip("손돌을 주웠을 때 오른손에 보이는 장비 View입니다.")]
    [SerializeField] private GameObject _handStoneView;
    [Tooltip("무기를 장착했을 때 보여줄 1인칭 오브젝트입니다. 참조가 없으면 비상용 검 형태를 만듭니다.")]
    [SerializeField] private GameObject _weaponView;

    [Header("Simple Swing")]
    [Tooltip("도구나 무기를 휘두를 때 적용할 최대 회전 각도입니다.")]
    [SerializeField] private float _swingAngle = 22f;
    [Tooltip("도구나 무기를 휘두르는 속도입니다.")]
    [SerializeField] private float _swingSpeed = 18f;
    [Tooltip("무기 View가 없을 때 임시 검 오브젝트를 자동 생성합니다.")]
    [SerializeField] private bool _createFallbackWeaponView = true;

    private Transform _currentToolTransform;
    private Quaternion _defaultRotation;
    private float _swingTime;
    private bool _isSwinging;
    private int _lastSwingFrame = -1;

    public static FirstPersonToolView EnsureSceneView()
    {
        FirstPersonToolView existing = FindFirstObjectByType<FirstPersonToolView>(FindObjectsInactive.Include);
        if (existing != null)
        {
            existing.gameObject.SetActive(true);
            return existing;
        }

        PlayerToolController toolController = FindFirstObjectByType<PlayerToolController>();
        if (toolController == null)
        {
            return null;
        }

        Transform viewParent = FindViewParent(toolController.transform);
        GameObject viewObject = new GameObject("View_FirstPersonToolRuntime");
        viewObject.transform.SetParent(viewParent, false);
        viewObject.transform.localPosition = Vector3.zero;
        viewObject.transform.localRotation = Quaternion.identity;
        viewObject.transform.localScale = Vector3.one;

        FirstPersonToolView view = viewObject.AddComponent<FirstPersonToolView>();
        view._playerToolController = toolController;
        return view;
    }

    private void Start()
    {
        ConnectComponents();
        EnsureFallbackHandStoneView();
        EnsureFallbackWeaponView();
        RefreshToolView();
    }

    private void Update()
    {
        RefreshToolView();
        UpdateSwing();
    }

    public void PlaySwing()
    {
        if (_currentToolTransform == null)
        {
            return;
        }

        if (_lastSwingFrame == Time.frameCount)
        {
            return;
        }

        _lastSwingFrame = Time.frameCount;
        _swingTime = 0f;
        _isSwinging = true;
    }

    private void ConnectComponents()
    {
        if (_playerToolController != null)
        {
            return;
        }

        _playerToolController = GetComponentInParent<PlayerToolController>();
    }

    private static Transform FindViewParent(Transform playerRoot)
    {
        if (playerRoot == null)
        {
            return null;
        }

        Camera playerCamera = playerRoot.GetComponentInChildren<Camera>(true);
        if (playerCamera != null)
        {
            return playerCamera.transform;
        }

        return playerRoot;
    }

    private void RefreshToolView()
    {
        if (_playerToolController == null)
        {
            SetToolActive(null);
            return;
        }

        if (_playerToolController.CurrentToolDataId == PlayerToolController.HandStoneToolDataId)
        {
            SetToolActive(_handStoneView);
            return;
        }

        GatherToolType currentToolType = _playerToolController.CurrentToolType;

        if (currentToolType == GatherToolType.Axe)
        {
            SetToolActive(_axeView);
            return;
        }

        if (currentToolType == GatherToolType.Pickaxe)
        {
            SetToolActive(_pickaxeView);
            return;
        }

        if (HasEquippedWeapon())
        {
            SetToolActive(_weaponView);
            return;
        }

        SetToolActive(null);
    }

    private void SetToolActive(GameObject activeToolObject)
    {
        SetActive(_axeView, activeToolObject == _axeView);
        SetActive(_pickaxeView, activeToolObject == _pickaxeView);
        SetActive(_handView, activeToolObject == _handView);
        SetActive(_handStoneView, activeToolObject == _handStoneView);
        SetActive(_weaponView, activeToolObject == _weaponView);

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

    private bool HasEquippedWeapon()
    {
        if (GameManager.Inst == null)
        {
            return false;
        }

        PlayerEquipmentModel equipmentModel = GameManager.Inst.PlayerEquipmentModel;
        if (equipmentModel == null)
        {
            return false;
        }

        return string.IsNullOrEmpty(equipmentModel.GetEquippedItemId(EquipmentSlotType.Weapon)) == false;
    }

    private void EnsureFallbackHandStoneView()
    {
        if (_handStoneView != null)
        {
            return;
        }

        GameObject stoneObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stoneObject.name = "View_HandStone_Fallback";
        stoneObject.transform.SetParent(transform, false);
        stoneObject.transform.localPosition = new Vector3(0.42f, -0.32f, 0.72f);
        stoneObject.transform.localRotation = Quaternion.Euler(8f, -18f, 0f);
        stoneObject.transform.localScale = new Vector3(0.22f, 0.18f, 0.20f);
        RemoveCollider(stoneObject);
        RuntimePrimitiveMaterial.ApplyColor(stoneObject, new Color(0.42f, 0.44f, 0.46f, 1f), "MAT_Runtime_ViewHandStone");
        stoneObject.SetActive(false);
        _handStoneView = stoneObject;
    }

    private void EnsureFallbackWeaponView()
    {
        if (_weaponView != null || _createFallbackWeaponView == false)
        {
            return;
        }

        GameObject weaponRoot = new GameObject("View_Weapon_Fallback");
        weaponRoot.transform.SetParent(transform, false);
        weaponRoot.transform.localPosition = new Vector3(0.42f, -0.35f, 0.82f);
        weaponRoot.transform.localRotation = Quaternion.Euler(20f, -18f, 8f);
        weaponRoot.transform.localScale = Vector3.one;

        GameObject bladeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bladeObject.name = "Blade";
        bladeObject.transform.SetParent(weaponRoot.transform, false);
        bladeObject.transform.localPosition = new Vector3(0f, 0.22f, 0.18f);
        bladeObject.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
        bladeObject.transform.localScale = new Vector3(0.08f, 0.62f, 0.06f);
        RemoveCollider(bladeObject);

        GameObject handleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handleObject.name = "Handle";
        handleObject.transform.SetParent(weaponRoot.transform, false);
        handleObject.transform.localPosition = new Vector3(0f, -0.18f, 0f);
        handleObject.transform.localRotation = Quaternion.identity;
        handleObject.transform.localScale = new Vector3(0.11f, 0.34f, 0.11f);
        RemoveCollider(handleObject);

        weaponRoot.SetActive(false);
        _weaponView = weaponRoot;
    }

    private void RemoveCollider(GameObject targetObject)
    {
        Collider targetCollider = targetObject.GetComponent<Collider>();
        if (targetCollider != null)
        {
            Destroy(targetCollider);
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
