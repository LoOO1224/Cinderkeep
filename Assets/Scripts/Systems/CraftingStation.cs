using System;
using System.Collections.Generic;
using UnityEngine;

// 닫힌 플레이 루프 안에서 하나의 구체적인 게임플레이 기능을 실행합니다.
// 클래스 책임은 좁게 유지하고, 다른 시스템과는 명확한 메서드나 이벤트로만 연결합니다.
namespace Cinderkeep.Gameplay
{
    // 제작대, 용광로, 모루 같은 제작 시설의 기본 컴포넌트입니다.
    // 이 클래스는 "이 시설에서 어떤 제작법을 열 수 있는지"만 판단합니다.
    public sealed class CraftingStation : MonoBehaviour
    {
        [Header("Station Data")]
        [Tooltip("crafting_stations.json의 _id입니다.")]
        [SerializeField] private string _stationDataId = "workbench_tier_1";
        [Tooltip("true이면 crafting_stations.json 값을 우선 사용합니다.")]
        [SerializeField] private bool _useStationData = true;
        [Tooltip("제작 시설 타입입니다. 예: Workbench, Furnace, Anvil")]
        [SerializeField] private string _stationType = "Workbench";
        [Tooltip("제작 시설 티어입니다. 제작법의 RequiredStationTier와 비교합니다.")]
        [SerializeField] private int _stationTier = 1;
        [Tooltip("현재 이 제작 시설을 사용할 수 있는지 결정합니다.")]
        [SerializeField] private bool _canOpen = true;

        private CraftingStationData _stationData;
        private GameObject _currentInteractor;
        private bool _hasAppliedStationData;
        private bool _hasWarnedMissingStationData;

        public event Action<CraftingStation, GameObject> OnStationOpened;

        public string StationDataId
        {
            get
            {
                return _stationDataId;
            }
        }

        public string StationType
        {
            get
            {
                ApplyStationDataIfPossible();
                return _stationType;
            }
        }

        public int StationTier
        {
            get
            {
                ApplyStationDataIfPossible();
                return _stationTier;
            }
        }

        public GameObject CurrentInteractor
        {
            get
            {
                return _currentInteractor;
            }
        }

        public bool CanOpen(GameObject interactor)
        {
            ApplyStationDataIfPossible();

            if (_canOpen == false)
            {
                return false;
            }

            return interactor != null;
        }

        public void OpenStation(GameObject interactor)
        {
            if (CanOpen(interactor) == false)
            {
                return;
            }

            _currentInteractor = interactor;
            NotifyStationOpened(interactor);
            global::CinderkeepLog.Verbose("CraftingStation: " + StationType + " 제작 시설을 열었습니다.");
        }

        public void GetAvailableRecipes(GameDataManager gameDataManager, int currentDay, List<CraftingRecipeData> resultRecipes)
        {
            if (resultRecipes == null)
            {
                return;
            }

            resultRecipes.Clear();
            if (gameDataManager == null)
            {
                return;
            }

            foreach (KeyValuePair<string, CraftingRecipeData> dataPair in gameDataManager.CraftingRecipeDataList)
            {
                CraftingRecipeData recipeData = dataPair.Value;
                if (CanUseRecipe(recipeData, currentDay))
                {
                    resultRecipes.Add(recipeData);
                }
            }
        }

        public bool CanUseRecipe(CraftingRecipeData recipeData, int currentDay)
        {
            ApplyStationDataIfPossible();

            if (recipeData == null)
            {
                return false;
            }

            if (IsSameStationType(recipeData.StationType) == false)
            {
                return false;
            }

            if (recipeData.RequiredStationTier > _stationTier)
            {
                return false;
            }

            if (recipeData.UnlockDay > currentDay)
            {
                return false;
            }

            return true;
        }

        public CraftingStationData GetStationData()
        {
            ApplyStationDataIfPossible();
            return _stationData;
        }

        private void NotifyStationOpened(GameObject interactor)
        {
            if (OnStationOpened == null)
            {
                return;
            }

            OnStationOpened.Invoke(this, interactor);
        }

        private bool IsSameStationType(string recipeStationType)
        {
            if (string.IsNullOrEmpty(recipeStationType))
            {
                return false;
            }

            return string.Equals(recipeStationType, _stationType, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyStationDataIfPossible()
        {
            if (_useStationData == false)
            {
                return;
            }

            if (_hasAppliedStationData)
            {
                return;
            }

            if (GameManager.Inst == null)
            {
                return;
            }

            GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
            if (gameDataManager == null)
            {
                return;
            }

            CraftingStationData stationData = gameDataManager.GetCraftingStation(_stationDataId);
            if (stationData == null)
            {
                WarnMissingStationData();
                return;
            }

            ApplyStationData(stationData);
        }

        private void ApplyStationData(CraftingStationData stationData)
        {
            _stationData = stationData;

            if (string.IsNullOrEmpty(stationData.StationType) == false)
            {
                _stationType = stationData.StationType;
            }

            if (stationData.Tier > 0)
            {
                _stationTier = stationData.Tier;
            }

            _hasAppliedStationData = true;
        }

        private void WarnMissingStationData()
        {
            if (_hasWarnedMissingStationData)
            {
                return;
            }

            if (string.IsNullOrEmpty(_stationDataId))
            {
                return;
            }

            _hasWarnedMissingStationData = true;
            Debug.LogWarning("CraftingStation: station data was not found. fallback values will be used. id=" + _stationDataId);
        }
    }
}
