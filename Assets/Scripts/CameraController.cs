using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{





    [Header("Footstep Sound")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.7f;
    [SerializeField] private float fadeOutTime = 0.2f;
    [SerializeField] private float minimumMoveSpeed = 0.1f;

    [Header("Movement Settings")]
    public float gravity = -9.81f;
    public float moveSpeed = 3.5f;
    public float verticalVelocity = 0f;
    public CharacterController characterController;
    public bool ignoreMovement = false;

    [Header("Mouse Look Settings")]
    public float jumpHeight = 1.2f; // 점프 높이 추가
    public float mouseSensitivity = 2f;
    public float minPitch = -60f; // 위쪽 제한
    public float maxPitch = 75f;  // 아래쪽 제한
    public float pitch = 0f; // 카메라의 위아래 회전 값
    public float rotateSpeed = 2f;
    public bool isPopupOpened = false;
    public bool isMenuOpened = false;

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
    public InputActionReference tabAction; // 상호작용 입력
    public InputActionReference jumpAction; // 점프 입력 추가
    [Header("GameObject References")]
    public GameObject popup;
    public GameObject menu;
    public GameObject menual;

    [SerializeField] Animator animator;

    [SerializeField] Transform HandlingPos;
    [SerializeField] Transform SpawnPos;

    public static event Action<int> OnResetPosition;

    [Header("텔레포트 못하는 bool")]
    public bool returnToggle = false;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (mainCamera == null)
        {
            mainCamera = _mainCamera.transform;
        }

        if (raycaster == null) raycaster = GetComponent<Raycaster>();

        characterController = GetComponent<CharacterController>();

        if (footstepAudioSource != null)
        {
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.loop = true;
            footstepAudioSource.volume = footstepVolume;

            if (footstepClip != null)
                footstepAudioSource.clip = footstepClip;
        }
        if (popup != null)
        {
            popup.SetActive(false);
        }
        isPopupOpened = false;
        targetFov = _mainCamera.fieldOfView;
        if (menu != null)
        {
            menu.SetActive(false);
        }

        //  DontDestroyOnLoad(gameObject);

    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        mouse = Mouse.current;

        moveInputAction.action.Enable();
        returnAction.action.Enable();
        scrollAction.action.Enable();
        tabAction.action.Enable();

        if (jumpAction != null)
        {
            jumpAction.action.Enable(); // 활성화
        }

        returnAction.action.performed += OnReturnPerformed;
        scrollAction.action.performed += OnScroll;
        tabAction.action.performed += _ => ToggleMenu();
    }

    private void OnDisable()
    {
        StopFootstepSound();

        moveInputAction.action.Disable();
        returnAction.action.Disable();
        scrollAction.action.Disable();
        tabAction.action.Disable();

        if (jumpAction != null)
        {
            jumpAction.action.Disable();
        }

        returnAction.action.performed -= OnReturnPerformed;
        scrollAction.action.performed -= OnScroll;
    }

    private void Update()
    {
        if (isPopupOpened || isMenuOpened)
        {
            if (animator != null)
                animator.SetBool("isWalking", false);

            StopFootstepSound();
            return;
        }

        if (Application.isFocused == false)
        {
            skipNextMouseDelta = true;
            StopFootstepSound();
            return;
        }
        HandleXRTurn();
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
        if (ignoreMovement)
        {
            if (animator != null)
                animator.SetBool("isWalking", false);

            StopFootstepSound();
            return;
        }

        Vector2 input = moveInputAction.action.ReadValue<Vector2>();

        bool hasInput = input.sqrMagnitude > 0.01f;

        Vector3 move = transform.right * input.x +
                       transform.forward * input.y;

        move = Vector3.ClampMagnitude(move, 1f);
        move *= moveSpeed;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (jumpAction != null && jumpAction.action.triggered)
            {
                verticalVelocity =
                    Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (animator != null)
                    animator.SetTrigger("Jump");
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        bool isWalking =
            hasInput &&
            characterController.isGrounded &&
            horizontalVelocity.magnitude > minimumMoveSpeed;

        if (animator != null)
            animator.SetBool("isWalking", isWalking);

        UpdateFootstepSound();
    }

    bool skipNextMouseDelta = false;
    private void HandleMouseLook()
    {
        if (skipNextMouseDelta)
        {
            skipNextMouseDelta = false;
            return;
        }

        //if (mouse.leftButton.isPressed && !raycaster.isDragging)
        //{
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
        //}
    }

    bool initFov = false;
    private Coroutine fadeCoroutine;

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
        if (!returnToggle) return;

        Debug.Log("CameraController: Reset Position");
        characterController.enabled = false;

        gameObject.transform.SetPositionAndRotation(SpawnPos.position, SpawnPos.rotation);
        OnResetPosition?.Invoke(0);

        characterController.enabled = true;
        returnToggle = false;
    }

    private CursorLockMode prevLockMode = CursorLockMode.Locked;
    private bool prevCursorVisiblity = false;
    public void ToggleMenu()
    {
        if (menu == null) return;

        // 오브젝트 활성화/비활성화 반전
        bool active = !menu.activeSelf;

        menu.SetActive(active);
        isMenuOpened = active;

        if (active)
        {
            prevLockMode = Cursor.lockState;
            prevCursorVisiblity = Cursor.visible;

            // 메뉴가 켜지면: 마우스 자유롭게 + 보이기
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // 필요하다면 게임 일시정지
        }
        else
        {
            // 메뉴가 꺼지면: 마우스 고정 + 숨기기
            Cursor.lockState = prevLockMode;
            Cursor.visible = prevCursorVisiblity;
            Time.timeScale = 1f; // 게임 다시 재생
            menual.SetActive(false);
        }
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

    public void MoveForObject(Transform target)
    {
        SetMoveLockState(true);
        transform.SetPositionAndRotation(target.position, target.rotation);
        SetMoveLockState(false);
    }

    public void SetMoveLockState(bool isLock)
    {
        skipNextMouseDelta = isLock;
        ignoreMovement = isLock;
        characterController.enabled = !isLock;
    }

    private void UpdateFootstepSound()
    {
        if (footstepAudioSource == null || characterController == null)
            return;

        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        bool isActuallyMoving =
            characterController.isGrounded &&
            horizontalVelocity.magnitude > minimumMoveSpeed &&
            !ignoreMovement;

        if (isActuallyMoving)
        {
            if (!footstepAudioSource.isPlaying)
                footstepAudioSource.Play();
        }
        else
        {
            StopFootstepSound();
        }
    }

    private void StopFootstepSound()
    {
        if (footstepAudioSource == null || !footstepAudioSource.isPlaying)
            return;

        if (fadeCoroutine != null)
            return;

        fadeCoroutine = StartCoroutine(FadeOutFootstep());
    }

    private IEnumerator FadeOutFootstep()
    {
        float start = footstepAudioSource.volume;
        float time = 0f;

        while (time < fadeOutTime)
        {
            time += Time.deltaTime;
            footstepAudioSource.volume = Mathf.Lerp(start, 0f, time / fadeOutTime);
            yield return null;
        }

        footstepAudioSource.Stop();
        footstepAudioSource.volume = footstepVolume;
        fadeCoroutine = null;
    }
}