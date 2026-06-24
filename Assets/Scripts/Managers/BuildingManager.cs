using System.Collections.Generic;
using System;
using UnityEngine;

// 5.00 direction: Coordinates a focused slice of the 5.00 game loop from scene and runtime references.
// 5.01+ note: Keep this manager as a thin hub; move calculations and feature rules into smaller systems/helpers.
namespace Cinderkeep.Gameplay
{
    // 거점 내 건축물의 배치 상태와 파괴 처리를 관리하는 매니저입니다.
    // 건축물 생성은 GameObjectManager를 통하고, 자리 상태는 BuildingSpot이 담당합니다.
    public sealed class BuildingManager : MonoBehaviour, IGameInitializable
    {
        public static event Action<BuildingData> BuildingPlacedGlobal;

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

            if (CanUseBuildingSpot(buildingSpot) == false)
            {
                return false;
            }

            if (buildingSpot.CanBuild() == false)
            {
                return TryUpgradeAtSpot(buildingSpot, playerModel, gameDataManager);
            }

            bool hasPreparedBuildingItem = HasPreparedBuildingItem(buildingData);
            if (hasPreparedBuildingItem == false
                && BuildingCostHelper.CanPayBuildCost(buildingData, playerModel, gameDataManager) == false)
            {
                Debug.LogWarning(BuildingCostHelper.GetNotEnoughResourceLog(buildingData, playerModel, gameDataManager));
                return false;
            }

            GameObject buildingPrefab = buildingSpot.GetBuildingPrefab(buildingData);
            GameObject createdBuilding = CreateBuildingObject(
                buildingPrefab,
                buildingData,
                buildingSpot.GetBuildPosition(),
                buildingSpot.GetBuildRotation());

            if (createdBuilding == null)
            {
                return false;
            }

            bool isCostPaid = hasPreparedBuildingItem
                ? TryConsumePreparedBuildingItem(buildingData)
                : BuildingCostHelper.TryPayBuildCost(buildingData, playerModel, gameDataManager);

            if (isCostPaid == false)
            {
                DestroyCreatedBuilding(createdBuilding);
                Debug.LogWarning("BuildingManager: 건축 비용 차감에 실패했습니다.");
                return false;
            }

            buildingSpot.PlaceBuilding(createdBuilding, buildingData.Id);
            buildingSpot.HideBuildingSpot();
            RegisterBuildingComponent(createdBuilding, buildingData);
            NotifyBuildingPlaced(buildingData);
            return true;
        }

        private bool TryUpgradeAtSpot(
            BuildingSpot buildingSpot,
            PlayerModel playerModel,
            GameDataManager gameDataManager)
        {
            string fromBuildingId = string.IsNullOrEmpty(buildingSpot.CurrentBuildingDataId)
                ? buildingSpot.BuildingDataId
                : buildingSpot.CurrentBuildingDataId;

            BuildingUpgradeData upgradeData = FindAvailableUpgrade(fromBuildingId, gameDataManager);
            if (upgradeData == null)
            {
                Debug.LogWarning("BuildingManager: 가능한 건축 업그레이드를 찾을 수 없습니다. from=" + fromBuildingId);
                return false;
            }

            BuildingData fromBuildingData = gameDataManager.GetBuilding(fromBuildingId);
            BuildingData toBuildingData = gameDataManager.GetBuilding(upgradeData.ToBuildingId);
            if (toBuildingData == null)
            {
                Debug.LogWarning("BuildingManager: 업그레이드 대상 건축 데이터를 찾을 수 없습니다. id=" + upgradeData.ToBuildingId);
                return false;
            }

            if (BuildingCostHelper.CanPayUpgradeCostDifference(fromBuildingData, toBuildingData, playerModel, gameDataManager) == false)
            {
                Debug.LogWarning("BuildingManager: 건축 업그레이드 차액 자원이 부족합니다. from="
                    + fromBuildingId + ", to=" + toBuildingData.Id);
                return false;
            }

            GameObject createdBuilding = CreateBuildingObject(
                buildingSpot.GetBuildingPrefab(toBuildingData),
                toBuildingData,
                buildingSpot.GetBuildPosition(),
                buildingSpot.GetBuildRotation());

            if (createdBuilding == null)
            {
                return false;
            }

            if (BuildingCostHelper.TryPayUpgradeCostDifference(fromBuildingData, toBuildingData, playerModel, gameDataManager) == false)
            {
                DestroyCreatedBuilding(createdBuilding);
                Debug.LogWarning("BuildingManager: 건축 업그레이드 차액 차감에 실패했습니다. to=" + toBuildingData.Id);
                return false;
            }

            GameObject previousBuildingObject = buildingSpot.CurrentBuildingObject;
            BuildingHp previousBuildingHp = previousBuildingObject == null ? null : previousBuildingObject.GetComponent<BuildingHp>();
            UnregisterBuilding(previousBuildingHp);

            if (previousBuildingObject != null)
            {
                Destroy(previousBuildingObject);
            }

            buildingSpot.ReplaceBuilding(createdBuilding, toBuildingData.Id);
            RegisterBuildingComponent(createdBuilding, toBuildingData);
            NotifyBuildingPlaced(toBuildingData);
            Debug.Log("BuildingManager: 건축 업그레이드 완료. from=" + fromBuildingId + ", to=" + toBuildingData.Id);
            return true;
        }

