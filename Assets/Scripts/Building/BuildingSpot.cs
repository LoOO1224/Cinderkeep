using System;
using System.Collections.Generic;
using Cinderkeep.Gameplay;
using UnityEngine;

// 기지 건축 지점, 비용, 체력, 방어 오브젝트 연결을 담당합니다.
// 설치, 비용, 내구도, 공격, 업그레이드 규칙을 나눠 방어 시스템을 확장하기 쉽게 유지합니다.
// 건축물이 올라갈 수 있는 자리 정보를 가진 컴포넌트입니다.
// 실제 생성은 BuildingManager가 담당하고, 이 클래스는 자리 상태만 관리합니다.
public sealed class BuildingSpot : MonoBehaviour
{
    [Header("Build Data")]
    [Tooltip("이 지점에 지을 건물의 기획 데이터 ID입니다. 예: wood_wall")]
    [SerializeField] private string _buildingDataId = "wood_wall";
    [Tooltip("기본으로 생성할 건축물 프리팹입니다. 별도 매핑이 없을 때 fallback으로 사용합니다.")]
    [SerializeField] private GameObject _buildingPrefab;
    [Tooltip("건축 데이터 ID 또는 prefabKey별로 실제 생성 프리팹을 덮어씁니다.")]
    [SerializeField] private List<BuildingPrefabEntry> _prefabOverrides = new List<BuildingPrefabEntry>();

    [Header("Build Position")]
    [Tooltip("건축물이 실제로 배치될 위치입니다. 비어 있으면 이 오브젝트의 Transform을 사용합니다.")]
    [SerializeField] private Transform _spawnAnchor;

    private GameObject _currentBuildingObject;
    private string _currentBuildingDataId;
    private bool _isEmpty = true;

    [Header("Build Spot Visual")]
    [Tooltip("건축 완료 시 끌 Spot 연출 오브젝트입니다. 비어 있으면 직접 자식 ParticleSystem을 찾습니다.")]
    [SerializeField] private GameObject _buildSpotVisualRoot;

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

    public string BuildingDataId
    {
        get
        {
            return _buildingDataId;
        }
    }

    public string CurrentBuildingDataId
    {
        get
        {
            return _currentBuildingDataId;
        }
    }

    public GameObject BuildingPrefab
    {
        get
        {
            return _buildingPrefab;
        }
    }

    public void ConfigureBuildingDataId(string buildingDataId)
    {
        if (string.IsNullOrEmpty(buildingDataId))
        {
            return;
        }

        _buildingDataId = buildingDataId;
    }

    public GameObject GetBuildingPrefab(BuildingData buildingData)
    {
        GameObject overridePrefab = FindOverridePrefab(buildingData);
        if (overridePrefab != null)
        {
            return overridePrefab;
        }

        return _buildingPrefab;
    }

    private void Awake()
    {
        InitializeAnchor();
        CacheBuildSpotVisualRoot();
    }

    private void Start()
    {
        RegisterToBuildingManager();
    }

    private void RegisterToBuildingManager()
    {
        if (GameManager.Inst == null)
        {
            return;
        }

        BuildingManager buildingManager = GameManager.Inst.GetBuildingManager();
        if (buildingManager == null)
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
        PlaceBuilding(buildingObject, _buildingDataId);
    }

    public void PlaceBuilding(GameObject buildingObject, string buildingDataId)
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
        _currentBuildingDataId = string.IsNullOrEmpty(buildingDataId) ? _buildingDataId : buildingDataId;
        buildingObject.transform.SetParent(transform, true);
        _isEmpty = false;
    }

    public void HideBuildingSpot()
    {
        SetBuildSpotVisualActive(false);
    }
    public void ShowBuildingSpot()
    {
        SetBuildSpotVisualActive(true);
    }

    public void ClearSpot()
    {
        _currentBuildingObject = null;
        _currentBuildingDataId = null;
        _isEmpty = true;
        ShowBuildingSpot();
    }

    public void ReplaceBuilding(GameObject buildingObject, string buildingDataId)
    {
        _currentBuildingObject = null;
        _isEmpty = true;
        PlaceBuilding(buildingObject, buildingDataId);
        HideBuildingSpot();
    }

    private void InitializeAnchor()
    {
        if (_spawnAnchor != null)
        {
            return;
        }

        _spawnAnchor = transform;
    }

    private void SetSpotRendererVisible(bool isVisible)
    {
        Renderer spotRenderer = GetComponent<Renderer>();
        if (spotRenderer != null)
        {
            spotRenderer.enabled = isVisible;
        }
    }

    private void CacheBuildSpotVisualRoot()
    {
        if (_buildSpotVisualRoot != null)
        {
            return;
        }
        // 직접 자식 중 Particle System만 찾습니다 (건축물 자식과 구분)
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            if (childTransform.GetComponent<ParticleSystem>() != null)
            {
                _buildSpotVisualRoot = childTransform.gameObject;
                return;
            }
        }
    }

    private void SetBuildSpotVisualActive(bool isActive)
    {
        CacheBuildSpotVisualRoot();

        if (_buildSpotVisualRoot != null)
        {
            _buildSpotVisualRoot.SetActive(isActive);
        }
        else
        {
            SetDirectChildSpotVisualsActive(isActive);
        }

        SetSpotRendererVisible(isActive);
    }

    private void SetDirectChildSpotVisualsActive(bool isActive)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // 이미 지어진 건축물 자식은 끄지 않습니다.
            if (_currentBuildingObject != null && childTransform.gameObject == _currentBuildingObject)
            {
                continue;
            }

            if (childTransform.GetComponent<ParticleSystem>() != null)
            {
                childTransform.gameObject.SetActive(isActive);
            }
        }
    }

    private GameObject FindOverridePrefab(BuildingData buildingData)
    {
        if (buildingData == null || _prefabOverrides == null)
        {
            return null;
        }

        for (int i = 0; i < _prefabOverrides.Count; i++)
        {
            BuildingPrefabEntry entry = _prefabOverrides[i];
            if (entry == null || entry.BuildingPrefab == null)
            {
                continue;
            }

            if (entry.Matches(buildingData))
            {
                return entry.BuildingPrefab;
            }
        }

        return null;
    }

    [Serializable]
    private sealed class BuildingPrefabEntry
    {
        [Tooltip("매칭할 buildings.json의 _id입니다.")]
        [SerializeField] private string _buildingDataId;
        [Tooltip("매칭할 buildings.json의 _prefabKey입니다. buildingDataId가 비어 있을 때도 사용할 수 있습니다.")]
        [SerializeField] private string _prefabKey;
        [Tooltip("해당 데이터와 매칭되면 생성할 실제 프리팹입니다.")]
        [SerializeField] private GameObject _buildingPrefab;

        public GameObject BuildingPrefab
        {
            get
            {
                return _buildingPrefab;
            }
        }

        public bool Matches(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_buildingDataId) == false
                && string.Equals(_buildingDataId, buildingData.Id, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.IsNullOrEmpty(_prefabKey) == false
                && string.Equals(_prefabKey, buildingData.PrefabKey, StringComparison.OrdinalIgnoreCase);
        }
    }
}
