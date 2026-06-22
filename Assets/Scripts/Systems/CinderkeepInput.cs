using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Unity의 New Input System을 프로젝트에서 쓰기 쉽게 감싼 입력 보조 클래스입니다.
// 플레이어 스크립트가 구 Input Manager API를 직접 쓰지 않도록 이 클래스만 거치게 합니다.
public static class CinderkeepInput
{
    private const float MouseDeltaScale = 0.02f;

    public static Vector2 GetMoveAxisRaw()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical);
    }

    public static Vector2 GetMouseAxis()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return Vector2.zero;
        }

        return mouse.delta.ReadValue() * MouseDeltaScale;
    }

    public static bool IsKeyPressed(KeyCode keyCode)
    {
        KeyControl keyControl = GetKeyControl(keyCode);
        if (keyControl == null)
        {
            return false;
        }

        return keyControl.isPressed;
    }

    public static bool WasKeyPressedThisFrame(KeyCode keyCode)
    {
        KeyControl keyControl = GetKeyControl(keyCode);
        if (keyControl == null)
        {
            return false;
        }

        return keyControl.wasPressedThisFrame;
    }

    public static bool WasLeftMousePressedThisFrame()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        return mouse.leftButton.wasPressedThisFrame;
    }

    private static KeyControl GetKeyControl(KeyCode keyCode)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return null;
        }

        Key key;
        if (TryGetKey(keyCode, out key) == false)
        {
            return null;
        }

        return keyboard[key];
    }

    private static bool TryGetKey(KeyCode keyCode, out Key key)
    {
        key = Key.None;

        switch (keyCode)
        {
            case KeyCode.Alpha1:
                key = Key.Digit1;
                return true;
            case KeyCode.Alpha2:
                key = Key.Digit2;
                return true;
            case KeyCode.Alpha3:
                key = Key.Digit3;
                return true;
            case KeyCode.Alpha4:
                key = Key.Digit4;
                return true;
            case KeyCode.Alpha5:
                key = Key.Digit5;
                return true;
            case KeyCode.Alpha6:
                key = Key.Digit6;
                return true;
            case KeyCode.Alpha7:
                key = Key.Digit7;
                return true;
            case KeyCode.Tab:
                key = Key.Tab;
                return true;
            case KeyCode.B:
                key = Key.B;
                return true;
            case KeyCode.E:
                key = Key.E;
                return true;
            case KeyCode.T:
                key = Key.T;
                return true;
            case KeyCode.Space:
                key = Key.Space;
                return true;
            case KeyCode.LeftShift:
                key = Key.LeftShift;
                return true;
            case KeyCode.RightShift:
                key = Key.RightShift;
                return true;
            case KeyCode.W:
                key = Key.W;
                return true;
            case KeyCode.A:
                key = Key.A;
                return true;
            case KeyCode.S:
                key = Key.S;
                return true;
            case KeyCode.D:
                key = Key.D;
                return true;
            case KeyCode.UpArrow:
                key = Key.UpArrow;
                return true;
            case KeyCode.DownArrow:
                key = Key.DownArrow;
                return true;
            case KeyCode.LeftArrow:
                key = Key.LeftArrow;
                return true;
            case KeyCode.RightArrow:
                key = Key.RightArrow;
                return true;
        }

        return false;
    }
}
