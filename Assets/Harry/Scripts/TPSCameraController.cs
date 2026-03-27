using UnityEngine;

public class TPSCameraController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 normalOffset = new Vector3(0f, 2.1f, -4f);
    [SerializeField] private Vector3 aimOffset = new Vector3(0.7f, 2f, -2.5f);
    [SerializeField] private float followSmooth = 12f;
    [SerializeField] private float lookSensitivity = 2.2f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 65f;

    private TPSPlayerController playerController;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        if (target != null)
        {
            playerController = target.GetComponent<TPSPlayerController>();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        bool isAiming = playerController != null && playerController.IsAiming;
        Vector3 desiredOffset = isAiming ? aimOffset : normalOffset;
        Vector3 desiredPos = target.position + rotation * desiredOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSmooth * Time.deltaTime
        );
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
