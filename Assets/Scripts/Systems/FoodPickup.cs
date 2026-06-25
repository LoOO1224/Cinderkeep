using System;
using Cinderkeep.Gameplay;
using UnityEngine;

// 바닥에 놓인 음식 픽업입니다.
// 획득한 음식은 인벤토리에 넣은 뒤 가능하면 4번 퀵슬롯으로 옮겨 바로 먹을 수 있게 합니다.
public sealed class FoodPickup : MonoBehaviour, IInteractable
{
    public static event Action<string, int> FoodPickedUpGlobal;

    [SerializeField] private string _foodItemId = FoodItemIds.RawMeat;
    [SerializeField] private int _amount = 1;
    [SerializeField] private bool _disableAfterPickup = true;
    [SerializeField] private bool _canInteract = true;

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        return _canInteract && gameObjectInteractor != null;
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        if (CanInteract(gameObjectInteractor) == false)
        {
            return;
        }

        if (TryGiveFood() == false)
        {
            return;
        }

        PlayPickupSfx();
        NotifyFoodPickedUp();
        ProcessPickedUp();
    }

    public void ResetPickup()
    {
        _canInteract = true;
        gameObject.SetActive(true);
    }

    private bool TryGiveFood()
    {
        if (GameManager.Inst == null)
        {
            return false;
        }

        PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
        if (inventoryModel == null)
        {
            return false;
        }

        if (inventoryModel.TryAddItem(_foodItemId, InventoryItemType.Food, _amount) == false)
        {
            return false;
        }

        TryMovePickedFoodToQuickSlot(inventoryModel);
        return true;
    }

    private void TryMovePickedFoodToQuickSlot(PlayerInventoryModel inventoryModel)
    {
        if (inventoryModel == null)
        {
            return;
        }

        int quickSlotIndex;
        bool isQuickSlotAssigned = inventoryModel.TryAddQuickSlotItem(
            _foodItemId,
            InventoryItemType.Food,
            _amount,
            QuickSlotAssignmentPolicy.FoodPreferredSlotIndex,
            QuickSlotAssignmentPolicy.ReplacementSlotIndex,
            out quickSlotIndex);

        if (isQuickSlotAssigned == false)
        {
            return;
        }

        inventoryModel.TryConsumeItem(_foodItemId, _amount);
    }

    private void NotifyFoodPickedUp()
    {
        if (FoodPickedUpGlobal == null)
        {
            return;
        }

        FoodPickedUpGlobal(_foodItemId, _amount);
    }

    private void PlayPickupSfx()
    {
        if (GameManager.Inst == null || GameManager.Inst.GetSoundManager() == null)
        {
            return;
        }

        GameManager.Inst.GetSoundManager().PlayResourcePickup();
    }

    private void ProcessPickedUp()
    {
        _canInteract = false;
        if (_disableAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }
}
