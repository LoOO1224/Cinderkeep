using UnityEngine;
using UnityEngine.Serialization;

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


    //실시간으로 달리고 있는지 여부를 외부에서 알 수 있도록 개방
    public bool IsRunningNow
    {
        get
        {
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);
            bool isMoving = _moveDirection.magnitude > 0.01f;
            bool hasStamina = _playerStatus != null && _playerStatus.CurrentStamina > 0f;

            return isMoving && isShiftPressed && hasStamina;
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
        if (_characterController == null || !_characterController.isGrounded)
        {
            return;
        }

        _verticalVelocity = jumpForce;
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
        if (_characterController == null)
        {
            return;
        }

        bool isRunning = IsRunningNow;

        // 달리는 중 PlayerStatus에게 스태미나 소모를 실시간 요청
        if (isRunning == true && _playerStatus != null)
        {
            _playerStatus.ConsumeStaminaInMovement();
        }

        float currentSpeed = isRunning ? _runSpeed : _moveSpeed;
        Vector3 horizontalVelocity = _moveDirection * currentSpeed;
        Vector3 velocity = horizontalVelocity + Vector3.up * _verticalVelocity;

        _characterController.Move(velocity * Time.deltaTime);
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundStickVelocity;
            return;
        }

        _verticalVelocity += _gravity * Time.deltaTime;
    }
}
