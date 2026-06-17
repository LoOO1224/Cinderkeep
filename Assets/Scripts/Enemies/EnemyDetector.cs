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


}
