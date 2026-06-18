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

            }
        }




        private void StartMovementRoutine()
        {
            if (_isInitialized)
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
