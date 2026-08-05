using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera VirtualCam;
    [SerializeField] private InputActionReference scrollAction;
    [SerializeField] private CameraSwitcher cameraSwitcher;

    [Header("FOV")]
    [SerializeField] private float zoomSpeed = 0.08f;
    [SerializeField] private float minFOV = 35f;
    [SerializeField] private float maxFOV = 90f;
    private float targetFov;

    [Header("Smoothing (optional)")]
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothTime = 10f;
    [SerializeField] Camera _mainCamera;

    private Mouse mouse;

    private void Awake()
    {
        _mainCamera = Camera.main;
        targetFov = _mainCamera.fieldOfView;
        mouse = Mouse.current;

        cameraSwitcher.OnCameraChanged += SetVirualCam;
        cameraSwitcher.OnCameraReverted += DisableVirtualCam;

        enabled = false;
    }

    private void OnEnable()
    {
        scrollAction.action.performed += OnScroll;
    }

    private void OnDisable()
    {
        scrollAction.action.performed -= OnScroll;
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        if (_mainCamera == null || Application.isFocused == false || mouse.rightButton.isPressed) return;

        Vector2 scroll = ctx.ReadValue<Vector2>();
        float delta = scroll.y;
        targetFov -= delta * zoomSpeed;
        targetFov = Mathf.Clamp(targetFov, minFOV, maxFOV);

        if (!smooth)
        {
            var lens = VirtualCam.Lens;
            lens.FieldOfView = targetFov;
            VirtualCam.Lens = lens;
        }
    }

    public void SetVirualCam(CinemachineCamera camera)
    {
        VirtualCam = camera;    
        enabled = true;
    }

    public void DisableVirtualCam()
    {
        enabled = false;
    }
}
