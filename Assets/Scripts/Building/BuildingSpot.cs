using UnityEngine;

// 건축물이 놓일 수 있는 자리 정보를 가진 컴포넌트입니다.
// 실제 생성은 BuildingManager가 담당하고, 이 클래스는 자리 상태만 관리합니다.
public sealed class BuildingSpot : MonoBehaviour
{
    [Header("Build Position")]
    [SerializeField] private Transform Transform_SpawnAnchor;

    private GameObject _currentBuildingObject;
    private bool _isEmpty = true;

    public bool IsEmpty
    {
        get { return _isEmpty; }
    }

    public GameObject CurrentBuildingObject
    {
        get { return _currentBuildingObject; }
    }

    private void Awake()
    {
        InitializeAnchor();
    }

    public bool CanBuild()
    {
        return _isEmpty;
    }

    public Vector3 GetBuildPosition()
    {
        InitializeAnchor();
        return Transform_SpawnAnchor.position;
    }

    public Quaternion GetBuildRotation()
    {
        InitializeAnchor();
        return Transform_SpawnAnchor.rotation;
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
        _currentBuildingObject.transform.SetParent(Transform_SpawnAnchor);
        _isEmpty = false;
    }

    public void ClearSpot()
    {
        _currentBuildingObject = null;
        _isEmpty = true;
    }

    private void InitializeAnchor()
    {
        if (Transform_SpawnAnchor != null)
        {
            return;
        }

        Transform_SpawnAnchor = transform;
    }
}
