using System.Collections.Generic;
using UnityEngine;

namespace Cinderkeep.Gameplay
{
    // 변하지 않는 기획 데이터를 JSON에서 읽어 보관하는 매니저입니다.
    // 현재는 3일 MVP 루프에 필요한 몬스터 데이터부터 연결합니다.
    // 다음 우선순위는 자원, 제작식, 고정 건축 지점 데이터입니다.
    // 이후 제작, 건축, 불꽃 강화 데이터도 같은 방식으로 추가합니다.
    // 이 매니저는 싱글톤이 아닙니다. GameManager가 Inspector 참조를 통해 Initialize를 호출합니다.
    public sealed class GameDataManager : MonoBehaviour, IGameInitializable
    {
        [SerializeField] private string _enemyDataResourcePath = GameUtil.EnemyDataResourcePath;

        private readonly Dictionary<string, EnemyData> _enemyDataList = new Dictionary<string, EnemyData>();
        private bool _isInitialized;

        public IReadOnlyDictionary<string, EnemyData> EnemyDataList
        {
            get
            {
                return _enemyDataList;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            GameUtil.LoadFullData(this);
            _isInitialized = true;
        }

        public void LoadEnemyData(string resourcePath)
        {
            _enemyDataList.Clear();

            TextAsset jsonAsset = Resources.Load<TextAsset>(resourcePath);
            if (jsonAsset == null)
            {
                Debug.LogWarning("GameDataManager: enemy JSON was not found at Resources/" + resourcePath + ".json");
                return;
            }

            EnemyDataCatalog catalog = JsonUtility.FromJson<EnemyDataCatalog>(jsonAsset.text);
            if (catalog == null || catalog.Items == null)
            {
                Debug.LogWarning("GameDataManager: enemy JSON format is invalid.");
                return;
            }

            for (int i = 0; i < catalog.Items.Count; i++)
            {
                AddData(_enemyDataList, catalog.Items[i]);
            }
        }

        public EnemyData GetEnemy(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            if (!_enemyDataList.ContainsKey(id))
            {
                return null;
            }

            return _enemyDataList[id];
        }

        public string GetEnemyDataResourcePath()
        {
            return _enemyDataResourcePath;
        }

        private void AddData<TData>(Dictionary<string, TData> target, TData data)
            where TData : GameDataBase
        {
            if (target == null || data == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(data.Id))
            {
                return;
            }

            if (target.ContainsKey(data.Id))
            {
                target[data.Id] = data;
                return;
            }

            target.Add(data.Id, data);
        }
    }
}
