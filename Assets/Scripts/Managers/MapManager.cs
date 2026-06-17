using Cinderkeep.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MapManager : MonoBehaviour, IGameInitializable
{
    [Header("Chunk Settings")]
    public float _chunkSize = 50f;
    public int _mapRadius = 2;

    [Header("Prefabs")]
    public GameObject _centerChunkPrefab;
    public List<GameObject> _normalChunkPrefabs;
    public List<GameObject> _spawnChunkPrefabs;

    private Dictionary<Vector2Int, GameObject> _spawnedChunks = new Dictionary<Vector2Int, GameObject>();

    private bool _isInitialized;
    public bool IsInitialized => _isInitialized;

    public void Initialize()
    {
        if (_isInitialized) return;

        GenerateModularMap();
        _isInitialized = true;
    }


    void GenerateModularMap()
    {
        for (int x = -_mapRadius; x <= _mapRadius; x++)
        {
            for (int z = -_mapRadius; z <= _mapRadius; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);
                GameObject selectedPrefab = null;

                if (x == 0 && z == 0)
                {
                    selectedPrefab = _centerChunkPrefab;
                }
                else if (Mathf.Abs(x) == _mapRadius || Mathf.Abs(z) == _mapRadius)
                {
                    selectedPrefab = _spawnChunkPrefabs[Random.Range(0, _spawnChunkPrefabs.Count)];
                }
                else
                {
                    selectedPrefab = _normalChunkPrefabs[Random.Range(0, _normalChunkPrefabs.Count)];
                }

                if (selectedPrefab != null)
                {
                    SpawnChunk(gridPos, selectedPrefab);
                }
            }
        }
    }

    void SpawnChunk(Vector2Int gridPos, GameObject prefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x * _chunkSize, 0f, gridPos.y * _chunkSize);

        float[] rotations = { 0f, 90f, 180f, 270f };
        float randomRot = rotations[Random.Range(0, rotations.Length)];
        Quaternion rotation = Quaternion.Euler(0f, randomRot, 0f);

        GameObject chunk = Instantiate(prefab, worldPos, rotation, transform);
        chunk.name = $"Chunk_{gridPos.x}_{gridPos.y}";

        _spawnedChunks.Add(gridPos, chunk);
    }
}
