using UnityEngine;
using UnityEngine.AI;
using Cinderkeep.Gameplay;
using System.Collections;

namespace Cinderkeep.Gameplay
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField]
        private EnemyDetector _detectorEnemy;

        private NavMeshAgent _navMeshAgent;
        private Transform _currentTrackingTarget;
        [SerializeField]
        private const float PathUpdateInterval = 0.2f;  //코루틴 주기.

        private Coroutine _corutineMovementRoutine;
        private bool _isInitialized;


                                                                                                        //scene 단계에서 생성할시, 초기화가 안이루어져 버그가 발생.
        [SerializeField] private string _testEnemyId = "ice_zombie";    //초기 테스트를 위한 데이터       때문에 Start단계에서 초기화가 안이뤄져있다면, 초기화를 하는 로직을 추가



        private void OnEnable()
        {
            StartMovementRoutine();
        }

        private void OnDisable()
        {
            StopMovementRoutine();
        }

        public void Initialize(EnemyData enemyData, EnemyDetector detectorEnemy)
        {
            if (enemyData == null)
            {
                return;
            }

            _navMeshAgent = GetComponent<NavMeshAgent>();
            _detectorEnemy = detectorEnemy;

            _navMeshAgent.speed = enemyData.MoveSpeed;
            _navMeshAgent.stoppingDistance = enemyData.StopDistance;

            _isInitialized = true;

            StartMovementRoutine();
        }

        private IEnumerator CoPerformMovementRoutine()
        {
            WaitForSeconds waitInterval = new WaitForSeconds(PathUpdateInterval);

            while (true)
            {
                if (_isInitialized && _detectorEnemy != null)
                {
                    PerformPriorityMovement();
                }
                yield return waitInterval;
            }
        }

        private void PerformPriorityMovement()
        {
            Transform nextTarget = null;

            if (_detectorEnemy.HasDetectedPlayer)
            {
                nextTarget = _detectorEnemy.DetectedPlayer;
            }
            else if (CinderHeart.InstanceTransform != null)
            {
                nextTarget = CinderHeart.InstanceTransform;
            }
            if(nextTarget != null)
            {
                UpdateNavMeshDestination(nextTarget);
            }
            else
            {
                if(_navMeshAgent.hasPath)
                {
                    _navMeshAgent.ResetPath();
                }
            }
        }

        private void UpdateNavMeshDestination(Transform targetTransform)
        {
            if(_currentTrackingTarget != targetTransform || _navMeshAgent.destination != targetTransform.position)
            {
                _currentTrackingTarget = targetTransform;
                _navMeshAgent.SetDestination(_currentTrackingTarget.position);
            }
        }



        private void StartMovementRoutine()
        {
            if (!_isInitialized)
            {
                return;
            }
            StopMovementRoutine();
            _corutineMovementRoutine = StartCoroutine(CoPerformMovementRoutine());
        }

        private void StopMovementRoutine()
        {
            if (_corutineMovementRoutine != null)
            {
                StopCoroutine(_corutineMovementRoutine);
                _corutineMovementRoutine = null;
            }
        }



    }
}
