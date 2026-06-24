using Cinderkeep.Gameplay;
using UnityEngine;

// 바닥에 놓인 작은 손돌 픽업입니다.
// 주우면 Stone 자원 1개와 hand_stone 도구를 퀵슬롯에 등록합니다.
public sealed class HandStonePickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string _toolDataId = PlayerToolController.HandStoneToolDataId;
    [SerializeField] private int _preferredQuickSlotIndex = 0;
    [SerializeField] private int _replacementQuickSlotIndex = PlayerInventoryModel.QuickSlotCount - 1;
    [SerializeField] private int _stoneAmountOnPickup = 1;
    [SerializeField] private bool _equipOnPickup = true;
    [SerializeField] private bool _disableAfterPickup = true;
    [SerializeField] private bool _canInteract = true;

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        if (_canInteract == false)
        {
            return false;
        }

        return gameObjectInteractor != null;
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        if (CanInteract(gameObjectInteractor) == false)
        {
            return;
        }

        if (TryGiveHandStone(gameObjectInteractor) == false)
        {
            return;
        }

        PlayPickupSfx();
        ProcessPickedUp();
    }

    public void ResetPickup()
    {
        _canInteract = true;
        gameObject.SetActive(true);
    }

    private bool TryGiveHandStone(GameObject gameObjectInteractor)
    {
        if (GameManager.Inst == null)
        {
            Debug.LogWarning("HandStonePickup: GameManager is missing.");
            return false;
        }

        PlayerInventoryModel inventoryModel = GameManager.Inst.PlayerInventoryModel;
        if (inventoryModel == null)
        {
            return false;
        }

        bool isSet = inventoryModel.TryAddQuickSlotItem(
            _toolDataId,
            InventoryItemType.Tool,
            1,
            _preferredQuickSlotIndex,
            _replacementQuickSlotIndex,
            out _);

        if (isSet == false)
        {
            return false;
        }

        GameManager.Inst.PlayerModel.AddResource(PlayerModel.ResourceStone, _stoneAmountOnPickup);

        if (_equipOnPickup)
        {
            PlayerToolController toolController = gameObjectInteractor.GetComponentInParent<PlayerToolController>();
            if (toolController != null)
            {
                toolController.EquipToolData(_toolDataId);
            }
        }

        return true;
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

        if (_disableAfterPickup == false)
        {
            return;
        }

        gameObject.SetActive(false);
    }
}
