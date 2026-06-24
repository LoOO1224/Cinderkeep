using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 거점 내 건축물의 배치 상태와 파괴 처리를 관리하는 매니저입니다.
    // 건축물 생성은 GameObjectManager를 통하고, 자리 상태는 BuildingSpot이 담당합니다.
    public sealed class BuildingManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private List<BuildingSpot> _buildingSpots = new List<BuildingSpot>();
        [SerializeField] private GameObjectManager _gameObjectManager;

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
            _gameObjectManager = gameObjectManager;
        }

        public bool TryBuildAtSpot(BuildingSpot buildingSpot,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            // 정식 건축은 반드시 BuildingSpot을 통해 요청합니다.
            // 건축 비용과 티어 값은 buildings.json, crafting_recipes.json 연결 작업에서 확장합니다.
            Initialize();

            if (buildingSpot == null)
            {
                Debug.LogWarning("BuildingManager: 건축 지점이 비어 있습니다.");
                return false;
            }

            if (gameDataManager == null)
            {
                Debug.LogWarning("BuildingManager: GameDataManager 연결이 필요합니다.");
                return false;
            }

            if (string.IsNullOrEmpty(buildingSpot.BuildingDataId))
            {
                Debug.LogWarning("BuildingManager: BuildingSpot에 buildingDataId가 없습니다.");
                return false;
            }

            BuildingData buildingData = gameDataManager.GetBuilding(buildingSpot.BuildingDataId);
            if (buildingData == null)
            {
                Debug.LogWarning("BuildingManager: 건축 데이터를 찾을 수 없습니다. id=" + buildingSpot.BuildingDataId);
                return false;
            }

            GameObject buildingPrefab = buildingSpot.BuildingPrefab;
            if (CanBuildAtSpot(buildingSpot, buildingPrefab) == false)
            {
                return false;
            }

            bool isPreparedBuildingConsumed = TryConsumePreparedBuildingItem(buildingData);
            if (isPreparedBuildingConsumed == false
                && BuildingCostHelper.CanPayBuildCost(buildingData, playerModel, gameDataManager) == false)
            {
                Debug.LogWarning(BuildingCostHelper.GetNotEnoughResourceLog(buildingData, playerModel, gameDataManager));
                return false;
            }

            if (isPreparedBuildingConsumed == false
                && BuildingCostHelper.TryPayBuildCost(buildingData, playerModel, gameDataManager) == false)
            {
                Debug.LogWarning("BuildingManager: 건축 비용 차감에 실패했습니다.");
                return false;
            }

            GameObject createdBuilding = _gameObjectManager.CreateGameObject(
                buildingPrefab,
                buildingSpot.GetBuildPosition(),
                buildingSpot.GetBuildRotation());

            if (createdBuilding == null)
            {
                return false;
            }

            buildingSpot.PlaceBuilding(createdBuilding);
            buildingSpot.HideBuildingSpot();
            RegisterBuildingComponent(createdBuilding);
            return true;
        }

        private bool TryConsumePreparedBuildingItem(BuildingData buildingData)
        {
            if (buildingData == null || GameManager.Inst == null)
            {
                return false;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            if (inventoryModel == null)
            {
                return false;
            }

            return inventoryModel.TryConsumeItem(buildingData.Id, 1);
        }

        // 기존 시그니처를 쓰는 코드가 남아 있을 때를 위한 호환용입니다.
        public bool TryBuildAtSpot(BuildingSpot buildingSpot, GameObject buildingPrefab)
        {
            if (GameManager.Inst == null)
            {
                return false;
            }
            return TryBuildAtSpot(
                buildingSpot,
                GameManager.Inst.PlayerModel,
                GameManager.Inst.GetGameDataManager());
        }

        public bool TryBuildAtPosition(GameObject buildingPrefab, Vector3 buildPosition, Quaternion buildRotation)
        {
            // 현재 건축 루프는 정해진 BuildingSpot만 허용합니다.
            // 자유 배치 건축이 필요해지면 이 메서드의 검증과 비용 처리를 다시 연결합니다.
            Debug.LogWarning("BuildingManager: BuildingSpot이 있는 위치에서만 건축할 수 있습니다.");
            return false;
        }

        public void RegisterBuildingSpot(BuildingSpot buildingSpot)
        {
            if (buildingSpot == null)
            {
                return;
            }

            if (_buildingSpots.Contains(buildingSpot))
            {
                return;
            }

            _buildingSpots.Add(buildingSpot);
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
            if (_gameObjectManager == null)
            {
                Debug.LogWarning("BuildingManager: GameObjectManager 연결이 필요합니다.");
                return false;
            }

            if (buildingSpot == null)
            {
                Debug.LogWarning("BuildingManager: 건축 지점이 비어 있습니다.");
                return false;
            }

            if (_buildingSpots.Contains(buildingSpot) == false)
            {
                Debug.LogWarning("BuildingManager: 등록되지 않은 건축 지점입니다.");
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
            // 정식 프리팹에는 BuildingHp를 미리 붙이는 것이 기준입니다.
            // 누락된 팀원 프리팹을 main에서 흡수할 때만 fallback으로 붙여 게임 루프가 끊기지 않게 합니다.
            if (buildingObject == null)
            {
                return;
            }

            BuildingHp buildingHp = buildingObject.GetComponent<BuildingHp>();
            if (buildingHp == null)
            {
                buildingHp = buildingObject.AddComponent<BuildingHp>();
            }

            buildingHp.InitializeHp();

            RegisterBuilding(buildingHp);
        }

        private void RegisterSceneBuildings()
        {
            for (int i = 0; i < _buildingSpots.Count; i++)
            {
                BuildingSpot buildingSpot = _buildingSpots[i];
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
            for (int i = 0; i < _buildingSpots.Count; i++)
            {
                BuildingSpot buildingSpot = _buildingSpots[i];
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
