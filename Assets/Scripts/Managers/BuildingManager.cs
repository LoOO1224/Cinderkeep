using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 거점 내 건축물의 배치 상태 관리 및 파괴 시의 게임 규칙을 총괄하는 매니저
    public sealed class BuildingManager : MonoBehaviour, IGameInitializable
    {
        // BuildingHp 리스트 관리
        private readonly List<BuildingHp> _activeBuildings = new List<BuildingHp>();

        [SerializeField] private List<BuildingSpot> List_BuildingSpots = new List<BuildingSpot>();
        [SerializeField] private GameObjectManager _gameObjectManager;

        private bool _isInitialized;
        public bool IsInitialized { get { return _isInitialized; } }

        public void Initialize()
        {
            Debug.Log("BuildingManager: 거점건축 시스템 초기화 완료");
            _isInitialized = true;
        }

        // 새로운 건축물이 지어졌을 때 리스트에 등록하고 이벤트를 구독하는 함수
        public void RegisterBuilding(BuildingHp building)
        {
            if (building == null) return;

            if (!_activeBuildings.Contains(building))
            {
                _activeBuildings.Add(building);

                // 이벤트 메서드를 직접 연결합니다
                building.OnBuildingDestroyed += HandleBuildingDestroyed;
            }
        }

        // 건축물이 파괴되었을 때 호출되어 전후 처리를 수행합니다
        public void HandleBuildingDestroyed(BuildingHp building)
        {
            if (building == null) return;

            // 이벤트 구독 해제 및 리스트에서 제외 (몬스터 타겟팅 방지)
            building.OnBuildingDestroyed -= HandleBuildingDestroyed;

            if (_activeBuildings.Contains(building))
            {
                _activeBuildings.Remove(building);
            }

            // 해당 건물이 서 있던 건축 슬롯(Spot)을 찾아 'ClearSpot' 호출
            foreach (var spot in List_BuildingSpots)
            {
                if (spot != null && spot.CurrentBuildingObj == building.gameObject)
                {
                    spot.ClearSpot(); // 자리를 다시 빈터로 만듬
                    break;
                }
            }

            //// 파괴된 건물에 불꽃심장 컴포넌트가 있다면 게임오버 트리거... 인데 이건 GameManager에서 하는게 좋아보여서 주석처리 (통으로 삭제하셔도 됩니다)
            //if (building.GetComponent<CinderHeart>() != null)
            //{
            //    // UIManager.Inst.OpenGameOverPanel(); // 추후 UI매니저와 연동해도 되는 부분
            //    Debug.LogWarning("BuildingManager: 불꽃심장이 파괴되었습니다 GAME OVER");
            //    return;
            //}

            Debug.Log($"[{building.gameObject.name}] 건축물 파괴에 따른 게임 규칙 처리 완료");
        }

        // 몬스터 AI 등이 가장 가까운 방어 건물을 타겟팅할 때 사용하는 함수
        public BuildingHp GetNearestBuilding(Vector3 monsterPosition)
        {
            if (_activeBuildings.Count == 0) return null;

            BuildingHp nearestBuilding = null;
            float shortestDistance = Mathf.Infinity;

            foreach (var building in _activeBuildings)
            {
                if (building == null) continue;

                float distance = Vector3.Distance(monsterPosition, building.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestBuilding = building;
                }
            }

            return nearestBuilding;
        }
    }
}