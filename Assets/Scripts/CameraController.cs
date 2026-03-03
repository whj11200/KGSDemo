using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float gravity = -9.81f;
    public float moveSpeed = 3.5f;
    public float verticalVelocity = 0f;
    public CharacterController characterController;
    public bool ignoreMovement = false;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -60f; // 위쪽 제한
    public float maxPitch = 75f;  // 아래쪽 제한
    public float pitch = 0f; // 카메라의 위아래 회전 값
    public float rotateSpeed = 2f;
    public bool isPopupOpened = false;

    public Transform mainCamera;
    private Camera _mainCamera;

    [Header("FOV")]
    [SerializeField] private float zoomSpeed = 0.08f;
    [SerializeField] private float minFOV = 35f;
    [SerializeField] private float maxFOV = 90f;

    [Header("Smoothing (optional)")]
    [SerializeField] private bool smooth = true;
    [SerializeField] private float smoothTime = 10f;
    private float targetFov;

    [Header("Click Interactor")]
    [SerializeField] private Raycaster raycaster;
    private Mouse mouse;

    [Header("Input Property")]
    public InputActionReference moveInputAction; // WASD 또는 방향키 이동 입력
    public InputActionReference returnAction; // 전시실로 돌아가기
    public InputActionReference scrollAction; // 줌 인/아웃
    public GameObject popup;

    [SerializeField] Transform HandlingPos;
    [SerializeField] Transform SpawnPos;

    public static event Action<int> OnResetPosition;

    private void Awake()
    {
        _mainCamera = Camera.main;
         
        if (mainCamera == null)
        {
            mainCamera = _mainCamera.transform;
        }

        if (raycaster == null) raycaster = GetComponent<Raycaster>();

        characterController = GetComponent<CharacterController>();
        popup.SetActive(false);
        isPopupOpened = false;
        targetFov = _mainCamera.fieldOfView;

      //  DontDestroyOnLoad(gameObject);
}

    private void OnEnable()
    {
        mouse = Mouse.current;

        moveInputAction.action.Enable();
        returnAction.action.Enable();
        scrollAction.action.Enable();

        returnAction.action.performed += OnReturnPerformed;
        scrollAction.action.performed += OnScroll;
    }

    private void OnDisable()
    {
        moveInputAction.action.Disable();
        returnAction.action.Disable();
        scrollAction.action.Disable();

        returnAction.action.performed -= OnReturnPerformed;
        scrollAction.action.performed -= OnScroll;
    }

    private void Update()
    {
        if (isPopupOpened) return;

        if (Application.isFocused == false)
        {
            skipNextMouseDelta = true;
            return;
        }

        // moveInputAction 입력받아서 이동
        HandleMovement();

        // 마우스 회전 처리
        // 왼쪽 마우스 버튼을 누르고 있을 때만 회전
        HandleMouseLook();

        // 마우스 휠 줌 인/아웃
        // HandleFovZoom();
    }

    private void Start()
    {
        _mainCamera.fieldOfView = maxFOV;
    }

    public void PickUp(Transform model, Quaternion ViewAngle)
    {
        model.SetParent(HandlingPos, true);

        model.localPosition = Vector3.zero;
        model.localRotation = ViewAngle;
    }

    private void HandleMovement()
    {
        if (ignoreMovement) return;

        Vector2 input = moveInputAction.action.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        move *= moveSpeed;

        // 중력 적용
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }

    bool skipNextMouseDelta = false;
    private void HandleMouseLook()
    {
        if (skipNextMouseDelta)
        {
            skipNextMouseDelta = false;
            return;
        }

        if (mouse.leftButton.isPressed && !raycaster.isDragging)
        {
            Vector2 delta = mouse.delta.ReadValue(); // 이번 프레임 마우스 이동량
            float speed = delta.magnitude;                   // 이동 속도(픽셀 변화량)

            // 속도 기반 가속 생성 (1.0 ~ 3.0 사이에서 자연스럽게 증가)
            float accel = Mathf.Lerp(1f, 3f, Mathf.Clamp01(speed * 0.05f));

            // 최종 회전량 = 이동량 × 감도 × 가속
            float yaw = delta.x * rotateSpeed * 0.01f * accel;
            float pitchDelta = delta.y * rotateSpeed * 0.01f * accel;

            transform.Rotate(Vector3.up, yaw);
            pitch -= pitchDelta;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            mainCamera.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }

    bool initFov = false;
    private void HandleFovZoom()
    {
        if (!smooth || _mainCamera == null || Application.isFocused == false) return;

        if (!initFov)
        {
            _mainCamera.fieldOfView = maxFOV;
            initFov = true;
            return;
        }

        float t = 1f - Mathf.Exp(-smoothTime * Time.deltaTime);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFov, t);
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        if (_mainCamera == null || Application.isFocused == false || mouse.rightButton.isPressed) return;

        Vector2 scroll = ctx.ReadValue<Vector2>();
        float delta = scroll.y;
        targetFov -= delta * zoomSpeed;
        targetFov = Mathf.Clamp(targetFov, minFOV, maxFOV);

        if (!smooth)
            _mainCamera.fieldOfView = targetFov;
    }

    private void OnReturnPerformed(InputAction.CallbackContext ctx)
    {
        // TogglePopup();

        Debug.Log("CameraController: Reset Position");
        characterController.enabled = false;
        gameObject.transform.SetPositionAndRotation(SpawnPos.position, SpawnPos.rotation);
        OnResetPosition?.Invoke(0);
        characterController.enabled = true;
    }

    public void TogglePopup()
    {
        bool active = !popup.activeSelf;
        popup.SetActive(active);

        isPopupOpened = active;
    }

    public void SetInputParam(float _rotSpeed, float _moveSpeed)
    {
        rotateSpeed = _rotSpeed;
        moveSpeed = _moveSpeed;
    }

    public void LookObject(Transform target)
    {
        // 마우스 이동 무시
        skipNextMouseDelta = true;

        Vector3 dir = (target.position - transform.position).normalized;

        // Yaw 계산 (수평 방향만)
        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.001f)
        {
            Quaternion yawRot = Quaternion.LookRotation(flatDir);
            transform.rotation = yawRot;
        }

        // Pitch 계산
        float angle = Vector3.SignedAngle(
            flatDir,
            dir,
            transform.right
        );

        pitch = Mathf.Clamp(-angle, minPitch, maxPitch);
        mainCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
