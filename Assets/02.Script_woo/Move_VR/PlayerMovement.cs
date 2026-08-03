using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("References")]
    [SerializeField] private Transform directionReference;
    [SerializeField] private CharacterController characterController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.2f;

    private float verticalVelocity;
    private bool movementBlocked;

    public Vector2 CurrentInput { get; private set; }
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (directionReference == null && Camera.main != null)
        {
            directionReference = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        jumpAction?.action.Disable();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (characterController == null || !characterController.enabled)
            return;

        if (movementBlocked)
        {
            CurrentInput = Vector2.zero;
            IsMoving = false;

            ApplyGravityOnly();
            return;
        }

        CurrentInput = moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        Transform reference = directionReference != null
            ? directionReference
            : transform;

        Vector3 forward = reference.forward;
        Vector3 right = reference.right;

        // HMD가 위아래를 보고 있어도 수평으로만 이동
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 horizontalMove =
            right * CurrentInput.x +
            forward * CurrentInput.y;

        horizontalMove = Vector3.ClampMagnitude(horizontalMove, 1f);

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (jumpAction != null &&
                jumpAction.action.WasPressedThisFrame())
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalMove * moveSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);

        IsMoving =
            horizontalMove.sqrMagnitude > 0.01f &&
            characterController.velocity.sqrMagnitude > 0.01f;
    }

    private void ApplyGravityOnly()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(
            Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void SetMovementBlocked(bool blocked)
    {
        movementBlocked = blocked;

        if (blocked)
        {
            CurrentInput = Vector2.zero;
            IsMoving = false;
        }
    }
}