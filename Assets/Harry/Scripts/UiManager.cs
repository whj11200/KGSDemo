using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UiManager : MonoBehaviour
{
    private GameManager gameManager;
    private DataManager dataManager;

    public static UiManager Instance { get; private set; }

    public Dictionary<string, GameObject> Uis = new Dictionary<string, GameObject>();


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

    private void Start()
    {
        Initialization();
    }

    public void Initialization()
    {
        gameManager = GameManager.Instance;
        dataManager = DataManager.Instance;

        // UI 초기화 로직 추가

        foreach(EUiName name in Enum.GetValues(typeof(EUiName)))
        {
            Transform child = transform.Find(name.ToString());
            if (child != null) Uis[name.ToString()] = child?.gameObject;
            else Debug.Log($"UiManager : There is no UI {name.ToString()}");
        }
    }

    public void OnUiSelect(string uiName = null)
    {
        foreach (var ui in Uis)
        {
            ui.Value.SetActive(false);
        }

        if (uiName != null && Uis.ContainsKey(uiName)) Uis[uiName].SetActive(true);
    }

    #region UI Select System

    #endregion

    #region Dialogue System


    public event Action<string> EndDialogueLine;
    public event Action<string> EndDialogue;
    private DialogueData currentDialogueData { get; set; }

    public void OnStartDialogueLine(DialogueData dialogue)
    {
        currentDialogueData = dialogue;
        currentDialogueData.DialogueIndex = 0;

        if (dialogue.DialogueType == EDialogueType.Conversation)
        {
            OnUiSelect(EUiName.DialogueUI.ToString());
            Uis[EUiName.DialogueUI.ToString()].GetComponent<DialogueUI>().OnFunction(currentDialogueData.GetCurrentStringData());
        }
    }
    public void CallNextDialogueLine(UiBase uiObject)
    {
        currentDialogueData.DialogueIndex++;
        StringData line = currentDialogueData.GetCurrentStringData();
        if (line != null) Uis[EUiName.DialogueUI.ToString()].GetComponent<DialogueUI>().OnFunction(line);
        else OnEndCurrentDialogue(uiObject);

    }
    public void OnEndCurrentDialogue(UiBase uiObject)
    {
        if (uiObject != null) uiObject.gameObject.SetActive(false);

        EndDialogue?.Invoke(currentDialogueData.SpeakerID);
    }
    public void CloseUi(UiBase uiObject)
    {
        uiObject.gameObject.SetActive(false);
    }
    #endregion


}
