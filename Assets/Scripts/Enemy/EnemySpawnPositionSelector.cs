using UnityEngine;

// 적의 감지, 타깃 판단, 이동, 공격, 보스 클리어 연결 중 한 역할을 담당합니다.
// AI 판단과 실제 행동 컴포넌트를 분리해 적 패턴이 늘어도 유지보수 가능하게 합니다.
// 적이 실제로 생성될 위치와 회전을 계산하는 클래스입니다.
// EnemySpawnPoint는 스폰 지점 설정을 보관하고, 위치 계산은 이 클래스에 맡깁니다.
public sealed class EnemySpawnPositionSelector
{
    private const float GoldenAngle = 137.50776f;
    private const float MinimumSpawnSpacing = 0.8f;

    public Vector3 GetSpawnPosition(
        Transform centerTransform,
        Transform[] candidatePoints,
        float spawnSpacing,
        int index,
        int totalCount)
    {
        Vector3 spawnPosition = GetSpawnCenterPosition(centerTransform, candidatePoints);
        spawnPosition += GetSpreadOffset(spawnSpacing, index, totalCount);
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
        Vector3 position = centerPosition;
        position += GetSpreadOffset(spawnSpacing, index, totalCount);
        return position;
    }

    private Vector3 GetSpreadOffset(float spawnSpacing, int index, int totalCount)
    {
        if (totalCount <= 1)
        {
            return Vector3.zero;
        }

        float safeSpacing = Mathf.Max(MinimumSpawnSpacing, spawnSpacing);
        float radius = safeSpacing * Mathf.Sqrt(index + 1);
        float angle = index * GoldenAngle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
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
