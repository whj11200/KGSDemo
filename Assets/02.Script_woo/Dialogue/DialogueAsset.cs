using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

// 1. NPC 애니메이션/행동 목록 (필요한 거 다 적어두세요)
public enum NPCActionType
{
    None,           // 아무것도 안 함
    Hello, // 인사
    Explain, // 설명
    Sad, // 슬픔
    Succeed, // 정답
    StartGuide, // 다음 목적지로 이동
    EndGuide // 가이드 종료 (엔딩 연출 시작)
}


// 2. 환경/오브젝트 이벤트 목록
public enum KGS_EnvEventType
{
    None,           // 아무것도 안 함
    GasLeakStart,   // 가스 누출 시작, 감지기 가져와서 측정 => 가스감지기 1 이 뜨면 자동으로 넘어감
    DectecorClear,   // 가스 감지기 클리어
    VavleCloseClear, // 벨브 잠금 클리어
    PPE_Clear, // PPE 착용 클리어
    StudyClear // 공부 클리어 (엔딩 연출 시작)

}

public enum TutorialEventType
{
    None,
    CathingObject,
    ObjectClear,
    ScrollZoomIn,
    ScrollzoominoutClear,
    TutorilClaar
}

public enum ManagerCenterWorkerLearningCenterEventType
{
    None,
    StartPhoneCall,
    VavleCloseStage,
    StartExitStage,
    StartHazeStage,
    StartLeakStage,
    VavleOpenStage,
    EndStage
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public string dialogueId;
    public List<DialogueNode> nodes = new();

    public void ReplaceText(string key, string value)
    {
        foreach (var node in nodes)
        {
            string final = HasFinalConsonant(value) ? value + "을" : value + "를";

            if (node.text.Contains(key))
            {
                node.text = node.text.Replace(key, final);
                node.voice = PlayerDeviceManager.IsVR && node.vrVoice != null ? node.vrVoice : node.voice; 
            }

            foreach (var choice in node.choices)
            {
                if (choice.text.Contains(key))
                {
                    choice.text = choice.text.Replace(key, final);
                }
            }
        }
    }

    private bool HasFinalConsonant(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        char c = text[^1];

        if (c < '가' || c > '힣')
            return false;

        return ((c - '가') % 28) != 0;
    }
}

[Serializable]
public class DialogueNode
{
    public string nodeId;

    [TextArea(2, 6)]
    public string text;

    public string speakerId;
    public AudioClip voice;
    public AudioClip vrVoice;
    public float autoAdvanceDelay;
    public List<DialogueChoice> choices = new();
    public string nextNodeId;

    // ----- [수정된 부분: String 대신 Enum으로 분리] -----
    [Header("시작할 때 발생할 이벤트")]
    public NPCActionType npcEnterAction;
    public KGS_EnvEventType envEnterEvent;
    public TutorialEventType tutorialEnterEvent;
    public ManagerCenterWorkerLearningCenterEventType managerCenterWorkerLearningCenterEnterEvent;

    [Header("끝날 때 발생할 이벤트")]
    public NPCActionType npcExitAction;
    public KGS_EnvEventType envExitEvent;
    public TutorialEventType tutorialExitEvent;
    public ManagerCenterWorkerLearningCenterEventType managerCenterWorkerLearningCenterExitEvent;
}

[Serializable]
public class DialogueChoice
{
    public string choiceId;

    [TextArea(1, 3)]
    public string text;

    public string nextNodeId;
}
