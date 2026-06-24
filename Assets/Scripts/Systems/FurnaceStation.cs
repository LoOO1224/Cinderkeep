using System;
using System.Collections.Generic;
using UnityEngine;

// 5.00 direction: Runs one concrete gameplay system in the 5.00 closed loop.
// 5.01+ note: Keep the class focused on one responsibility and expose simple events or methods for cross-system links.
namespace Cinderkeep.Gameplay
{
    // 용광로 전용 컴포넌트입니다.
    // 왼쪽 입력 슬롯, 진행률, 오른쪽 결과 슬롯 흐름을 코드로 표현합니다.
    public sealed class FurnaceStation : MonoBehaviour
    {
        [Header("Connected Station")]
        [Tooltip("이 용광로가 사용하는 제작 시설 기준 컴포넌트입니다.")]
        [SerializeField] private CraftingStation _craftingStation;

        private readonly List<SmeltingRecipeData> _availableRecipes = new List<SmeltingRecipeData>();
        private SmeltingRecipeData _currentRecipeData;
        private string _currentInputResourceId = "";
        private string _currentOutputResourceId = "";
        private int _currentInputAmount;
        private int _queuedOutputAmount;
        private int _readyOutputAmount;
        private float _smeltTimer;
        private float _smeltDuration;
        private bool _isSmelting;

        public event Action OnFurnaceStateChanged;

        public bool IsSmelting
        {
            get
            {
                return _isSmelting;
            }
        }

        public string CurrentInputResourceId
        {
            get
            {
                return _currentInputResourceId;
            }
        }

        public string CurrentOutputResourceId
        {
            get
            {
                return _currentOutputResourceId;
            }
        }

        public int CurrentInputAmount
        {
            get
            {
                return _currentInputAmount;
            }
        }

        public int ReadyOutputAmount
        {
            get
            {
                return _readyOutputAmount;
            }
        }

        private void Start()
        {
            ConnectComponents();
        }

        private void Update()
        {
            TickSmelting();
        }

        public float GetProgress01()
        {
            if (_smeltDuration <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_smeltTimer / _smeltDuration);
        }

        public void GetAvailableSmeltingRecipes(GameDataManager gameDataManager, List<SmeltingRecipeData> resultRecipes)
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

            _availableRecipes.Clear();
            foreach (KeyValuePair<string, SmeltingRecipeData> dataPair in gameDataManager.SmeltingRecipeDataList)
            {
                SmeltingRecipeData recipeData = dataPair.Value;
                if (CanUseSmeltingRecipe(recipeData))
                {
                    _availableRecipes.Add(recipeData);
                }
            }

            for (int i = 0; i < _availableRecipes.Count; i++)
            {
                resultRecipes.Add(_availableRecipes[i]);
            }
        }

        public bool TryStartSmeltingByInputResource(string inputResourceId, int inputAmount, PlayerModel playerModel)
        {
            ConnectComponents();

            if (CanStartNewSmelting(inputAmount, playerModel) == false)
            {
                return false;
            }

            SmeltingRecipeData recipeData = GetRecipeByInputResource(inputResourceId);
            if (recipeData == null)
            {
                return false;
            }

            return TryStartSmelting(recipeData, inputAmount, playerModel);
        }

        public bool TryStartSmelting(SmeltingRecipeData recipeData, int inputAmount, PlayerModel playerModel)
        {
            ConnectComponents();

            if (CanStartNewSmelting(inputAmount, playerModel) == false)
            {
                return false;
            }

            if (CanUseSmeltingRecipe(recipeData) == false)
            {
                return false;
            }

            int batchCount = GetBatchCount(recipeData, inputAmount);
            if (batchCount <= 0)
            {
                return false;
            }

            int requiredInputAmount = recipeData.InputAmount * batchCount;
            if (playerModel.UseResource(recipeData.InputResourceId, requiredInputAmount) == false)
            {
                return false;
            }

            StartSmelting(recipeData, requiredInputAmount, batchCount);
            return true;
        }

