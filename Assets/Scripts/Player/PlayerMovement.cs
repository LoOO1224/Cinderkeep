using UnityEngine;
using UnityEngine.Serialization;

// 1인칭 플레이어의 입력, 상태, 장착, 채집, 전투, 건축 중 한 흐름을 담당합니다.
// 입력 제어와 실제 효과를 분리해 퀵슬롯, 도구, 무기, 튜토리얼이 서로 얽히지 않게 합니다.
// 플레이어의 WASD 이동과 Shift 달리기를 담당하는 컴포넌트입니다.
// PlayerController가 있으면 입력 의도를 전달받고, 없으면 기존처럼 직접 입력을 읽어 움직입니다.
public sealed class PlayerMovement : MonoBehaviour
{
    [FormerlySerializedAs("moveSpeed")]
    [Tooltip("플레이어 기본 걷기 속도입니다.")]
    [SerializeField] private float _moveSpeed = 5f;
    [FormerlySerializedAs("runSpeed")]
    [Tooltip("Shift를 누르고 이동할 때 적용되는 달리기 속도입니다.")]
    [SerializeField] private float _runSpeed = 10f;
    [Tooltip("플레이어에게 적용되는 중력 값입니다. 음수 값이 아래 방향입니다.")]
    [SerializeField] private float _gravity = -20f;
    [Tooltip("땅에 붙어 있을 때 아래로 살짝 눌러주는 속도입니다. CharacterController가 지면을 안정적으로 인식하게 합니다.")]
    [SerializeField] private float _groundStickVelocity = -2f;

    private PlayerStatus _playerStatus;
    private PlayerController _playerController;
    private CharacterController _characterController;
    private Vector3 _moveDirection;
    private float _verticalVelocity;
    private float _equipmentMoveSpeedBonus;
    private bool _runIntent;

    public bool IsRunningNow
    {
        get
        {
            bool isMoving = _moveDirection.magnitude > 0.01f;
            bool canRun = CheckCanRun();

            return isMoving && _runIntent && canRun;
        }
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerStatus = GetComponent<PlayerStatus>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (ShouldReadMoveInputDirectly())
        {
            ReadMoveInput();
        }

        Move();
    }

    public void MovePlayer(Vector3 moveDirection)
    {
        _moveDirection = moveDirection;
    }

    public void ProcessMove(Vector2 moveInput, bool runIntent)
    {
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        _moveDirection = moveDirection.normalized;
        _runIntent = runIntent;
    }

    public void SetEquipmentMoveSpeedBonus(float moveSpeedBonus)
    {
        _equipmentMoveSpeedBonus = Mathf.Clamp(moveSpeedBonus, -0.5f, 1f);
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
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            ProcessMove(Vector2.zero, false);
            return;
        }

        Vector2 moveInput = CinderkeepInput.GetMoveAxisRaw();
        ProcessMove(moveInput, CinderkeepInput.IsKeyPressed(KeyCode.LeftShift));
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
        float baseSpeed = isRunning ? _runSpeed : _moveSpeed;
        return Mathf.Max(0.1f, baseSpeed * (1f + _equipmentMoveSpeedBonus));
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

    private bool ShouldReadMoveInputDirectly()
    {
        return _playerController == null || _playerController.enabled == false;
    }
}