        private bool HasPreparedBuildingItem(BuildingData buildingData)
        {
            if (buildingData == null || GameManager.Inst == null)
            {
                return false;
            }

            PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
            return inventoryModel != null && inventoryModel.HasPreparedBuilding(buildingData.Id);
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

            return inventoryModel.TryConsumePreparedBuilding(buildingData.Id, 1);
        }

        private void DestroyCreatedBuilding(GameObject createdBuilding)
        {
            if (createdBuilding == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(createdBuilding);
                return;
            }

            DestroyImmediate(createdBuilding);
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

        private bool CanUseBuildingSpot(BuildingSpot buildingSpot)
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

            return true;
        }

        private bool CanBuildAtSpot(BuildingSpot buildingSpot)
        {
            if (CanUseBuildingSpot(buildingSpot) == false)
            {
                return false;
            }

            if (buildingSpot.CanBuild() == false)
            {
                Debug.LogWarning("BuildingManager: 이미 건축물이 있는 지점입니다.");
                return false;
            }

            return true;
        }

        private BuildingUpgradeData FindAvailableUpgrade(string fromBuildingId, GameDataManager gameDataManager)
        {
            if (string.IsNullOrEmpty(fromBuildingId) || gameDataManager == null)
            {
                return null;
            }

            int currentDay = GameManager.Inst == null || GameManager.Inst.GameRunModel == null
                ? GameRunModel.FirstDay
                : GameManager.Inst.GameRunModel.Day;

            foreach (BuildingUpgradeData upgradeData in gameDataManager.BuildingUpgradeDataList.Values)
            {
                if (upgradeData == null)
                {
                    continue;
                }

                if (string.Equals(upgradeData.FromBuildingId, fromBuildingId, StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                if (upgradeData.RequiredDay > currentDay)
                {
                    continue;
                }

                return upgradeData;
            }

            return null;
        }

        private GameObject CreateBuildingObject(
            GameObject buildingPrefab,
            BuildingData buildingData,
            Vector3 buildPosition,
            Quaternion buildRotation)
        {
            if (buildingPrefab != null)
            {
                return _gameObjectManager.CreateGameObject(buildingPrefab, buildPosition, buildRotation);
            }

            GameObject fallbackBuilding = CreateRuntimeFallbackBuilding(buildingData, buildPosition, buildRotation);
            if (fallbackBuilding == null)
            {
                return null;
            }

            _gameObjectManager.RegisterGameObject(fallbackBuilding);
            return fallbackBuilding;
        }

        private GameObject CreateRuntimeFallbackBuilding(
            BuildingData buildingData,
            Vector3 buildPosition,
            Quaternion buildRotation)
        {
            if (buildingData == null)
            {
                Debug.LogWarning("BuildingManager: 건축 데이터가 없어 임시 건축물을 만들 수 없습니다.");
                return null;
            }

            PrimitiveType primitiveType = ResolveFallbackPrimitiveType(buildingData);
            GameObject createdBuilding = GameObject.CreatePrimitive(primitiveType);
            createdBuilding.name = "RuntimeFallback_" + buildingData.Id;
            createdBuilding.transform.position = buildPosition;
            createdBuilding.transform.rotation = buildRotation;
            createdBuilding.transform.localScale = ResolveFallbackScale(buildingData);
            ApplyFallbackColor(createdBuilding, buildingData);

            Debug.LogWarning("BuildingManager: 프리팹 연결이 없어 임시 건축물을 생성했습니다. building=" + buildingData.Id);
            return createdBuilding;
        }

        private PrimitiveType ResolveFallbackPrimitiveType(BuildingData buildingData)
        {
            if (IsTowerBuilding(buildingData))
            {
                return PrimitiveType.Cylinder;
            }

            return PrimitiveType.Cube;
        }

        private Vector3 ResolveFallbackScale(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                return Vector3.one;
            }

            if (IsTowerBuilding(buildingData))
            {
                return new Vector3(1.25f, 2.8f, 1.25f);
            }

            if (IsTrapBuilding(buildingData))
            {
                return new Vector3(2.1f, 0.18f, 2.1f);
            }

            if (string.Equals(buildingData.BuildingType, "Station", StringComparison.OrdinalIgnoreCase))
            {
                return new Vector3(1.6f, 1.2f, 1.6f);
            }

            return new Vector3(2.6f, 1.8f, 0.45f);
        }

        private void ApplyFallbackColor(GameObject createdBuilding, BuildingData buildingData)
        {
            Renderer targetRenderer = createdBuilding == null ? null : createdBuilding.GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                return;
            }

            targetRenderer.material.color = ResolveTierColor(buildingData);
        }

