using UnityEngine;
using UnityEngine.Serialization;

// 플레이어의 WASD 이동과 Shift 달리기를 담당하는 컴포넌트입니다.
// 체력/스태미나 계산은 PlayerStatus가 담당하고, 이 클래스는 이동 입력만 처리합니다.
public sealed class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("moveSpeed")]
    [SerializeField] private float _moveSpeed = 5f;
    [FormerlySerializedAs("runSpeed")]
    [SerializeField] private float _runSpeed = 10f;
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _groundStickVelocity = -2f;

    private PlayerStatus _playerStatus;
    private CharacterController _characterController;
    private Vector3 _moveDirection;
    private float _verticalVelocity;

    public bool IsRunningNow
    {
        get
        {
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
            bool isMoving = _moveDirection.magnitude > 0.01f;
            bool canRun = CheckCanRun();

            return isMoving && isShiftPressed && canRun;
        }
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        ReadMoveInput();
        Move();
    }

    public void MovePlayer(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }

    public void Jump(float jumpForce)
    {
        if (_characterController == null || _characterController.isGrounded == false)
        {
            return;
        }

        _verticalVelocity = jumpForce;
    }

    private void ReadMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        _moveDirection = moveDirection.normalized;
    }

    private void Move()
    {
        if (_characterController == null)
        {
            return;
        }

        bool isRunning = IsRunningNow;

        if (isRunning == true && _playerStatus != null)
        {
            _playerStatus.ConsumeStaminaForRun();
        }

        float currentSpeed = GetCurrentSpeed(isRunning);
        Vector3 horizontalVelocity = _moveDirection * currentSpeed;
        Vector3 velocity = horizontalVelocity + Vector3.up * _verticalVelocity;

        _characterController.Move(velocity * Time.deltaTime);
        ApplyGravity();
    }

    private float GetCurrentSpeed(bool isRunning)
    {
        if (isRunning)
        {
            return _runSpeed;
        }

        return _moveSpeed;
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded == true && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundStickVelocity;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }

    private bool CheckCanRun()
    {
        if (_playerStatus == null)
        {
            return true;
        }

        return _playerStatus.CanRun();
    }
}
