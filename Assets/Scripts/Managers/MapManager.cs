using System.Collections.Generic;
using UnityEngine;

// 씬 참조와 런타임 모델을 연결하는 얇은 매니저 허브입니다.
// 계산과 세부 규칙은 작은 시스템/헬퍼로 분리하고, 이 클래스는 연결 책임에 집중합니다.
namespace Cinderkeep.Gameplay
{
    // GameManager 초기화 순서에 맞춰 모듈형 맵 청크를 준비하는 매니저입니다.
    // 중앙 청크는 CinderHeart 방어, 전투, 건축 테스트가 가능한 평평한 구역으로 유지합니다.
    public sealed class MapManager : MonoBehaviour, IGameInitializable
    {
        [Header("Chunk Settings")]
        [Tooltip("GameManager 초기화 시 자동으로 모듈형 맵을 생성할지 결정합니다.")]
        [SerializeField] private bool _generateOnInitialize;
        [Tooltip("청크 하나의 월드 크기입니다. 청크 간 배치 간격으로 사용됩니다.")]
        [SerializeField] private float _chunkSize = 120f;
        [Tooltip("중앙 청크 기준으로 몇 칸까지 맵을 생성할지 결정합니다. 1이면 3x3 구조입니다.")]
        [SerializeField] private int _mapRadius = 2;

        [Header("Chunk Prefabs")]
        [Tooltip("중앙에 배치할 평평한 코어 청크 프리팹입니다.")]
        [SerializeField] private GameObject _centerChunkPrefab;
        [Tooltip("중앙과 외곽 사이에 랜덤 배치할 일반 청크 프리팹 목록입니다.")]
        [SerializeField] private List<GameObject> _normalChunkPrefabs = new List<GameObject>();
        [Tooltip("맵 바깥쪽에 랜덤 배치할 외곽 청크 프리팹 목록입니다.")]
        [SerializeField] private List<GameObject> _edgeChunkPrefabs = new List<GameObject>();

        private Dictionary<Vector2Int, GameObject> _spawnedChunks = new Dictionary<Vector2Int, GameObject>();
        private bool _isInitialized;

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

            if (_generateOnInitialize == true)
            {
                GenerateModularMap();
            }

            _isInitialized = true;
        }

        public void GenerateModularMap()
        {
            if (CheckCanGenerateMap() == false)
            {
                return;
            }

            ClearGeneratedChunks();

            for (int x = -_mapRadius; x <= _mapRadius; x++)
            {
                for (int z = -_mapRadius; z <= _mapRadius; z++)
                {
                    Vector2Int gridPosition = new Vector2Int(x, z);
                    GameObject chunkPrefab = SelectChunkPrefabCanBeNull(gridPosition);

                    if (chunkPrefab != null)
                    {
                        SpawnChunk(gridPosition, chunkPrefab);
                    }
                }
            }
        }

        public void ClearGeneratedChunks()
        {
            foreach (GameObject chunk in _spawnedChunks.Values)
            {
                if (chunk != null)
                {
                    Destroy(chunk);
                }
            }

            _spawnedChunks.Clear();
        }

        private bool CheckCanGenerateMap()
        {
            if (_chunkSize <= 0f)
            {
                Debug.LogWarning("MapManager: Chunk size must be bigger than zero.");
                return false;
            }

            if (_mapRadius < 0)
            {
                Debug.LogWarning("MapManager: Map radius cannot be negative.");
                return false;
            }

            if (_centerChunkPrefab == null)
            {
                Debug.LogWarning("MapManager: Center chunk prefab is empty.");
                return false;
            }

            if (_normalChunkPrefabs == null || _normalChunkPrefabs.Count == 0)
            {
                Debug.LogWarning("MapManager: Normal chunk prefab list is empty.");
                return false;
            }

            if (_edgeChunkPrefabs == null || _edgeChunkPrefabs.Count == 0)
            {
                Debug.LogWarning("MapManager: Edge chunk prefab list is empty.");
                return false;
            }

            return true;
        }

        private GameObject SelectChunkPrefabCanBeNull(Vector2Int gridPosition)
        {
            if (CheckIsCenterPosition(gridPosition) == true)
            {
                return _centerChunkPrefab;
            }

            if (CheckIsEdgePosition(gridPosition) == true)
            {
                return GetRandomPrefabCanBeNull(_edgeChunkPrefabs);
            }

            return GetRandomPrefabCanBeNull(_normalChunkPrefabs);
        }

        private bool CheckIsCenterPosition(Vector2Int gridPosition)
        {
            return gridPosition.x == 0 && gridPosition.y == 0;
        }

        private bool CheckIsEdgePosition(Vector2Int gridPosition)
        {
            bool isEdgeX = Mathf.Abs(gridPosition.x) == _mapRadius;
            bool isEdgeZ = Mathf.Abs(gridPosition.y) == _mapRadius;

            return isEdgeX || isEdgeZ;
        }

        private GameObject GetRandomPrefabCanBeNull(List<GameObject> prefabs)
        {
            if (prefabs == null || prefabs.Count == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, prefabs.Count);
            return prefabs[randomIndex];
        }

        private void SpawnChunk(Vector2Int gridPosition, GameObject prefab)
        {
            Vector3 worldPosition = new Vector3(gridPosition.x * _chunkSize, 0f, gridPosition.y * _chunkSize);

            GameObject chunk = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
            chunk.name = "MapChunk_" + gridPosition.x + "_" + gridPosition.y;

            _spawnedChunks.Add(gridPosition, chunk);
        }
    }
}
