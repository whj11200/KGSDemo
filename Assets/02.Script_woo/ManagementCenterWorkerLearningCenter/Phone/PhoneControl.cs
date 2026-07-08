using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhoneControl : MonoBehaviour
{
    [Serializable]
    public class PhoneChatRoute
    {
        [Header("전화가 발생한 Dialogue Node ID")]
        public string fromNodeId;

        [Header("실행할 Phone Chat Asset")]
        public PhoneChatAsset chatAsset;
    }

    [SerializeField] private PhoneSystem phoneSystem;
    [SerializeField] private PhoneChatController phoneChatController;
    [SerializeField] private DialogueModeul dialogueModeul;

    [Header("Dialogue Node ID별 Phone Chat Asset")]
    [SerializeField] private List<PhoneChatRoute> phoneChatRoutes = new();

    [Header("매칭 실패 시 사용할 기본 채팅")]
    [SerializeField] private PhoneChatAsset fallbackChatAsset;

    private void Awake()
    {
        if (phoneSystem == null)
            phoneSystem = GetComponent<PhoneSystem>();

        if (phoneChatController == null)
            phoneChatController = GetComponentInChildren<PhoneChatController>(true);
    }

    public void AcceptCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        // 전화 벨소리, 진동, 수신 UI만 끔.
        // PhoneObject 자체는 끄면 안 됨.
        phoneSystem.StopRingingOnly();

        PhoneChatAsset chatAsset = GetCurrentPhoneChatAsset();

        if (phoneChatController != null && chatAsset != null)
        {
            phoneChatController.Play(chatAsset, OnChatFinished);
        }
        else
        {
            Debug.LogWarning("PhoneControl: 실행할 PhoneChatAsset이 없습니다.");
            OnChatFinished();
        }

        phoneSystem.InvokeAcceptEvent();
    }

    public void RejectCall()
    {
        if (phoneSystem == null)
            return;

        if (!phoneSystem.IsCalling)
            return;

        phoneSystem.StopCall();
        phoneSystem.HideCursor();

        phoneSystem.InvokeRejectEvent();

        phoneSystem.RestartCallAfterDelay();
    }

    private PhoneChatAsset GetCurrentPhoneChatAsset()
    {
        string currentNodeId = "";

        if (dialogueModeul != null)
            currentNodeId = dialogueModeul.CurrentDialogueNodeId;

        if (string.IsNullOrWhiteSpace(currentNodeId))
        {
            Debug.LogWarning("PhoneControl: 현재 Dialogue Node ID가 없습니다. fallbackChatAsset을 사용합니다.");
            return fallbackChatAsset;
        }

        PhoneChatRoute route = phoneChatRoutes.FirstOrDefault(r =>
            r != null && r.fromNodeId == currentNodeId
        );

        if (route != null && route.chatAsset != null)
        {
            Debug.Log($"PhoneControl: {currentNodeId}에 맞는 PhoneChatAsset 실행");
            return route.chatAsset;
        }

        Debug.LogWarning($"PhoneControl: {currentNodeId}에 맞는 PhoneChatAsset이 없습니다. fallbackChatAsset을 사용합니다.");
        return fallbackChatAsset;
    }

    private void OnChatFinished()
    {
        if (phoneSystem != null)
            phoneSystem.ClosePhone();

        if (dialogueModeul != null)
            dialogueModeul.StartNextPartByCurrentId();
    }
}