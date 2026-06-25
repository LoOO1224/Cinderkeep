using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 제작, 제련, 건축, 건축 업그레이드 데이터를 묶어 관리하는 GameDataManager 내부 카탈로그입니다.
    // 제작 결과 타입 확장과 건축물 단계 추가가 GameDataManager 본문을 계속 키우지 않게 합니다.
    public sealed class GameCraftingDataCatalog
    {
        private readonly Dictionary<string, BuildingData> _buildingDataList = new Dictionary<string, BuildingData>();
        private readonly Dictionary<string, CraftingRecipeData> _craftingRecipeDataList = new Dictionary<string, CraftingRecipeData>();
        private readonly Dictionary<string, CraftingStationData> _craftingStationDataList = new Dictionary<string, CraftingStationData>();
        private readonly Dictionary<string, SmeltingRecipeData> _smeltingRecipeDataList = new Dictionary<string, SmeltingRecipeData>();
        private readonly Dictionary<string, BuildingUpgradeData> _buildingUpgradeDataList = new Dictionary<string, BuildingUpgradeData>();

        public IReadOnlyDictionary<string, BuildingData> BuildingDataList
        {
            get { return _buildingDataList; }
        }

        public IReadOnlyDictionary<string, CraftingRecipeData> CraftingRecipeDataList
        {
            get { return _craftingRecipeDataList; }
        }

        public IReadOnlyDictionary<string, CraftingStationData> CraftingStationDataList
        {
            get { return _craftingStationDataList; }
        }

        public IReadOnlyDictionary<string, SmeltingRecipeData> SmeltingRecipeDataList
        {
            get { return _smeltingRecipeDataList; }
        }

        public IReadOnlyDictionary<string, BuildingUpgradeData> BuildingUpgradeDataList
        {
            get { return _buildingUpgradeDataList; }
        }

        public void LoadBuildingData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BuildingData, BuildingDataCatalog>(_buildingDataList, resourcePath, "building", catalog => catalog.Items);
        }

        public BuildingData GetBuilding(string id)
        {
            return GameDataCatalogLookup.GetById(_buildingDataList, id);
        }

        public void LoadCraftingRecipeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CraftingRecipeData, CraftingRecipeDataCatalog>(_craftingRecipeDataList, resourcePath, "crafting recipe", catalog => catalog.Items);
        }

        public CraftingRecipeData GetCraftingRecipe(string id)
        {
            return GameDataCatalogLookup.GetById(_craftingRecipeDataList, id);
        }

        public void LoadCraftingStationData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CraftingStationData, CraftingStationDataCatalog>(_craftingStationDataList, resourcePath, "crafting station", catalog => catalog.Items);
        }

        public CraftingStationData GetCraftingStation(string id)
        {
            return GameDataCatalogLookup.GetById(_craftingStationDataList, id);
        }

        public void LoadSmeltingRecipeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<SmeltingRecipeData, SmeltingRecipeDataCatalog>(_smeltingRecipeDataList, resourcePath, "smelting recipe", catalog => catalog.Items);
        }

        public SmeltingRecipeData GetSmeltingRecipe(string id)
        {
            return GameDataCatalogLookup.GetById(_smeltingRecipeDataList, id);
        }

        public void LoadBuildingUpgradeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<BuildingUpgradeData, BuildingUpgradeDataCatalog>(_buildingUpgradeDataList, resourcePath, "building upgrade", catalog => catalog.Items);
        }

        public BuildingUpgradeData GetBuildingUpgrade(string id)
        {
            return GameDataCatalogLookup.GetById(_buildingUpgradeDataList, id);
        }
    }
}
