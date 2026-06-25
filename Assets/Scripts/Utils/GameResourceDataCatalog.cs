using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 자원, 채집 노드, 도구 데이터를 묶어 관리하는 GameDataManager 내부 카탈로그입니다.
    // 외부 시스템은 계속 GameDataManager를 통해 접근하고, 실제 Dictionary 소유권만 이 클래스로 분리합니다.
    public sealed class GameResourceDataCatalog
    {
        private readonly Dictionary<string, ResourceData> _resourceDataList = new Dictionary<string, ResourceData>();
        private readonly Dictionary<string, HarvestNodeData> _harvestNodeDataList = new Dictionary<string, HarvestNodeData>();
        private readonly Dictionary<string, ToolData> _toolDataList = new Dictionary<string, ToolData>();

        public IReadOnlyDictionary<string, ResourceData> ResourceDataList
        {
            get { return _resourceDataList; }
        }

        public IReadOnlyDictionary<string, HarvestNodeData> HarvestNodeDataList
        {
            get { return _harvestNodeDataList; }
        }

        public IReadOnlyDictionary<string, ToolData> ToolDataList
        {
            get { return _toolDataList; }
        }

        public void LoadResourceData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ResourceData, ResourceDataCatalog>(_resourceDataList, resourcePath, "resource", catalog => catalog.Items);
        }

        public ResourceData GetResource(string id)
        {
            return GameDataCatalogLookup.GetById(_resourceDataList, id);
        }

        public void LoadHarvestNodeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<HarvestNodeData, HarvestNodeDataCatalog>(_harvestNodeDataList, resourcePath, "harvest node", catalog => catalog.Items);
        }

        public HarvestNodeData GetHarvestNode(string id)
        {
            return GameDataCatalogLookup.GetById(_harvestNodeDataList, id);
        }

        public void LoadToolData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<ToolData, ToolDataCatalog>(_toolDataList, resourcePath, "tool", catalog => catalog.Items);
        }

        public ToolData GetTool(string id)
        {
            return GameDataCatalogLookup.GetById(_toolDataList, id);
        }
    }
}
