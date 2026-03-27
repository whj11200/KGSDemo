using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TPSPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraPivot;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float rotationSmooth = 12f;

    [Header("Aim")]
    [SerializeField] private float aimMoveSpeed = 3.5f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private int aimMouseButton = 1;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isAiming;

    public bool IsAiming => isAiming;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraPivot == null && Camera.main != null)
        {
            cameraPivot = Camera.main.transform;
        }
    }

    private void Update()
    {
        isAiming = Input.GetMouseButton(aimMouseButton);
        HandleMove();
    }

    private void HandleMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        if (input.sqrMagnitude > 0.001f)
        {
            Vector3 forward = cameraPivot.forward;
            Vector3 right = cameraPivot.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDir = (forward * input.z + right * input.x).normalized;

            float speed = isAiming ? aimMoveSpeed : moveSpeed;
            if (!isAiming && Input.GetKey(sprintKey))
            {
                speed = sprintSpeed;
            }

            controller.Move(moveDir * speed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmooth * Time.deltaTime
            );
        }

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
