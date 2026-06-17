using UnityEngine;
using UnityEngine.Serialization;

public sealed class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("moveSpeed")]
    [SerializeField] private float _moveSpeed = 5f;
    [FormerlySerializedAs("runSpeed")]
    [SerializeField] private float _runSpeed = 10f;

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReadMoveInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void MoveCharacter(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }

    private void ReadMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.right * horizontal) + (transform.forward * vertical);
        _moveDirection = moveDirection.normalized;
    }

    private void Move()
    {
        if (_rigidbody == null)
        {
            return;
        }

        // Shift를 누르면 같은 이동 입력을 달리기 속도로 적용합니다.
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool isMoving = _moveDirection.magnitude > 0.01f;
        bool isRunning = isMoving && isShiftPressed;

        float currentSpeed = isRunning ? _runSpeed : _moveSpeed;
        Vector3 targetVelocity = _moveDirection * currentSpeed;

        targetVelocity.y = _rigidbody.linearVelocity.y;
        _rigidbody.linearVelocity = targetVelocity;
    }
}
