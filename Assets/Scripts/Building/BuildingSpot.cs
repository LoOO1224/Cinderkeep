using UnityEngine;
using Cinderkeep.Gameplay;

// 건축물이 올라갈 수 있는 자리 정보를 가진 컴포넌트입니다.
// 실제 생성은 BuildingManager가 담당하고, 이 클래스는 자리 상태만 관리합니다.
public sealed class BuildingSpot : MonoBehaviour
{
    [Header("Build Position")]
    [Tooltip("건축물이 실제로 배치될 위치입니다. 비어 있으면 이 오브젝트의 Transform을 사용합니다.")]
    [SerializeField] private Transform _spawnAnchor;

    private GameObject _currentBuildingObject;
    private bool _isEmpty = true;

    public bool IsEmpty
    {
        get
        {
            return _isEmpty;
        }
    }

    public GameObject CurrentBuildingObject
    {
        get
        {
            return _currentBuildingObject;
        }
    }

    private void Awake()
    {
        InitializeAnchor();
    }

    private void Start()
    {
        RegisterToBuildingManager();
    }

    // 씬이 시작될 때 각각의 BuildingSpot들이 스스로 등록하는 로직
    private void RegisterToBuildingManager()
    {
        if(GameManager.Inst == null)
        {
            return;
        }

        BuildingManager buildingManager = GameManager.Inst.GetBuildingManager();
        if(buildingManager == null)
        {
            return;
        }

        buildingManager.RegisterBuildingSpot(this);
    }

    public bool CanBuild()
    {
        return _isEmpty;
    }

    public Vector3 GetBuildPosition()
    {
        InitializeAnchor();
        return _spawnAnchor.position;
    }

    public Quaternion GetBuildRotation()
    {
        InitializeAnchor();
        return _spawnAnchor.rotation;
    }

    public void PlaceBuilding(GameObject buildingObject)
    {
        if (buildingObject == null)
        {
            return;
        }

        if (_isEmpty == false)
        {
            Debug.LogWarning(gameObject.name + " 건축 지점에는 이미 건축물이 있습니다.");
            return;
        }

        _currentBuildingObject = buildingObject;
        _currentBuildingObject.transform.SetParent(_spawnAnchor);
        _isEmpty = false;
    }

    public void ClearSpot()
    {
        _currentBuildingObject = null;
        _isEmpty = true;
    }

    private void InitializeAnchor()
    {
        if (_spawnAnchor != null)
        {
            return;
        }

        _spawnAnchor = transform;
    }
}
