using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// CinderHeart 근처에서 인벤토리/퀵슬롯의 생고기를 익힌 고기로 바꾸는 조리 컴포넌트입니다.
// 음식 종류가 늘어나면 Food 데이터 카탈로그로 확장하되, 현재는 고기 루프를 안정적으로 닫습니다.
public sealed class CinderHeartFoodCooker : MonoBehaviour
{
    public static event Action<int> FoodCookedGlobal;

    [SerializeField] private float _cookRadius = 7f;
    [SerializeField] private float _cookSeconds = 5f;

    private CinderHeart _cinderHeart;
    private PlayerStatus _playerStatus;
    private float _cookProgressSeconds;

    public float CookProgress01
    {
        get
        {
            if (_cookSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(_cookProgressSeconds / _cookSeconds);
        }
    }

    public static void EnsureSceneCooker()
    {
        CinderHeart cinderHeart = UnityEngine.Object.FindFirstObjectByType<CinderHeart>();
        if (cinderHeart == null)
        {
            return;
        }

        CinderHeartFoodCooker cooker = cinderHeart.GetComponent<CinderHeartFoodCooker>();
        if (cooker == null)
        {
            cooker = cinderHeart.gameObject.AddComponent<CinderHeartFoodCooker>();
        }

        cooker.ConnectTargets();
    }

    private void Awake()
    {
        ConnectTargets();
    }

    private void Update()
    {
        ConnectTargets();
        UpdateCooking();
    }

    private void ConnectTargets()
    {
        if (_cinderHeart == null)
        {
            _cinderHeart = GetComponent<CinderHeart>();
        }

        if (_playerStatus == null)
        {
            _playerStatus = UnityEngine.Object.FindFirstObjectByType<PlayerStatus>();
        }
    }

    private void UpdateCooking()
    {
        PlayerInventoryModel inventoryModel = GetInventoryModel();
        if (_cinderHeart == null || _playerStatus == null || inventoryModel == null)
        {
            ResetProgress();
            return;
        }

        if (_cinderHeart.IsDestroyed || _playerStatus.IsDead())
        {
            ResetProgress();
            return;
        }

        if (inventoryModel.HasItemInInventoryOrQuickSlot(FoodItemIds.RawMeat, InventoryItemType.Food) == false)
        {
            ResetProgress();
            return;
        }

        float distanceSqr = (_playerStatus.transform.position - _cinderHeart.transform.position).sqrMagnitude;
        if (distanceSqr > _cookRadius * _cookRadius)
        {
            ResetProgress();
            return;
        }

        _cookProgressSeconds += Time.deltaTime;
        if (_cookProgressSeconds < _cookSeconds)
        {
            return;
        }

        int cookedAmount = inventoryModel.GetItemAmountInInventoryOrQuickSlot(FoodItemIds.RawMeat, InventoryItemType.Food);
        if (inventoryModel.TryReplaceItemIdEverywhere(FoodItemIds.RawMeat, FoodItemIds.CookedMeat, InventoryItemType.Food))
        {
            NotifyFoodCooked(cookedAmount);
            global::CinderkeepLog.Verbose("[CinderHeartFoodCooker] 생고기를 익힌 고기로 조리했습니다. amount=" + cookedAmount);
        }

        ResetProgress();
    }

    private PlayerInventoryModel GetInventoryModel()
    {
        if (GameManager.Inst == null)
        {
            return null;
        }

        return GameManager.Inst.PlayerInventoryModel;
    }

    private void NotifyFoodCooked(int amount)
    {
        if (FoodCookedGlobal == null || amount <= 0)
        {
            return;
        }

        FoodCookedGlobal(amount);
    }

    private void ResetProgress()
    {
        _cookProgressSeconds = 0f;
    }
}
