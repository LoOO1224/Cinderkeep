using UnityEngine;

public enum PlayerControlState
{
    Normal,
    Building,
    Teleporting,
    OpenUI,
    Dead
}

// 플레이어 입력을 읽고 현재 제어 상태에 따라 이동/공격 컴포넌트에 명령을 전달합니다.
// 건축 홀드 입력은 PlayerBuild가 직접 처리하므로, 이 클래스는 이동과 전투 입력 흐름만 제어합니다.
public sealed class PlayerController : MonoBehaviour
{
    [Header("Control State")]
    [Tooltip("현재 플레이어 조작 상태입니다. UI 열림, 순간이동, 사망 상태에서는 입력 처리를 막습니다.")]
    [SerializeField] private PlayerControlState _currentState = PlayerControlState.Normal;

    [Header("Connected Components")]
    [Tooltip("키보드와 마우스 입력을 읽어 의도만 저장하는 컴포넌트입니다.")]
    [SerializeField] private PlayerInput _playerInput;
    [Tooltip("실제 이동 처리를 담당하는 컴포넌트입니다.")]
    [SerializeField] private PlayerMovement _playerMovement;
    [Tooltip("실제 공격 처리를 담당하는 컴포넌트입니다. 무기 수치는 weapons.json을 우선 사용합니다.")]
    [SerializeField] private PlayerAttack _playerAttack;

    public PlayerControlState CurrentState
    {
        get { return _currentState; }
    }

    private void Awake()
    {
        ConnectComponents();
        CinderkeepInput.RegisterPlayerController(this);
    }

    private void OnDestroy()
    {
        CinderkeepInput.UnregisterPlayerController(this);
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
    }
}
