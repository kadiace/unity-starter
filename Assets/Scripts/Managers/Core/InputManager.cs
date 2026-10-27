using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    public InputMode Mode { get; private set; } = InputMode.Player;

    public void Init()
    {
        SetMode(InputMode.Player);
    }

    public void SetMode(InputMode mode)
    {
        Mode = mode;

        switch (mode)
        {
            case InputMode.Player:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 1f;
                break;
            case InputMode.UI:
            case InputMode.Cinematic:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                break;
        }
    }

    public Vector2 ReadMove()
    {
        if (Mode != InputMode.Player)
            return Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector2.zero;

        Vector2 move = Vector2.zero;

        if (keyboard.leftArrowKey.isPressed)
            move.x -= 1f;
        if (keyboard.rightArrowKey.isPressed)
            move.x += 1f;
        if (keyboard.downArrowKey.isPressed)
            move.y -= 1f;
        if (keyboard.upArrowKey.isPressed)
            move.y += 1f;

        return move.normalized;
    }

    public bool IsCrouchHeld()
    {
        if (Mode != InputMode.Player)
            return false;

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
    }

    public bool WasJumpPressedThisFrame()
    {
        return WasPressedThisFrame(Key.C);
    }

    public bool WasAttackPressedThisFrame()
    {
        return WasPressedThisFrame(Key.X);
    }

    public bool WasInteractPressedThisFrame()
    {
        return WasPressedThisFrame(Key.Z);
    }

    private bool WasPressedThisFrame(Key key)
    {
        if (Mode != InputMode.Player)
            return false;

        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard[key].wasPressedThisFrame;
    }
}
