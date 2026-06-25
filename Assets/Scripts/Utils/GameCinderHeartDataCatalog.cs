using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // CinderHeart 업그레이드와 아침 보상 스킬 데이터를 묶어 관리합니다.
    // 제작형 CinderHeartUpgrade는 비활성 흐름이고, 실제 플레이 보상은 CinderHeartSkillData가 중심입니다.
    public sealed class GameCinderHeartDataCatalog
    {
        private readonly Dictionary<string, CinderHeartUpgradeData> _cinderHeartUpgradeDataList = new Dictionary<string, CinderHeartUpgradeData>();
        private readonly Dictionary<string, CinderHeartSkillData> _cinderHeartSkillDataList = new Dictionary<string, CinderHeartSkillData>();

        public IReadOnlyDictionary<string, CinderHeartUpgradeData> CinderHeartUpgradeDataList
        {
            get { return _cinderHeartUpgradeDataList; }
        }

        public IReadOnlyDictionary<string, CinderHeartSkillData> CinderHeartSkillDataList
        {
            get { return _cinderHeartSkillDataList; }
        }

        public void LoadCinderHeartUpgradeData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CinderHeartUpgradeData, CinderHeartUpgradeDataCatalog>(_cinderHeartUpgradeDataList, resourcePath, "CinderHeart upgrade", catalog => catalog.Items);
        }

        public CinderHeartUpgradeData GetCinderHeartUpgrade(string id)
        {
            return GameDataCatalogLookup.GetById(_cinderHeartUpgradeDataList, id);
        }

        public void LoadCinderHeartSkillData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<CinderHeartSkillData, CinderHeartSkillDataCatalog>(_cinderHeartSkillDataList, resourcePath, "CinderHeart skill", catalog => catalog.Items);
        }

        public CinderHeartSkillData GetCinderHeartSkill(string id)
        {
            return GameDataCatalogLookup.GetById(_cinderHeartSkillDataList, id);
        }
    }
}
