using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneChatBubbleView : MonoBehaviour
{
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject speakerRoot;
    [SerializeField] private RectTransform messageBG;

    public void SetData(string speaker, string message)
    {
        if (speakerText != null)
            speakerText.text = speaker;

        if (messageText != null)
        {
            messageText.text = message;

            messageBG.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                messageText.preferredHeight
            );
        }

        if (speakerRoot != null)
            speakerRoot.SetActive(!string.IsNullOrWhiteSpace(speaker));

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageBG);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}