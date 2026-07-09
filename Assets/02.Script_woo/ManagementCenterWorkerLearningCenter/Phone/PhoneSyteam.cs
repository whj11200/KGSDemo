using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PhoneSystem : MonoBehaviour
{
    [Header("Phone UI")]
    [SerializeField] private GameObject phoneObject;       // 실제 폰 UI 전체
    [SerializeField] private RectTransform phoneRect;      // 흔들릴 UI
    [SerializeField] private GameObject incomingCallUI;    // 전화 수신 화면
    [SerializeField] private GameObject darkPhoneBackGround;     // 전화 배경 화면

    [Header("Sound")]
    [SerializeField] private AudioSource ringtoneAudio;

    [Header("Vibration")]
    [SerializeField] private float rotateAngle = 4f;
    [SerializeField] private float rotateSpeed = 45f;
    [SerializeField] private float positionShake = 2f;

    [Header("Reject")]
    [SerializeField] private float recallDelay = 1.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onIncomingCall;
    [SerializeField] private UnityEvent onAcceptCall;
    [SerializeField] private UnityEvent onRejectCall;



    [SerializeField] CameraController controller;
    private Quaternion originRotation;
    private Vector2 originAnchoredPosition;

    private Coroutine vibrationCoroutine;
    private Coroutine recallCoroutine;

    private bool isCalling;
    private bool initialized;

    public bool IsCalling => isCalling;

    private void Awake()
    {
        Init();
        HidePhone();
    }
    private void OnEnable()
    {
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.StartPhoneCall.ToString(), 
            StartIncomingCall);
    }
    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(
            ManagerCenterWorkerLearningCenterEventType.StartPhoneCall.ToString(),
            StartIncomingCall
        );
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

    /// <summary>
    /// 외부에서 전화 오게 만들 때 호출.
    /// </summary>
    public void StartIncomingCall()
    {
        Init();

        if (!initialized)
            return;

        ShowCursor();

        if (darkPhoneBackGround != null)
            darkPhoneBackGround.SetActive(true);

        if (recallCoroutine != null)
        {
            StopCoroutine(recallCoroutine);
            recallCoroutine = null;
        }

        isCalling = true;

        ShowPhone();

        if (incomingCallUI != null)
            incomingCallUI.SetActive(true);

        if (ringtoneAudio != null)
        {
            ringtoneAudio.loop = true;
            ringtoneAudio.Play();
        }

        StartVibration();

        onIncomingCall?.Invoke();
    }

    /// <summary>
    /// 전화 상태 정리.
    /// 받기/끊기 둘 다 여기로 정리 가능.
    /// </summary>
    public void StopCall()
    {
        isCalling = false;

        StopVibration();

        if (ringtoneAudio != null)
            ringtoneAudio.Stop();

        if (incomingCallUI != null)
            incomingCallUI.SetActive(false);

        HidePhone();
    }

    /// <summary>
    /// 전화 거절 후 일정 시간 뒤 다시 전화.
    /// </summary>
    public void RestartCallAfterDelay()
    {
        if (recallCoroutine != null)
            StopCoroutine(recallCoroutine);

        recallCoroutine = StartCoroutine(RecallRoutine());
    }

    /// <summary>
    /// 전화 받기 이벤트 실행.
    /// </summary>
    public void InvokeAcceptEvent()
    {
        onAcceptCall?.Invoke();
    }

    /// <summary>
    /// 전화 끊기 이벤트 실행.
    /// </summary>
    public void InvokeRejectEvent()
    {
        onRejectCall?.Invoke();
    }

    private IEnumerator RecallRoutine()
    {
        yield return new WaitForSecondsRealtime(recallDelay);

        StartIncomingCall();
    }

    private void ShowPhone()
    {
        if (controller != null)
        {
            controller.isPhoneOpened = true;
        }
        if (phoneObject != null)
            phoneObject.SetActive(true);
        darkPhoneBackGround.SetActive(true);
    }

    private void HidePhone()
    {
        if (phoneObject != null)
            phoneObject.SetActive(false);
        if (controller != null)
        {
            controller.isPhoneOpened = false;
        }
          
        darkPhoneBackGround.SetActive(false);
    }

    private void StartVibration()
    {
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

            float zRotation = Mathf.Sin(time * rotateSpeed) * rotateAngle;

            float xShake = Random.Range(-positionShake, positionShake);
            float yShake = Random.Range(-positionShake, positionShake);

            phoneRect.localRotation = originRotation * Quaternion.Euler(0f, 0f, zRotation);
            phoneRect.anchoredPosition = originAnchoredPosition + new Vector2(xShake, yShake);
            ShowCursor();
            yield return null;
        }

        ResetPhoneUI();
    }

    private void ResetPhoneUI()
    {
        if (phoneRect == null)
            return;

        phoneRect.localRotation = originRotation;
        phoneRect.anchoredPosition = originAnchoredPosition;
        
    }
    public string currentNodeID { get; private set; } = "";
    public void HandleDialogueStart(string nodeID)
    {
        // 여기서 S0, S1 등을 판단해서 매니저 상태를 동기화!
        this.currentNodeID = nodeID;
        Debug.Log($"[Tutorial] NPC 대화 감지됨. 현재 단계: {currentNodeID}");
    }
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
    public void StopRingingOnly()
    {
        isCalling = false;

        StopVibration();

        if (ringtoneAudio != null)
            ringtoneAudio.Stop();

        if (incomingCallUI != null)
            incomingCallUI.SetActive(false);
    }

    public void ClosePhone()
    {
        StopCall();
        HideCursor();
    }


}