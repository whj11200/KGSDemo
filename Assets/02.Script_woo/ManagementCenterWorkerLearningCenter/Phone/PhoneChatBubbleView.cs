using TMPro;
using UnityEngine;

public class PhoneChatBubbleView : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject speakerRoot;

    public void SetData(string speaker, string message)
    {
        if (speakerText != null)
            speakerText.text = speaker;

        if (messageText != null)
            messageText.text = message;

        if (speakerRoot != null)
            speakerRoot.SetActive(!string.IsNullOrWhiteSpace(speaker));
    }
}