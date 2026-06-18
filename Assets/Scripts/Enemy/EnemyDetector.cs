using UnityEngine;
using Cinderkeep.Gameplay;
using System.Collections;

public sealed class EnemyDetector : MonoBehaviour
{
    [SerializeField] private float _viewAngle = 90.0f;

    private const string TargetTag = "Player";
    private const int MaxAllocation = 20;                   // 객체가 동시에 감지할 수 있는 콜라이더 최대 할당량
    private const float DetectionInterval = 0.2f;           // 연산 주기

    private Coroutine _coroutineDetectionRoutine;
    private readonly Collider[] _overlapColliders = new Collider[MaxAllocation]; 

    private float _detectorDistance;

    private bool _isInitialized;                                    //초기 테스트를 위한 변수         scene 단계에서 생성할시, 초기화가 안이루어져 버그가 발생.
    [SerializeField] private string _testEnemyId = "ice_zombie";    //초기 테스트를 위한 데이터       때문에 Start단계에서 초기화가 안이뤄져있다면, 초기화를 하는 로직을 추가

    public Transform DetectedPlayer { get; private set; }   //타겟 플레이어 위치 정보

    public bool HasDetectedPlayer                           //타겟 플레이어 감지 유무
    {
        get
        {
            return DetectedPlayer != null;
        }
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            if (string.IsNullOrEmpty(_testEnemyId))
            {
                Debug.LogError($"[{gameObject.name}] 테스트용 _testEnemyId가 비어있습니다! 인스펙터에 JSON ID를 적어주세요.");
                return;
            }

            if (GameManager.Inst == null)
            {
                Debug.LogError($"[{gameObject.name}] 씬에 GameManager가 존재하지 않아 테스트 초기화에 실패했습니다.");
                return;
            }

            GameDataManager dataManager = GameManager.Inst.GetGameDataManager();
            if (dataManager == null)
            {
                Debug.LogError($"[{gameObject.name}] GameManager에 GameDataManager가 연결되어 있지 않습니다.");
                return;
            }

            // 데이터 추출 후 자가 초기화 진행
            EnemyData enemyData = dataManager.GetEnemy(_testEnemyId);
            if (enemyData == null)
            {
                Debug.LogError($"[{gameObject.name}] JSON 테이블에서 ID [{_testEnemyId}]를 찾을 수 없습니다.");
                return;
            }

            Initialize(enemyData);
        }
    }



    private void OnEnable()
    {
        StartDetectionRoutine();
    }

    private void OnDisable()
    {
        StopDetectionRoutine();
    }

    
    public void Initialize(EnemyData enemyData)              //초기화
    {
        if (enemyData == null)
        {
            return;
        }
        _detectorDistance = enemyData.DetectorDistance;
    }

    
    private IEnumerator CoPerformDetectionRoutine()         //0.2초마다 부채꼴 탐색을 수행하는 루틴
    {
        WaitForSeconds waitInterval = new WaitForSeconds(DetectionInterval);

        while (true)
        {
                                                            // 이미 타겟을 잡고 있는 상태라면, 사거리 이탈 여부만 체크
            if (HasDetectedPlayer)
            {
                float currentDistance = Vector3.Distance(transform.position, DetectedPlayer.position);
                if (currentDistance > _detectorDistance)
                {
                    DetectedPlayer = null; 
                }

                yield return waitInterval;
                continue; 
            }

            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);
            Transform transformTargetFound = null;

            for (int i = 0; i < hitCount; i++)
            {
                Collider col = _overlapColliders[i];

                if (col.transform == transform)
                {
                    continue;                               // 자기 자신 콜라이더 제외
                }
                if (!col.CompareTag(TargetTag))
                {
                    continue;                               // 플레이어가 아닌 프롭/환경 오브젝트 스킵
                }

                if (IsInSectorFieldOfView(col.transform))
                {
                    transformTargetFound = col.transform;
                    break;                                  // 정면의 플레이어를 찾았을때
                }
            }

            DetectedPlayer = transformTargetFound;
            yield return waitInterval;
        }
    }

    
    private bool IsInSectorFieldOfView(Transform transformTarget)
    {
        Vector3 directionToTarget = (transformTarget.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        return angle <= (_viewAngle * 0.5f);        //부채꼴 감지 각도
    }

    
    public void EnableAlertMode()                   //경계모드로, 피격시 부채꼴이 아닌 자신 주위 적 확인.
    {
        if (HasDetectedPlayer)
        {
            return;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, _detectorDistance, _overlapColliders);

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = _overlapColliders[i];

            if (col.transform == transform) continue;
            if (!col.CompareTag(TargetTag)) continue;

            DetectedPlayer = col.transform;
            Debug.Log($"[{gameObject.name}] 시야각 외 피격! 주변 범위 내 플레이어를 강제 포착해 타겟을 고정합니다.");
            return;
        }

        Debug.Log($"[{gameObject.name}] 피격당했으나 감지 범위 내에 플레이어 태그가 존재하지 않아 맞고 끝납니다.");
    }

    private void StartDetectionRoutine()
    {
        StopDetectionRoutine();
        _coroutineDetectionRoutine = StartCoroutine(CoPerformDetectionRoutine());
    }

    private void StopDetectionRoutine()
    {
        if (_coroutineDetectionRoutine != null)
        {
            StopCoroutine(_coroutineDetectionRoutine);
            _coroutineDetectionRoutine = null;
        }
    }
}