using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepPickupNode : MonoBehaviour, ICinderkeepInteractable
    {
        [SerializeField] private CinderkeepItemId _itemId = CinderkeepItemId.Stone;
        [SerializeField] private int _amount = 1;
        [SerializeField] private string _displayName = "Stone";

        public string GetPrompt()
        {
            return $"[E] Pick up {_displayName}";
        }

        public bool CanInteract(CinderkeepFirstPersonPlayer player)
        {
            return player != null && _itemId != CinderkeepItemId.None && _amount > 0;
        }

        public void Interact(CinderkeepFirstPersonPlayer player)
        {
            if (!CanInteract(player))
            {
                return;
            }

            player.AddItem(_itemId, _amount);
            player.ShowStatus($"+{_amount} {CinderkeepItemCatalog.GetDisplayName(_itemId)}");
            gameObject.SetActive(false);
        }

        public void SetPickup(CinderkeepItemId itemId, int amount, string displayName)
        {
            _itemId = itemId;
            _amount = amount;
            _displayName = displayName;
        }
    }
}
