using System.Collections.Generic;

namespace Cinderkeep.Gameplay
{
    // 낮/밤 흐름과 웨이브 스폰 규칙처럼 게임 진행 데이터를 묶어 관리합니다.
    // GameFlowController와 EnemySpawnPoint는 GameDataManager 허브를 통해 이 데이터를 조회합니다.
    public sealed class GameFlowDataCatalog
    {
        private readonly Dictionary<string, EnemySpawnRuleData> _enemySpawnRuleDataList = new Dictionary<string, EnemySpawnRuleData>();
        private readonly Dictionary<string, GameFlowPhaseData> _gameFlowPhaseDataList = new Dictionary<string, GameFlowPhaseData>();

        public IReadOnlyDictionary<string, EnemySpawnRuleData> EnemySpawnRuleDataList
        {
            get { return _enemySpawnRuleDataList; }
        }

        public IReadOnlyDictionary<string, GameFlowPhaseData> GameFlowPhaseDataList
        {
            get { return _gameFlowPhaseDataList; }
        }

        public void LoadEnemySpawnRuleData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<EnemySpawnRuleData, EnemySpawnRuleDataCatalog>(_enemySpawnRuleDataList, resourcePath, "enemy spawn rule", catalog => catalog.Items);
        }

        public EnemySpawnRuleData GetEnemySpawnRule(string id)
        {
            return GameDataCatalogLookup.GetById(_enemySpawnRuleDataList, id);
        }

        public void LoadGameFlowPhaseData(string resourcePath)
        {
            GameDataCatalogLoader.LoadCatalog<GameFlowPhaseData, GameFlowPhaseDataCatalog>(_gameFlowPhaseDataList, resourcePath, "game flow phase", catalog => catalog.Items);
        }

        public GameFlowPhaseData GetGameFlowPhase(string id)
        {
            return GameDataCatalogLookup.GetById(_gameFlowPhaseDataList, id);
        }
    }
}
