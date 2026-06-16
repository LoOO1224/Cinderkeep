using UnityEngine;

namespace OODong.Cinderkeep
{
    public sealed class CinderkeepStructure : MonoBehaviour
    {
        [SerializeField] private string _buildingDataId = "wall";
        [SerializeField] private int _maxHealth = 20;
        [SerializeField] private int _currentHealth = 20;
        [SerializeField] private Renderer Renderer_View;

        public string BuildingDataId => _buildingDataId;
        public bool IsBroken => _currentHealth <= 0;

        private void Awake()
        {
            if (Renderer_View == null)
            {
                Renderer_View = GetComponentInChildren<Renderer>();
            }

            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
            RefreshView();
        }

        public void Configure(CinderkeepBuildingData buildingData)
        {
            if (buildingData == null)
            {
                return;
            }

            _buildingDataId = buildingData.Id;
            _maxHealth = buildingData.MaxHealth;
            _currentHealth = _maxHealth;
            RefreshView();
        }

        public void Repair(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            RefreshView();
        }

        public void TakeDamage(int damage)
        {
            if (damage <= 0 || IsBroken)
            {
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            RefreshView();
        }

        public void SetViewRenderer(Renderer viewRenderer)
        {
            Renderer_View = viewRenderer;
            RefreshView();
        }

        private void RefreshView()
        {
            if (Renderer_View == null || _maxHealth <= 0)
            {
                return;
            }

            float healthRate = Mathf.Clamp01((float)_currentHealth / _maxHealth);
            Material material = Application.isPlaying ? Renderer_View.material : Renderer_View.sharedMaterial;
            if (material != null)
            {
                material.color = Color.Lerp(new Color(0.25f, 0.12f, 0.09f, 1f), new Color(0.8f, 0.72f, 0.58f, 1f), healthRate);
            }
        }
    }
}
