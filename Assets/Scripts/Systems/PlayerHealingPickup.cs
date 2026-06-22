using UnityEngine;

public sealed class PlayerHealingPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private float _healAmount = 25f;
    [SerializeField] private bool _disableAfterPickup = true;

    private bool _isCollected;

    private void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider == null)
        {
            return;
        }

        TryCollect(otherCollider.gameObject);
    }

    public bool CanInteract(GameObject gameObjectInteractor)
    {
        if (_isCollected == true)
        {
            return false;
        }

        return GetPlayerStatus(gameObjectInteractor) != null;
    }

    public void Interact(GameObject gameObjectInteractor)
    {
        TryCollect(gameObjectInteractor);
    }

    public void ResetPickup()
    {
        _isCollected = false;
        gameObject.SetActive(true);
    }

    private void TryCollect(GameObject targetObject)
    {
        if (_isCollected == true)
        {
            return;
        }

        PlayerStatus playerStatus = GetPlayerStatus(targetObject);
        if (playerStatus == null)
        {
            return;
        }

        float healedAmount = playerStatus.Heal(_healAmount);
        if (healedAmount <= 0f)
        {
            return;
        }

        _isCollected = true;
        EmeraldQuestEventHub.RaiseHealingPickupCollected();
        Debug.Log("[PlayerHealingPickup] Player healed by pickup: " + healedAmount);

        if (_disableAfterPickup == true)
        {
            gameObject.SetActive(false);
        }
    }

    private PlayerStatus GetPlayerStatus(GameObject targetObject)
    {
        if (targetObject == null)
        {
            return null;
        }

        PlayerStatus playerStatus = targetObject.GetComponent<PlayerStatus>();
        if (playerStatus != null)
        {
            return playerStatus;
        }

        return targetObject.GetComponentInParent<PlayerStatus>();
    }
}