        public bool TryCollectOutput(PlayerModel playerModel)
        {
            if (playerModel == null)
            {
                return false;
            }

            if (_readyOutputAmount <= 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_currentOutputResourceId))
            {
                return false;
            }

            playerModel.AddResource(_currentOutputResourceId, _readyOutputAmount);
            _readyOutputAmount = 0;
            NotifyStateChanged();
            return true;
        }

        public void ResetFurnaceState()
        {
            _currentRecipeData = null;
            _currentInputResourceId = "";
            _currentOutputResourceId = "";
            _currentInputAmount = 0;
            _queuedOutputAmount = 0;
            _readyOutputAmount = 0;
            _smeltTimer = 0f;
            _smeltDuration = 0f;
            _isSmelting = false;
            NotifyStateChanged();
        }

        private void StartSmelting(SmeltingRecipeData recipeData, int inputAmount, int batchCount)
        {
            _currentRecipeData = recipeData;
            _currentInputResourceId = recipeData.InputResourceId;
            _currentOutputResourceId = recipeData.OutputResourceId;
            _currentInputAmount = inputAmount;
            _queuedOutputAmount = recipeData.OutputAmount * batchCount;
            _readyOutputAmount = 0;
            _smeltTimer = 0f;
            _smeltDuration = Mathf.Max(0.1f, recipeData.SmeltSeconds * batchCount);
            _isSmelting = true;
            NotifyStateChanged();
        }

        private void TickSmelting()
        {
            if (_isSmelting == false)
            {
                return;
            }

            _smeltTimer += Time.deltaTime;
            if (_smeltTimer < _smeltDuration)
            {
                NotifyStateChanged();
                return;
            }

            CompleteSmelting();
        }

        private void CompleteSmelting()
        {
            _isSmelting = false;
            _smeltTimer = _smeltDuration;
            _readyOutputAmount += _queuedOutputAmount;
            _currentInputAmount = 0;
            _queuedOutputAmount = 0;
            NotifyStateChanged();
        }

        private bool CanStartNewSmelting(int inputAmount, PlayerModel playerModel)
        {
            if (_isSmelting)
            {
                return false;
            }

            if (inputAmount <= 0)
            {
                return false;
            }

            if (playerModel == null)
            {
                return false;
            }

            if (GameManager.Inst == null || GameManager.Inst.GetGameDataManager() == null)
            {
                return false;
            }

            return true;
        }

        private bool CanUseSmeltingRecipe(SmeltingRecipeData recipeData)
        {
            if (recipeData == null)
            {
                return false;
            }

            if (_craftingStation == null)
            {
                return false;
            }

            if (string.Equals(recipeData.StationType, _craftingStation.StationType, StringComparison.OrdinalIgnoreCase) == false)
            {
                return false;
            }

            if (recipeData.RequiredStationTier > _craftingStation.StationTier)
            {
                return false;
            }

            return true;
        }

        private SmeltingRecipeData GetRecipeByInputResource(string inputResourceId)
        {
            if (string.IsNullOrEmpty(inputResourceId))
            {
                return null;
            }

            GameDataManager gameDataManager = GameManager.Inst.GetGameDataManager();
            foreach (KeyValuePair<string, SmeltingRecipeData> dataPair in gameDataManager.SmeltingRecipeDataList)
            {
                SmeltingRecipeData recipeData = dataPair.Value;
                if (recipeData == null)
                {
                    continue;
                }

                if (string.Equals(recipeData.InputResourceId, inputResourceId, StringComparison.OrdinalIgnoreCase))
                {
                    return recipeData;
                }
            }

            return null;
        }

        private int GetBatchCount(SmeltingRecipeData recipeData, int inputAmount)
        {
            if (recipeData == null || recipeData.InputAmount <= 0)
            {
                return 0;
            }

            return inputAmount / recipeData.InputAmount;
        }

        private void ConnectComponents()
        {
            if (_craftingStation == null)
            {
                _craftingStation = GetComponent<CraftingStation>();
            }
        }

        private void NotifyStateChanged()
        {
            if (OnFurnaceStateChanged == null)
            {
                return;
            }

            OnFurnaceStateChanged.Invoke();
        }
    }
}
