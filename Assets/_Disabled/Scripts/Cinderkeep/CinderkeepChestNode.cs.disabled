using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepChestNode : MonoBehaviour, ICinderkeepInteractable
    {
        [SerializeField] private CinderkeepItemId _rewardItemId = CinderkeepItemId.Apple;
        [SerializeField] private int _rewardAmount = 1;
        [SerializeField] private Renderer Renderer_Target;

        private bool _isOpened;

        private void Awake()
        {
            if (Renderer_Target == null)
            {
                Renderer_Target = GetComponentInChildren<Renderer>();
            }
        }

        public string GetPrompt()
        {
            return "[E] Open Chest";
        }

        public bool CanInteract(CinderkeepFirstPersonPlayer player)
        {
            return player != null && !_isOpened;
        }

        public void Interact(CinderkeepFirstPersonPlayer player)
        {
            if (!CanInteract(player))
            {
                return;
            }

            _isOpened = true;
            player.AddItem(_rewardItemId, _rewardAmount);
            player.ShowStatus($"Chest opened: +{_rewardAmount} {CinderkeepItemCatalog.GetDisplayName(_rewardItemId)}");

            if (Renderer_Target != null)
            {
                Renderer_Target.material.color = new Color(0.45f, 0.38f, 0.22f, 1f);
            }
        }

        public void SetReward(CinderkeepItemId rewardItemId, int rewardAmount)
        {
            _rewardItemId = rewardItemId;
            _rewardAmount = Mathf.Max(1, rewardAmount);
        }
    }
}
