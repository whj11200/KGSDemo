using UnityEngine;

public class PhoneControl : MonoBehaviour
{
    [SerializeField] private PhoneSystem phoneSystem;
    [SerializeField] private PhoneChatController phoneChatController;
    [SerializeField] private PhoneChatAsset phoneChatAsset;

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

        // 전화 벨소리, 진동, 수신 UI만 정리
        // 폰 전체를 끄면 안 됨
        phoneSystem.StopRingingOnly();

        if (phoneChatController != null && phoneChatAsset != null)
        {
            phoneChatController.Play(phoneChatAsset, OnChatFinished);
        }
        else
        {
            Debug.LogWarning("PhoneControl: PhoneChatController 또는 PhoneChatAsset이 연결되지 않았습니다.");
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

    private void OnChatFinished()
    {
        if (phoneSystem == null)
            return;

        phoneSystem.ClosePhone();
    }
}