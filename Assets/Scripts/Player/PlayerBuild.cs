using UnityEngine;
using UnityEngine.Serialization;

public sealed class PlayerBuild : MonoBehaviour
{
    [FormerlySerializedAs("Prefab_Fence")]
    [FormerlySerializedAs("GameObject_BuildingPrefab")]
    [SerializeField] private GameObject _buildingPrefab;
    [FormerlySerializedAs("SpawnDistance")]
    [SerializeField] private float _spawnDistance = 3f;
    [SerializeField] private KeyCode _buildKey = KeyCode.B;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(_buildKey))
        {
            SpawnBuilding();
        }
    }

    private void SpawnBuilding()
    {
        if (_buildingPrefab == null)
        {
            Debug.LogError("PlayerBuild: 건축 프리팹이 인스펙터에 할당되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition = transform.position + transform.forward * _spawnDistance;
        Quaternion spawnRotation = transform.rotation;

        // 검증용 직접 생성입니다. 정식 건축 시스템에서는 GameObjectManager 경유로 교체합니다.
        Instantiate(_buildingPrefab, spawnPosition, spawnRotation);

        Debug.Log("PlayerBuild: 플레이어 앞에 건축물이 생성되었습니다.");
    }
}
