using UnityEngine;

// 키보드와 마우스 입력을 읽어 이동, 달리기, 공격 같은 플레이어 의도만 저장합니다.
// 실제 이동/공격 실행은 PlayerController가 PlayerMovement와 PlayerAttack에 전달합니다.
public sealed class PlayerInput : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool RunIntent { get; private set; }
    public bool AttackTriggered { get; private set; }
    public bool BuildTriggered { get; private set; }
    public bool JumpTriggered { get; private set; }

    private void Update()
    {
        ReadInput();
    }

    public void ReadInput()
    {
        if (CinderkeepInput.IsGameplayInputBlocked())
        {
            ClearInput();
            return;
        }

        MoveInput = CinderkeepInput.GetMoveAxisRaw();
        RunIntent = CinderkeepInput.IsKeyPressed(KeyCode.LeftShift);
        AttackTriggered = CinderkeepInput.WasLeftMousePressedThisFrame();
        BuildTriggered = CinderkeepInput.WasKeyPressedThisFrame(KeyCode.B);
        JumpTriggered = CinderkeepInput.WasKeyPressedThisFrame(KeyCode.Space);
    }

    public void ClearInput()
    {
        MoveInput = Vector2.zero;
        RunIntent = false;
        AttackTriggered = false;
        BuildTriggered = false;
        JumpTriggered = false;
    }
}
