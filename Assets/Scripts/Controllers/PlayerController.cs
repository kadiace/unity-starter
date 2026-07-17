using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Vector2 _lastLoggedMove;
    private bool _lastLoggedCrouch;

    private void Update()
    {
        Vector2 moveInput = Managers.Input.ReadMove();
        bool crouchHeld = Managers.Input.IsCrouchHeld();

        Move(moveInput);
        LogMoveInput(moveInput);
        LogCrouchInput(crouchHeld);

        if (Managers.Input.WasJumpPressedThisFrame())
            OnJumpInput();
        if (Managers.Input.WasAttackPressedThisFrame())
            OnAttackInput();
        if (Managers.Input.WasInteractPressedThisFrame())
            OnInteractInput();
    }

    private void Move(Vector2 moveInput)
    {
        Vector3 velocity = new(moveInput.x, moveInput.y, 0f);
        transform.position += moveSpeed * Time.deltaTime * velocity;
    }

    private void LogMoveInput(Vector2 moveInput)
    {
        if (moveInput == _lastLoggedMove)
            return;

        _lastLoggedMove = moveInput;
        Debug.Log($"[PlayerInput] Arrow move = {moveInput}");
    }

    private void LogCrouchInput(bool crouchHeld)
    {
        if (crouchHeld == _lastLoggedCrouch)
            return;

        _lastLoggedCrouch = crouchHeld;
        Debug.Log($"[PlayerInput] Shift crouch held = {crouchHeld}");
    }

    private void OnJumpInput()
    {
        Debug.Log("[PlayerInput] C jump pressed");
    }

    private void OnAttackInput()
    {
        Debug.Log("[PlayerInput] X attack pressed");
    }

    private void OnInteractInput()
    {
        Debug.Log("[PlayerInput] Z interact pressed");
    }
}
