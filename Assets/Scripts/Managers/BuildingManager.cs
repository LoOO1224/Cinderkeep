using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 거점 내 건축물의 배치 상태와 파괴 처리를 관리하는 매니저입니다.
    // 건축물 생성은 GameObjectManager를 통하고, 자리 상태는 BuildingSpot이 담당합니다.
    public sealed class BuildingManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private List<BuildingSpot> List_BuildingSpots = new List<BuildingSpot>();
        [SerializeField] private GameObjectManager GameObjectManager_GameObjectManager;

        private readonly List<BuildingHp> _activeBuildings = new List<BuildingHp>();
        private bool _isInitialized;

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            RegisterSceneBuildings();
            _isInitialized = true;
        }

        public void SetGameObjectManager(GameObjectManager gameObjectManager)
        {
            GameObjectManager_GameObjectManager = gameObjectManager;
        }

        public bool TryBuildAtSpot(BuildingSpot buildingSpot, GameObject buildingPrefab)
        {
            Initialize();

            if (CanBuildAtSpot(buildingSpot, buildingPrefab) == false)
            {
                return false;
            }

            GameObject createdBuilding = GameObjectManager_GameObjectManager.CreateGameObject(
                buildingPrefab,
                buildingSpot.GetBuildPosition(),
                buildingSpot.GetBuildRotation());

            if (createdBuilding == null)
            {
                return false;
            }

            buildingSpot.PlaceBuilding(createdBuilding);
            RegisterBuildingComponent(createdBuilding);
            return true;
        }

        public void RegisterBuildingSpot(BuildingSpot buildingSpot)
        {
            if (buildingSpot == null)
            {
                return;
            }

            if (List_BuildingSpots.Contains(buildingSpot))
            {
                return;
            }

            List_BuildingSpots.Add(buildingSpot);
        }

        public void RegisterBuilding(BuildingHp building)
        {
            if (building == null)
            {
                return;
            }

            if (_activeBuildings.Contains(building))
            {
                return;
            }

            _activeBuildings.Add(building);
            building.OnBuildingDestroyed += HandleBuildingDestroyed;
        }

        public void HandleBuildingDestroyed(BuildingHp building)
        {
            if (building == null)
            {
                return;
            }

            building.OnBuildingDestroyed -= HandleBuildingDestroyed;
            RemoveBuilding(building);
            ClearSpotByBuilding(building.gameObject);
            Debug.Log(building.gameObject.name + " 건축물 파괴 처리를 완료했습니다.");
        }

        public BuildingHp GetNearestBuilding(Vector3 monsterPosition)
        {
            if (_activeBuildings.Count == 0)
            {
                return null;
            }

            BuildingHp nearestBuilding = null;
            float shortestDistance = Mathf.Infinity;

            for (int i = 0; i < _activeBuildings.Count; i++)
            {
                BuildingHp building = _activeBuildings[i];
                if (building == null)
                {
                    continue;
                }

                float distance = Vector3.Distance(monsterPosition, building.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestBuilding = building;
                }
            }

            return nearestBuilding;
        }

        private bool CanBuildAtSpot(BuildingSpot buildingSpot, GameObject buildingPrefab)
        {
            if (GameObjectManager_GameObjectManager == null)
            {
                Debug.LogWarning("BuildingManager: GameObjectManager 연결이 필요합니다.");
                return false;
            }

            if (buildingSpot == null)
            {
                Debug.LogWarning("BuildingManager: 건축 지점이 비어 있습니다.");
                return false;
            }

            if (buildingSpot.CanBuild() == false)
            {
                Debug.LogWarning("BuildingManager: 이미 건축물이 있는 지점입니다.");
                return false;
            }

            if (buildingPrefab == null)
            {
                Debug.LogWarning("BuildingManager: 건축 프리팹이 비어 있습니다.");
                return false;
            }

            return true;
        }

        private void RegisterBuildingComponent(GameObject buildingObject)
        {
            if (buildingObject == null)
            {
                return;
            }

            BuildingHp buildingHp = buildingObject.GetComponent<BuildingHp>();
            if (buildingHp == null)
            {
                buildingHp = buildingObject.AddComponent<BuildingHp>();
            }

            RegisterBuilding(buildingHp);
        }

        private void RegisterSceneBuildings()
        {
            for (int i = 0; i < List_BuildingSpots.Count; i++)
            {
                BuildingSpot buildingSpot = List_BuildingSpots[i];
                if (buildingSpot == null)
                {
                    continue;
                }

                GameObject buildingObject = buildingSpot.CurrentBuildingObject;
                if (buildingObject != null)
                {
                    RegisterBuildingComponent(buildingObject);
                }
            }
        }

        private void RemoveBuilding(BuildingHp building)
        {
            if (_activeBuildings.Contains(building) == false)
            {
                return;
            }

            _activeBuildings.Remove(building);
        }

        private void ClearSpotByBuilding(GameObject buildingObject)
        {
            for (int i = 0; i < List_BuildingSpots.Count; i++)
            {
                BuildingSpot buildingSpot = List_BuildingSpots[i];
                if (buildingSpot == null)
                {
                    continue;
                }

                if (buildingSpot.CurrentBuildingObject == buildingObject)
                {
                    buildingSpot.ClearSpot();
                    return;
                }
            }
        }
    }
}
