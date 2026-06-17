using UnityEngine;

public class PlayerBuild : MonoBehaviour
{
    [SerializeField] private GameObject Prefab_Fence;
    [SerializeField] private float SpawnDistance = 3.0f;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            SpawnBuilding();
        }
    }

    private void SpawnBuilding()
    {
        if(Prefab_Fence == null)
        {
            Debug.LogError("Prefab_Fence가 인스펙터에 할당되지 않았습니다");
            return;
        }

        Vector3 spawnPosition = transform.position + (transform.forward * SpawnDistance);

        Quaternion spawnRotation = transform.rotation;

        Instantiate(Prefab_Fence, spawnPosition, spawnRotation);

        Debug.Log($"캐릭터 정면 {SpawnDistance}m 앞에 건축물이 생성되었습니다");
    }
}
