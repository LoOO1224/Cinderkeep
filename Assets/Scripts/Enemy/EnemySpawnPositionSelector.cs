using UnityEngine;

// 적이 실제로 생성될 위치와 회전을 계산하는 클래스입니다.
// EnemySpawnPoint는 스폰 지점 설정을 보관하고, 위치 계산은 이 클래스에 맡깁니다.
public sealed class EnemySpawnPositionSelector
{
    public Vector3 GetSpawnPosition(
        Transform centerTransform,
        Transform[] candidatePoints,
        float spawnSpacing,
        int index,
        int totalCount)
    {
        Vector3 spawnPosition = GetSpawnCenterPosition(centerTransform, candidatePoints);
        float centerOffset = (totalCount - 1) * 0.5f;
        float xOffset = (index - centerOffset) * spawnSpacing;
        spawnPosition.x += xOffset;
        return spawnPosition;
    }

    public Quaternion GetSpawnRotation(Transform centerTransform)
    {
        if (centerTransform == null)
        {
            return Quaternion.identity;
        }

        return centerTransform.rotation;
    }

    public Vector3 GetGizmoPosition(Vector3 centerPosition, float spawnSpacing, int index, int totalCount)
    {
        float centerOffset = (totalCount - 1) * 0.5f;
        float xOffset = (index - centerOffset) * spawnSpacing;
        Vector3 position = centerPosition;
        position.x += xOffset;
        return position;
    }

    private Vector3 GetSpawnCenterPosition(Transform centerTransform, Transform[] candidatePoints)
    {
        Transform candidatePoint = GetRandomCandidatePoint(candidatePoints);
        if (candidatePoint != null)
        {
            return candidatePoint.position;
        }

        if (centerTransform == null)
        {
            return Vector3.zero;
        }

        return centerTransform.position;
    }

    private Transform GetRandomCandidatePoint(Transform[] candidatePoints)
    {
        if (candidatePoints == null)
        {
            return null;
        }

        if (candidatePoints.Length == 0)
        {
            return null;
        }

        int randomStartIndex = Random.Range(0, candidatePoints.Length);
        for (int i = 0; i < candidatePoints.Length; i++)
        {
            int candidateIndex = (randomStartIndex + i) % candidatePoints.Length;
            Transform candidatePoint = candidatePoints[candidateIndex];
            if (candidatePoint != null)
            {
                return candidatePoint;
            }
        }

        return null;
    }
}