        private Color ResolveTierColor(BuildingData buildingData)
        {
            if (buildingData == null)
            {
                return Color.white;
            }

            switch (buildingData.Tier)
            {
                case 2:
                    return new Color(0.45f, 0.58f, 0.68f);
                case 3:
                    return new Color(0.95f, 0.74f, 0.22f);
                case 4:
                    return new Color(0.58f, 0.28f, 0.82f);
                default:
                    return new Color(0.42f, 0.26f, 0.14f);
            }
        }

        private void RegisterBuildingComponent(GameObject buildingObject)
        {
            RegisterBuildingComponent(buildingObject, null);
        }

        private void RegisterBuildingComponent(GameObject buildingObject, BuildingData buildingData)
        {
            // 정식 프리팹에는 BuildingHp를 미리 붙이는 것이 기준입니다.
            // 누락된 팀원 프리팹을 흡수할 때만 fallback으로 붙여 게임 루프가 끊기지 않게 합니다.
            if (buildingObject == null)
            {
                return;
            }

            BuildingHp buildingHp = buildingObject.GetComponent<BuildingHp>();
            if (buildingHp == null)
            {
                buildingHp = buildingObject.AddComponent<BuildingHp>();
            }

            buildingHp.Initialize(buildingData);
            InitializeBuildingRoleComponents(buildingObject, buildingData);

            RegisterBuilding(buildingHp);
        }

        private void InitializeBuildingRoleComponents(GameObject buildingObject, BuildingData buildingData)
        {
            if (buildingObject == null || buildingData == null)
            {
                return;
            }

            if (IsTowerBuilding(buildingData) == false)
            {
                if (IsTrapBuilding(buildingData))
                {
                    EnsureTrapComponents(buildingObject, buildingData);
                }

                return;
            }

            EnsureTowerComponents(buildingObject);
            BuildingTower buildingTower = buildingObject.GetComponent<BuildingTower>();
            if (buildingTower != null)
            {
                buildingTower.Initialize(buildingData);
            }
        }

        private bool IsTowerBuilding(BuildingData buildingData)
        {
            return buildingData != null
                && string.Equals(buildingData.BuildingType, "Tower", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsTrapBuilding(BuildingData buildingData)
        {
            return buildingData != null
                && string.Equals(buildingData.BuildingType, "Trap", StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureTowerComponents(GameObject buildingObject)
        {
            if (buildingObject.GetComponent<TowerTargeting>() == null)
            {
                buildingObject.AddComponent<TowerTargeting>();
            }

            if (buildingObject.GetComponent<TowerAttack>() == null)
            {
                buildingObject.AddComponent<TowerAttack>();
            }

            if (buildingObject.GetComponent<DamageDealer>() == null)
            {
                buildingObject.AddComponent<DamageDealer>();
            }

            if (buildingObject.GetComponent<BuildingTower>() == null)
            {
                buildingObject.AddComponent<BuildingTower>();
            }
        }

        private void EnsureTrapComponents(GameObject buildingObject, BuildingData buildingData)
        {
            Collider collider = buildingObject.GetComponent<Collider>();
            if (collider == null)
            {
                collider = buildingObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;

            if (buildingObject.GetComponent<TrapCrowdControlReporter>() == null)
            {
                buildingObject.AddComponent<TrapCrowdControlReporter>();
            }

            TrapSlowZone trapSlowZone = buildingObject.GetComponent<TrapSlowZone>();
            if (trapSlowZone == null)
            {
                trapSlowZone = buildingObject.AddComponent<TrapSlowZone>();
            }

            trapSlowZone.Initialize(buildingData);
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

        private void UnregisterBuilding(BuildingHp building)
        {
            if (building == null)
            {
                return;
            }

            building.OnBuildingDestroyed -= HandleBuildingDestroyed;
            RemoveBuilding(building);
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

        private void NotifyBuildingPlaced(BuildingData buildingData)
        {
            if (BuildingPlacedGlobal == null)
            {
                return;
            }

            BuildingPlacedGlobal(buildingData);
        }
    }
}
