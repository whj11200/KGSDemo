using UnityEngine;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

public class DialogueUI : UiBase
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI MainText;
    string Text { get; set; }
    public void ClickedConfirm()
    {
        uiManager.CallNextDialogueLine(this);
    }
    public void ClickedSkip()
    {
        uiManager.OnEndCurrentDialogue(this);
    }
    public override void OnFunction(object obj = null)
    {
        if (obj is not StringData stringData)
        {
            Debug.LogError("DialogueUI: OnFunction - obj is not StringData");
            uiManager.OnEndCurrentDialogue(this);
            uiManager.CloseUi(this);
            return;
        }
        NameText.text = stringData.SpeakerID;
        MainText.text = stringData.Text_Kr;
    }
}
