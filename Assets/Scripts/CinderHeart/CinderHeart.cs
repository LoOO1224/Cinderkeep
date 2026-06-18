using UnityEngine;

// CinderHeart 오브젝트에 붙이는 표식 컴포넌트입니다.
// 몬스터 이동 목표는 EnemyMovement.SetCinderHeartTarget()으로 명확하게 연결합니다.
public sealed class CinderHeart : MonoBehaviour
{
    public Transform GetTargetTransform()
    {
        return transform;
    }
}
