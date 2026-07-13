using UnityEngine;

// 적 루트의 실제 이동량을 Animator의 MoveSpeed 값으로 전달합니다.
// 이동과 타깃 판단은 변경하지 않고 Idle과 Walk 외형 전환만 담당합니다.
public sealed class EnemyLocomotionAnimator : MonoBehaviour
{
    private const string MoveSpeedParameter = "MoveSpeed";
    private const float MinimumDeltaTime = 0.0001f;

    private Animator _animator;
    private int _moveSpeedParameterHash;
    private Vector3 _previousPosition;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _moveSpeedParameterHash = Animator.StringToHash(MoveSpeedParameter);
        _previousPosition = transform.position;
    }

    private void OnEnable()
    {
        _previousPosition = transform.position;
    }

    private void Update()
    {
        if (_animator == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        if (deltaTime <= MinimumDeltaTime)
        {
            return;
        }

        Vector3 currentPosition = transform.position;
        float moveSpeed = Vector3.Distance(currentPosition, _previousPosition) / deltaTime;
        _previousPosition = currentPosition;

        _animator.SetFloat(_moveSpeedParameterHash, moveSpeed, 0.12f, deltaTime);
    }
}
