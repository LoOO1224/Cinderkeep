using UnityEngine;

namespace OODong.Cinderkeep
{
    // 밤 웨이브와 보스 생성을 담당하는 Spawner.
    // Controller는 스폰 ON/OFF와 난이도 값만 넘기고, 실제 Instantiate/등록은 이 컴포넌트가 수행한다.
    public sealed class CinderkeepEnemySpawner : MonoBehaviour
    {
        [SerializeField] private CinderkeepEnemy CinderkeepEnemy_EnemyTemplate;
        [SerializeField] private Transform Transform_Target;
        [SerializeField] private GameDataManager GameDataManager_GameDataManager;
        [SerializeField] private float _spawnRadius = 28f;
        [SerializeField] private float _spawnInterval = 4f;
        [SerializeField] private int _maxAliveCount = 6;
        [SerializeField] private string _enemyDataId = "plant";
        [SerializeField] private string _bossDataId = "boss";

        private CinderkeepEnemy _bossEnemy;
        private float _timer;
        private bool _isSpawningEnabled = true;

        public bool HasLivingBoss => _bossEnemy != null && _bossEnemy.IsAlive;

        private void Update()
        {
            if (!_isSpawningEnabled || CinderkeepEnemy_EnemyTemplate == null || Transform_Target == null)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer < _spawnInterval)
            {
                return;
            }

            _timer = 0f;
            if (CountAliveEnemies() >= _maxAliveCount)
            {
                return;
            }

            SpawnEnemy();
        }

        public void SetTarget(Transform target)
        {
            Transform_Target = target;
        }

        public void SetEnemyTemplate(CinderkeepEnemy enemyTemplate)
        {
            CinderkeepEnemy_EnemyTemplate = enemyTemplate;
        }

        public void SetGameDataManager(GameDataManager gameDataManager)
        {
            GameDataManager_GameDataManager = gameDataManager;
        }

        public void SetSpawningEnabled(bool isEnabled)
        {
            _isSpawningEnabled = isEnabled;
            _timer = 0f;
        }

        public void SetMaxAliveCount(int maxAliveCount)
        {
            _maxAliveCount = Mathf.Max(0, maxAliveCount);
        }

        public void SetSpawnInterval(float spawnInterval)
        {
            _spawnInterval = Mathf.Max(0.2f, spawnInterval);
        }

        public void SetSpawnRadius(float spawnRadius)
        {
            _spawnRadius = Mathf.Max(8f, spawnRadius);
        }

        public void SetEnemyDataId(string enemyDataId)
        {
            _enemyDataId = string.IsNullOrWhiteSpace(enemyDataId) ? "plant" : enemyDataId;
        }

        public void SetBossDataId(string bossDataId)
        {
            _bossDataId = string.IsNullOrWhiteSpace(bossDataId) ? "boss" : bossDataId;
        }

        public CinderkeepEnemy SpawnBoss()
        {
            // TODO(팀원 작업 요청): 보스 전용 Prefab이 준비되면 EnemyTemplate 대신 BossTemplate 필드를 추가해 연결해 주세요.
            if (CinderkeepEnemy_EnemyTemplate == null || Transform_Target == null)
            {
                return null;
            }

            Vector3 spawnPosition = Transform_Target.position + new Vector3(0f, 0f, _spawnRadius + 8f);
            CinderkeepEnemy boss = SpawnEnemyAt(spawnPosition, _bossDataId, true);
            boss.name = "Enemy_Boss_CinderWarden";
            _bossEnemy = boss;
            return boss;
        }

        private void SpawnEnemy()
        {
            Vector3 spawnPosition = GameUtil.GetRandomRingPosition(Transform_Target.position, _spawnRadius);
            SpawnEnemyAt(spawnPosition, _enemyDataId, false);
        }

        private CinderkeepEnemy SpawnEnemyAt(Vector3 spawnPosition, string enemyDataId, bool isBoss)
        {
            // 생성된 적은 GameObjectManager에 등록해서 충돌/피해/드랍 처리에서 instance id로 추적할 수 있게 한다.
            CinderkeepEnemy enemy = Instantiate(CinderkeepEnemy_EnemyTemplate, spawnPosition, Quaternion.identity, transform.parent);
            enemy.gameObject.SetActive(true);
            enemy.SetTarget(Transform_Target);

            GameDataManager dataManager = GameDataManager_GameDataManager != null ? GameDataManager_GameDataManager : GameDataManager.Instance;
            enemy.Configure(dataManager?.GetEnemyData(enemyDataId), isBoss);
            GameObjectManager.Instance?.RegisterGameObject(enemy.gameObject);
            return enemy;
        }

        private int CountAliveEnemies()
        {
            int aliveCount = 0;
            CinderkeepEnemy[] enemies = FindObjectsByType<CinderkeepEnemy>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != CinderkeepEnemy_EnemyTemplate && enemies[i].IsAlive)
                {
                    aliveCount++;
                }
            }

            return aliveCount;
        }
    }
}
