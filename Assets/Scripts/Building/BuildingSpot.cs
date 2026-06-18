using Cinderkeep.Gameplay;
using UnityEngine;

public class BuildingSpot : MonoBehaviour
{
    // 건물이 생성될 기준점
    [SerializeField] private Transform Transform_SpawnAnchor;

    private GameObject _currentBuildingObj = null;

    public bool IsEmpty { get; private set; } = true;

    // 매니저가 이 자리에 배치된 옵젝을 확인할수 있도록 프로퍼티 제공
    public GameObject CurrentBuildingObj { get { return _currentBuildingObj; } }

    private void Awake()
    {
        // 등록 안했다면 본인의 Transform을 기본값으로 설정
        if(Transform_SpawnAnchor == null)
        {
            Transform_SpawnAnchor = this.transform;
        }
    }

    // 해당 BuildingSpot에 건물을 짓는 함수
    public void Build(GameObject buildingPrefab)
    {
        // 자리에 이미 건물있을때
        if (!IsEmpty)
        {
            Debug.LogWarning($"{gameObject.name} 자리에 이미 건물이 있습니다");
            return;
        }
        
        // 프리팹이 null일때 그냥 return;
        if (buildingPrefab == null) return;

        _currentBuildingObj= Instantiate(buildingPrefab, Transform_SpawnAnchor.position, Transform_SpawnAnchor.rotation);

        _currentBuildingObj.transform.SetParent(Transform_SpawnAnchor);

        // 건물이 지어졌으니 상태변경
        IsEmpty = false;

        BuildingHp buildingHp = _currentBuildingObj.GetComponent<BuildingHp>();
        if (buildingHp != null && GameManager.Inst != null)
        {
            GameManager.Inst.GetBuildingManager().RegisterBuilding(buildingHp);
        }
    }

    // 건물 파괴됐을때 Spot 정리후 비워두는 함수
    public void ClearSpot()
    {
        _currentBuildingObj = null;
        IsEmpty = true;
    }
}
