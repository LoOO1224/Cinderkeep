using UnityEngine;
using Cinderkeep.Gameplay;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] private float _viewAngle = 90.0f;

    private const string TargetTag = "Player";
    private const int MaxAllocation = 20;   //객체가 감지하는 오브젝트의 최대 갯수

    private readonly Collider[] _overlapColliders = new Collider[MaxAllocation];    //감지된 객체의 배열

    private float _detectorDistance;

    public Transform Transform_DetectedPlayer { get; private set; }

    public bool _isHashDetectedPlayer
    {
        get
        {
            return _isHashDetectedPlayer != null;
        }
    }

    private void Update()
    {
        PerformDetection();
    }


    public void Initialize(EnemyData enemyData)
    {
        if(enemyData == null)
        {
            return;
        }
        _detectorDistance = enemyData.DetectorDistance;
    }

    private void PerformDetection()
    {
        if(_isHashDetectedPlayer)
        {
            return ;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);
        Transform transformTargetFound = null;

        for(int i = 0; i < hitCount; i++)
        {
            Collider col = _overlapColliders[i];
            if(col.transform == transform)
            {
                continue;
            }
            if(!col.CompareTag(TargetTag))
            {
                continue;
            }

            if(IsInSectorFieldOfView(col.transform))
            {
                transformTargetFound = col.transform;
                break;
            }
        }
        Transform_DetectedPlayer = transformTargetFound;
    }
    private bool IsInSectorFieldOfView(Transform transformTarget)
    {
        Vector3 directionToTarget = (transformTarget.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= (_viewAngle * 0.5f);
    }

    public void EnableAlertMode()   //데미지를 받으면 원 내의 플레이어 캐릭터에게 어그로를 끔
    {
        if(_isHashDetectedPlayer)
        {
            return ;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for(int i = 0; i < hitCount;i++)
        {
            Collider col = _overlapColliders[i];

            if(col.transform == transform)
            {
                continue;
            }
            if(!col.CompareTag(TargetTag))
            {
                continue;
            }

            Transform_DetectedPlayer = col.transform;
            Debug.Log($"[{gameObject.name}] 시야각 외 피격 주변 범위 내 플레이어를 강제 포착해 타겟을 고정합니다.");
        }
        Debug.Log($"[{gameObject.name}] 피격당했으나 감지 범위 내에 플레이어 태그가 존재하지 않아 맞고 끝납니다.");
    }

}
