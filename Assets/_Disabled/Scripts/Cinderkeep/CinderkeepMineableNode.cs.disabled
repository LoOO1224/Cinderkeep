using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepMineableNode : MonoBehaviour, ICinderkeepInteractable
    {
        [SerializeField] private string _displayName = "Ore";
        [SerializeField] private int _requiredHits = 3;
        [SerializeField] private CinderkeepItemId _yieldItemId = CinderkeepItemId.Ore;
        [SerializeField] private int _yieldAmount = 1;
        [SerializeField] private Renderer Renderer_Target;

        private int _hitsRemaining;
        private Color _defaultColor;

        private void Awake()
        {
            if (Renderer_Target == null)
            {
                Renderer_Target = GetComponentInChildren<Renderer>();
            }

            if (Renderer_Target != null)
            {
                _defaultColor = Renderer_Target.sharedMaterial.color;
            }
        }

        private void OnEnable()
        {
            _hitsRemaining = Mathf.Max(1, _requiredHits);
            RefreshView();
        }

        public string GetPrompt()
        {
            return $"[E] Mine {_displayName}";
        }

        public bool CanInteract(CinderkeepFirstPersonPlayer player)
        {
            return player != null && _hitsRemaining > 0;
        }

        public void Interact(CinderkeepFirstPersonPlayer player)
        {
            if (!CanInteract(player))
            {
                return;
            }

            if (!player.HasItem(CinderkeepItemId.Pickaxe))
            {
                player.ShowStatus("Pickaxe required");
                return;
            }

            _hitsRemaining--;
            player.PlayPickaxeSwing();
            player.ShowMiningProgress(1f - ((float)_hitsRemaining / Mathf.Max(1, _requiredHits)));

            if (_hitsRemaining <= 0)
            {
                player.AddItem(_yieldItemId, _yieldAmount);
                player.ShowStatus($"Mined {_displayName}: +{_yieldAmount} {CinderkeepItemCatalog.GetDisplayName(_yieldItemId)}");
                gameObject.SetActive(false);
                return;
            }

            player.ShowStatus($"{_displayName} hit {_requiredHits - _hitsRemaining}/{_requiredHits}");
            RefreshView();
        }

        public void SetMineable(string displayName, int requiredHits, CinderkeepItemId yieldItemId, int yieldAmount)
        {
            _displayName = displayName;
            _requiredHits = Mathf.Max(1, requiredHits);
            _yieldItemId = yieldItemId;
            _yieldAmount = Mathf.Max(1, yieldAmount);
            _hitsRemaining = _requiredHits;
        }

        private void RefreshView()
        {
            if (Renderer_Target == null)
            {
                return;
            }

            float damageRate = 1f - ((float)_hitsRemaining / Mathf.Max(1, _requiredHits));
            Color targetColor = Color.Lerp(_defaultColor, Color.gray, damageRate);
            Renderer_Target.material.color = targetColor;
        }
    }
}
