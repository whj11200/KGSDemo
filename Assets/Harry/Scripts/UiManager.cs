using System;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    #region Parameters
    public static UiManager Instance { get; private set; }

    public GameObject dialogueUI;
    //public InventoryUI inventoryUI;
    //public StatusUI statusUI;

    private DialogueData currentDialogueData { get; set; }

    #endregion


    #region Events
    public event Action<string> EndDialogueLine;
    public event Action<string> EndDialogue;
    #endregion

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 유지
        DontDestroyOnLoad(gameObject);
    }

    public void OnStartDialogueLine(DialogueData dialogue)
    {
        currentDialogueData = dialogue;
        currentDialogueData.DialogueIndex = 0;

        if (dialogue.DialogueType == EDialogueType.Conversation)
        {
            dialogueUI.SetActive(true);
            dialogueUI.GetComponent<DialogueUI>().OnFunction(currentDialogueData.GetCurrentStringData());
        }
    }
    public void CallNextDialogueLine(UiBase uiObject)
    {
        currentDialogueData.DialogueIndex++;
        StringData line =  currentDialogueData.GetCurrentStringData();
        if (line != null) dialogueUI.GetComponent<DialogueUI>().OnFunction(line);
        else OnEndCurrentDialogue(uiObject);

    }
    public void OnEndCurrentDialogue(UiBase uiObject)
    {
        if(uiObject != null) uiObject.gameObject.SetActive(false);

        EndDialogue?.Invoke(currentDialogueData.SpeakerID);
    }
    public void CloseUi(UiBase uiObject)
    {
        uiObject.gameObject.SetActive(false);
    }

}
