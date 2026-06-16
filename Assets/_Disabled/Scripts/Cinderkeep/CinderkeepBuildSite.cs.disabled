using UnityEngine;

namespace OODong.Cinderkeep
{
    // 고정 건설 지점. 자유 건축 대신 발표용 MVP에서는 지정된 위치에만 건설한다.
    // 사용자는 E로 상호작용하고, 실제 비용/체력 값은 buildings.json에서 가져온다.
    public sealed class CinderkeepBuildSite : MonoBehaviour, ICinderkeepInteractable
    {
        [SerializeField] private string _buildingDataId = "wall";
        [SerializeField] private CinderkeepItemId _fallbackCostItemId = CinderkeepItemId.Stone;
        [SerializeField] private int _fallbackCostAmount = 2;
        [SerializeField] private GameObject GameObject_StructureView;
        [SerializeField] private CinderkeepStructure CinderkeepStructure_Structure;
        [SerializeField] private Renderer Renderer_Preview;

        private bool _isBuilt;

        private void Awake()
        {
            if (CinderkeepStructure_Structure == null && GameObject_StructureView != null)
            {
                CinderkeepStructure_Structure = GameObject_StructureView.GetComponent<CinderkeepStructure>();
            }

            SetStructureVisible(false);
        }

        public string GetPrompt()
        {
            CinderkeepBuildingData data = GameDataManager.Instance?.GetBuildingData(_buildingDataId);
            string displayName = data != null ? data.DisplayName : _buildingDataId;
            CinderkeepItemId costItemId = GetCostItemId(data);
            int costAmount = GetCostAmount(data);

            return _isBuilt
                ? $"[E] Repair {displayName} ({costItemId} x1)"
                : $"[E] Build {displayName} ({costItemId} x{costAmount})";
        }

        public bool CanInteract(CinderkeepFirstPersonPlayer player)
        {
            return player != null;
        }

        public void Interact(CinderkeepFirstPersonPlayer player)
        {
            if (!CanInteract(player))
            {
                return;
            }

            if (_isBuilt)
            {
                Repair(player);
                return;
            }

            Build(player);
        }

        public void SetBuildSite(string buildingDataId, GameObject structureView, CinderkeepStructure structure)
        {
            _buildingDataId = buildingDataId;
            GameObject_StructureView = structureView;
            CinderkeepStructure_Structure = structure;
            SetStructureVisible(false);
        }

        public void SetPreviewRenderer(Renderer previewRenderer)
        {
            Renderer_Preview = previewRenderer;
        }

        private void Build(CinderkeepFirstPersonPlayer player)
        {
            // TODO(팀원 작업 요청): 건설 UI 미리보기/선택지는 UIManagerExtension 쪽에 별도 창으로 추가해 주세요.
            CinderkeepBuildingData data = GameDataManager.Instance?.GetBuildingData(_buildingDataId);
            CinderkeepItemId costItemId = GetCostItemId(data);
            int costAmount = GetCostAmount(data);

            if (player.Inventory == null || !player.Inventory.TryRemoveItem(costItemId, costAmount))
            {
                player.ShowStatus($"{CinderkeepItemCatalog.GetDisplayName(costItemId)} x{costAmount} required");
                return;
            }

            _isBuilt = true;
            SetStructureVisible(true);
            CinderkeepStructure_Structure?.Configure(data);
            GameObjectManager.Instance?.RegisterGameObject(GameObject_StructureView);
            player.ShowStatus($"Built {(data != null ? data.DisplayName : _buildingDataId)}");
        }

        private void Repair(CinderkeepFirstPersonPlayer player)
        {
            if (player.Inventory == null || !player.Inventory.TryRemoveItem(CinderkeepItemId.Stone, 1))
            {
                player.ShowStatus("Stone x1 required to repair");
                return;
            }

            CinderkeepStructure_Structure?.Repair(10);
            player.ShowStatus("Structure repaired");
        }

        private void SetStructureVisible(bool isVisible)
        {
            if (GameObject_StructureView != null)
            {
                GameObject_StructureView.SetActive(isVisible);
            }

            if (Renderer_Preview != null)
            {
                Renderer_Preview.enabled = !isVisible;
            }
        }

        private CinderkeepItemId GetCostItemId(CinderkeepBuildingData data)
        {
            return data != null ? GameUtil.ParseItemId(data.CostItemId, _fallbackCostItemId) : _fallbackCostItemId;
        }

        private int GetCostAmount(CinderkeepBuildingData data)
        {
            return data != null ? data.CostAmount : Mathf.Max(1, _fallbackCostAmount);
        }
    }
}
