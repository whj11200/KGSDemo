using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhoneControl : MonoBehaviour
{
    public enum PhoneCallType
    {
        Incoming, // 상대방이 나에게 전화
        Outgoing  // 내가 상대방에게 전화
    }

    [Serializable]
    public class PhoneChatRoute
    {
        [Header("전화가 발생한 Dialogue Node ID")]
        public string fromNodeId;

        [Header("전화 종류")]
        public PhoneCallType callType = PhoneCallType.Incoming;

        [Header("실행할 Phone Chat Asset")]
        public PhoneChatAsset chatAsset;
    }

    [Header("References")]
    [SerializeField] private PhoneSystem phoneSystem;
    [SerializeField] private PhoneChatController phoneChatController;
    [SerializeField] private DialogueModeul dialogueModeul;

    [Header("Dialogue Node ID별 전화 설정")]
    [SerializeField] private List<PhoneChatRoute> phoneChatRoutes = new();

    [Header("매칭 실패 시 기본 설정")]
    [SerializeField]
    private PhoneCallType fallbackCallType =
        PhoneCallType.Incoming;

    [SerializeField] private PhoneChatAsset fallbackChatAsset;

    private PhoneCallType currentCallType;
    private PhoneChatAsset currentChatAsset;

    public PhoneCallType CurrentCallType => currentCallType;

    private void Awake()
    {
        if (phoneSystem == null)
            phoneSystem = GetComponent<PhoneSystem>();

        if (phoneChatController == null)
        {
            phoneChatController =
                GetComponentInChildren<PhoneChatController>(true);
        }
    }

    private void OnEnable()
    {
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType
                .StartPhoneCall.ToString(),
            StartCallByCurrentNode
        );
    }

    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(
            ManagerCenterWorkerLearningCenterEventType
                .StartPhoneCall.ToString(),
            StartCallByCurrentNode
        );
    }

    /// <summary>
    /// 현재 Dialogue Node ID를 확인해 수신 또는 발신 전화를 시작한다.
    /// </summary>
    public void StartCallByCurrentNode()
    {
        if (phoneSystem == null)
        {
            Debug.LogError("PhoneControl: PhoneSystem이 없습니다.");
            return;
        }

        ResolveCurrentCall();

        switch (currentCallType)
        {
            case PhoneCallType.Incoming:
                phoneSystem.StartIncomingCall();
                break;

            case PhoneCallType.Outgoing:
                phoneSystem.StartOutgoingCall();
                break;
        }
    }

    /// <summary>
    /// 수신 전화 받기 버튼.
    /// </summary>
    public void AcceptCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        if (currentCallType != PhoneCallType.Incoming)
        {
            Debug.LogWarning(
                "PhoneControl: 발신 전화에서는 AcceptCall을 사용할 수 없습니다."
            );
            return;
        }

        StartCurrentPhoneChat();

        phoneSystem.InvokeAcceptEvent();
    }

    /// <summary>
    /// 발신 전화가 연결되었을 때 호출.
    /// </summary>
    public void ConnectOutgoingCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        if (currentCallType != PhoneCallType.Outgoing)
        {
            Debug.LogWarning(
                "PhoneControl: 수신 전화에서는 ConnectOutgoingCall을 사용할 수 없습니다."
            );
            return;
        }

        StartCurrentPhoneChat();

        phoneSystem.InvokeAcceptEvent();
    }

    /// <summary>
    /// 수신 전화 거절.
    /// </summary>
    public void RejectCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        if (currentCallType != PhoneCallType.Incoming)
        {
            Debug.LogWarning(
                "PhoneControl: 발신 전화는 RejectCall이 아니라 CancelOutgoingCall을 사용하세요."
            );
            return;
        }

        phoneSystem.StopCall();
        phoneSystem.HideCursor();

        phoneSystem.InvokeRejectEvent();

        phoneSystem.RestartCallAfterDelay();
    }

    /// <summary>
    /// 내가 걸고 있는 발신 전화 취소.
    /// </summary>
    public void CancelOutgoingCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        if (currentCallType != PhoneCallType.Outgoing)
        {
            Debug.LogWarning(
                "PhoneControl: 수신 전화에서는 CancelOutgoingCall을 사용할 수 없습니다."
            );
            return;
        }

        phoneSystem.StopCall();
        phoneSystem.HideCursor();
    }

    /// <summary>
    /// 현재 Node ID에 맞는 전화 종류와 채팅 에셋을 찾는다.
    /// </summary>
    private void ResolveCurrentCall()
    {
        string currentNodeId = "";

        if (dialogueModeul != null)
            currentNodeId = dialogueModeul.CurrentDialogueNodeId;

        if (string.IsNullOrWhiteSpace(currentNodeId))
        {
            currentCallType = fallbackCallType;
            currentChatAsset = fallbackChatAsset;

            Debug.LogWarning(
                "PhoneControl: 현재 Dialogue Node ID가 없습니다. " +
                "fallback 설정을 사용합니다."
            );

            return;
        }

        PhoneChatRoute route = phoneChatRoutes.FirstOrDefault(r =>
            r != null &&
            string.Equals(
                r.fromNodeId,
                currentNodeId,
                StringComparison.Ordinal
            )
        );

        if (route != null)
        {
            currentCallType = route.callType;
            currentChatAsset = route.chatAsset;

            Debug.Log(
                $"PhoneControl: Node={currentNodeId}, " +
                $"CallType={currentCallType}"
            );

            return;
        }

        currentCallType = fallbackCallType;
        currentChatAsset = fallbackChatAsset;

        Debug.LogWarning(
            $"PhoneControl: {currentNodeId}에 맞는 전화 설정이 없습니다. " +
            "fallback 설정을 사용합니다."
        );
    }

    private void StartCurrentPhoneChat()
    {
        // 수신 벨소리 또는 발신 대기음만 정지.
        // 휴대전화 화면 자체는 유지한다.
        phoneSystem.StopRingingOnly();

        if (phoneChatController != null && currentChatAsset != null)
        {
            phoneChatController.Play(
                currentChatAsset,
                OnChatFinished
            );
        }
        else
        {
            Debug.LogWarning(
                "PhoneControl: 실행할 PhoneChatAsset이 없습니다."
            );

            OnChatFinished();
        }
    }

    private void OnChatFinished()
    {
        if (phoneSystem != null)
            phoneSystem.ClosePhone();

        currentChatAsset = null;

        if (dialogueModeul != null)
            dialogueModeul.StartNextPartByCurrentId();
    }
}