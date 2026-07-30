using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PhoneSystem : OverlayUI
{
    [Header("Phone UI")]
    [SerializeField] private GameObject phoneObject;
    [SerializeField] private RectTransform phoneRect;

    [Tooltip("상대방에게 전화가 걸려왔을 때 표시되는 UI")]
    [SerializeField] private GameObject incomingCallUI;

    [Tooltip("내가 상대방에게 전화할 때 표시되는 UI")]
    [SerializeField] private GameObject outgoingCallUI;

    [SerializeField] private GameObject darkPhoneBackGround;

    [Header("Sound")]
    [SerializeField] private AudioSource callAudioSource;

    [Tooltip("상대방에게 전화가 걸려왔을 때 재생되는 벨소리")]
    [SerializeField] private AudioClip incomingCallClip;

    [Tooltip("내가 상대방에게 전화할 때 재생되는 발신 대기음")]
    [SerializeField] private AudioClip outgoingCallClip;

    [Header("Vibration")]
    [SerializeField] private float rotateAngle = 4f;
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float positionShake = 2f;

    [Header("Reject")]
    [SerializeField] private float recallDelay = 1.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onIncomingCall;
    [SerializeField] private UnityEvent onOutgoingCall;
    [SerializeField] private UnityEvent onAcceptCall;
    [SerializeField] private UnityEvent onRejectCall;

    [Header("References")]
    [SerializeField] private CameraController controller;

    private Quaternion originRotation;
    private Vector2 originAnchoredPosition;

    private Coroutine vibrationCoroutine;
    private Coroutine recallCoroutine;

    private bool isCalling;
    private bool initialized;

    private bool IsDesktop => PlayerDeviceManager.IsDesktop;

    /// <summary>
    /// 현재 전화가 울리거나 발신 중인지 여부.
    /// </summary>
    public bool IsCalling => isCalling;

    public string currentNodeID { get; private set; } = "";

    protected override void Awake()
    {
        base.Awake();

        Init();

        HideCallUI();
        HidePhone();
    }
    
    private void Init()
    {
        if (initialized)
            return;

        if (phoneObject == null)
        {
            Debug.LogError("PhoneSystem: phoneObject가 연결되지 않았습니다.");
            return;
        }

        if (phoneRect == null)
            phoneRect = phoneObject.GetComponent<RectTransform>();

        if (phoneRect == null)
        {
            Debug.LogError("PhoneSystem: phoneRect가 연결되지 않았습니다.");
            return;
        }

        originRotation = phoneRect.localRotation;
        originAnchoredPosition = phoneRect.anchoredPosition;

        initialized = true;
    }

    // =========================================================
    // 수신 전화
    // =========================================================

    /// <summary>
    /// 상대방에게 전화가 걸려오는 상태를 시작한다.
    /// </summary>
    public void StartIncomingCall()
    {
        Init();

        if (!initialized)
            return;

        CancelRecall();

        isCalling = true;

        ShowCursor();
        ShowPhone();
        ShowIncomingCallUI();

        PlayCallSound(incomingCallClip, true);
        StartVibration();

        onIncomingCall?.Invoke();
    }

    // =========================================================
    // 발신 전화
    // =========================================================

    /// <summary>
    /// 내가 상대방에게 전화를 거는 상태를 시작한다.
    /// </summary>
    public void StartOutgoingCall()
    {
        Init();

        if (!initialized)
            return;

        CancelRecall();

        isCalling = true;

        ShowCursor();
        ShowPhone();
        ShowOutgoingCallUI();

        // 발신 대기음 재생
        //PlayCallSound(outgoingCallClip, true);

        // 발신 중에는 폰 진동을 사용하지 않음
        StopVibration();

        onOutgoingCall?.Invoke();
    }

    // =========================================================
    // 전화 상태 종료
    // =========================================================

    /// <summary>
    /// 벨소리, 발신음, 진동, 전화 UI, 폰 화면을 모두 종료한다.
    /// </summary>
    public void StopCall()
    {
        isCalling = false;

        StopVibration();
        StopCallSound();

        HideCallUI();
        HidePhone();
    }

    /// <summary>
    /// 전화 연결 시 벨소리와 수신/발신 UI만 끈다.
    /// PhoneChat UI를 표시해야 하므로 phoneObject는 유지한다.
    /// </summary>
    public void StopRingingOnly()
    {
        isCalling = false;

        StopVibration();
        StopCallSound();

        HideCallUI();
    }

    /// <summary>
    /// 전화 UI 전체를 닫고 마우스 커서를 다시 잠근다.
    /// </summary>
    public void ClosePhone()
    {
        StopCall();
        HideCursor();
    }

    // =========================================================
    // 전화 거절 후 재호출
    // =========================================================

    /// <summary>
    /// 수신 전화를 거절한 뒤 일정 시간 후 다시 전화가 오게 한다.
    /// </summary>
    public void RestartCallAfterDelay()
    {
        CancelRecall();

        recallCoroutine = StartCoroutine(RecallRoutine());
    }

    private IEnumerator RecallRoutine()
    {
        yield return new WaitForSecondsRealtime(recallDelay);

        recallCoroutine = null;

        // 재호출은 수신 전화로 시작
        StartIncomingCall();
    }

    private void CancelRecall()
    {
        if (recallCoroutine == null)
            return;

        StopCoroutine(recallCoroutine);
        recallCoroutine = null;
    }

    // =========================================================
    // UI
    // =========================================================

    private void ShowPhone()
    {
        if (phoneObject != null)
            phoneObject.SetActive(true);

        if (darkPhoneBackGround != null && IsDesktop)
            darkPhoneBackGround.SetActive(true);
    }

    private void HidePhone()
    {
        HideCallUI();

        if (phoneObject != null)
            phoneObject.SetActive(false);

        if (darkPhoneBackGround != null && IsDesktop)
            darkPhoneBackGround.SetActive(false);
    }

    /// <summary>
    /// 수신 전화 UI만 표시한다.
    /// </summary>
    private void ShowIncomingCallUI()
    {
        if (incomingCallUI != null)
            incomingCallUI.SetActive(true);

        if (outgoingCallUI != null)
            outgoingCallUI.SetActive(false);
    }

    /// <summary>
    /// 발신 전화 UI만 표시한다.
    /// </summary>
    private void ShowOutgoingCallUI()
    {
        if (incomingCallUI != null)
            incomingCallUI.SetActive(false);

        if (outgoingCallUI != null)
            outgoingCallUI.SetActive(true);
    }

    /// <summary>
    /// 수신 및 발신 UI를 모두 숨긴다.
    /// </summary>
    private void HideCallUI()
    {
        if (incomingCallUI != null)
            incomingCallUI.SetActive(false);

        if (outgoingCallUI != null)
            outgoingCallUI.SetActive(false);
    }

    // =========================================================
    // 사운드
    // =========================================================

    private void PlayCallSound(AudioClip clip, bool loop)
    {
        if (callAudioSource == null)
        {
            Debug.LogWarning("PhoneSystem: callAudioSource가 연결되지 않았습니다.");
            return;
        }

        callAudioSource.Stop();
        callAudioSource.clip = clip;
        callAudioSource.loop = loop;

        if (clip != null)
        {
            callAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("PhoneSystem: 재생할 AudioClip이 없습니다.");
        }
    }

    private void StopCallSound()
    {
        if (callAudioSource == null)
            return;

        callAudioSource.Stop();
        callAudioSource.loop = false;
        callAudioSource.clip = null;
    }

    // =========================================================
    // 진동
    // =========================================================

    private void StartVibration()
    {
        if (!initialized || phoneRect == null)
            return;

        if (vibrationCoroutine != null)
            StopCoroutine(vibrationCoroutine);

        vibrationCoroutine = StartCoroutine(VibrationRoutine());
    }

    private void StopVibration()
    {
        if (vibrationCoroutine != null)
        {
            StopCoroutine(vibrationCoroutine);
            vibrationCoroutine = null;
        }

        ResetPhoneUI();
    }

    private IEnumerator VibrationRoutine()
    {
        while (isCalling)
        {
            float time = Time.unscaledTime;

            float zRotation =
                Mathf.Sin(time * rotateSpeed) * rotateAngle;

            float xShake =
                Random.Range(-positionShake, positionShake);

            float yShake =
                Random.Range(-positionShake, positionShake);

            phoneRect.localRotation =
                originRotation * Quaternion.Euler(0f, 0f, zRotation);

            phoneRect.anchoredPosition =
                originAnchoredPosition + new Vector2(xShake, yShake);

            yield return null;
        }

        vibrationCoroutine = null;

        ResetPhoneUI();
    }

    private void ResetPhoneUI()
    {
        if (phoneRect == null)
            return;

        phoneRect.localRotation = originRotation;
        phoneRect.anchoredPosition = originAnchoredPosition;
    }

    // =========================================================
    // Unity Events
    // =========================================================

    public void InvokeAcceptEvent()
    {
        onAcceptCall?.Invoke();
    }

    public void InvokeRejectEvent()
    {
        onRejectCall?.Invoke();
    }

    // =========================================================
    // Dialogue
    // =========================================================

    public void HandleDialogueStart(string nodeID)
    {
        currentNodeID = nodeID;

        Debug.Log(
            $"[Tutorial] NPC 대화 감지됨. 현재 단계: {currentNodeID}"
        );
    }

    // =========================================================
    // Cursor
    // =========================================================

    public void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}