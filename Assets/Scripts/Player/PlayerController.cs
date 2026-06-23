using UnityEngine;

public enum PlayerControlState
{
    Normal,
    Building,
    Teleporting,
    OpenUI,
    Dead
}

// PlayerInput의 의도를 읽고 현재 상태에 따라 하위 행동 컴포넌트에 명령을 전달합니다.
// UI, 기절, 귀환처럼 플레이어 조작을 막아야 하는 상태가 늘어나면 이 컴포넌트에서 흐름을 통제합니다.
public sealed class PlayerController : MonoBehaviour
{
    [Header("Control State")]
    [Tooltip("현재 플레이어 조작 상태입니다. UI 열림, 귀환, 사망 같은 상태에서 입력 처리를 막는 기준입니다.")]
    [SerializeField] private PlayerControlState _currentState = PlayerControlState.Normal;

    [Header("Connected Components")]
    [Tooltip("키보드/마우스 입력을 읽어 의도만 저장하는 컴포넌트입니다.")]
    [SerializeField] private PlayerInput _playerInput;
    [Tooltip("실제 이동 처리를 담당하는 컴포넌트입니다.")]
    [SerializeField] private PlayerMovement _playerMovement;
    [Tooltip("실제 공격 처리를 담당하는 컴포넌트입니다. 무기 수치는 weapons.json을 우선 사용합니다.")]
    [SerializeField] private PlayerAttack _playerAttack;

    public PlayerControlState CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    private void Awake()
    {
        ConnectComponents();
    }

    private void Update()
    {
        if (CanProcessInput() == false)
        {
            StopMovementInput();
            return;
        }

        _playerInput.ReadInput();
        ProcessMovement();
        ProcessActions();
    }

    public void SetState(PlayerControlState newState)
    {
        _currentState = newState;
    }

    public bool IsInputBlocked()
    {
        return _currentState == PlayerControlState.Teleporting
            || _currentState == PlayerControlState.OpenUI
            || _currentState == PlayerControlState.Dead;
    }

    private void ConnectComponents()
    {
        if (_playerInput == null)
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        if (_playerAttack == null)
        {
            _playerAttack = GetComponent<PlayerAttack>();
        }
    }

    private bool CanProcessInput()
    {
        return _playerInput != null && IsInputBlocked() == false;
    }

    private void StopMovementInput()
    {
        if (_playerMovement != null)
        {
            _playerMovement.ProcessMove(Vector2.zero, false);
        }

        if (_playerInput != null)
        {
            _playerInput.ClearInput();
        }
    }

    private void ProcessMovement()
    {
        if (_playerMovement == null)
        {
            return;
        }

        _playerMovement.ProcessMove(_playerInput.MoveInput, _playerInput.RunIntent);
    }

    private void ProcessActions()
    {
        if (_playerAttack == null)
        {
            return;
        }

        if (_playerInput.AttackTriggered)
        {
            _playerAttack.ExecuteAttack();
        }

        if (_playerInput.BuildTriggered)
        {
            SetState(PlayerControlState.Building);
        }
    }
}
