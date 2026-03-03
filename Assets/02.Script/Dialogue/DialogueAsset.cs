using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public string dialogueId;
    public List<DialogueNode> nodes = new();
}

[Serializable]
public class DialogueNode
{
    public string nodeId;

    [TextArea(2, 6)]
    public string text;

    public string speakerId;          // "NPC_1" 같은 키
    public AudioClip voice;           // 선택
    public float autoAdvanceDelay;    // 0이면 자동 진행 안 함
    public List<DialogueChoice> choices = new();

    public string nextNodeId;         // choices 없을 때 다음 노드

    public string onEnterEvent;   //  핵심
    public string onExitEvent;
}

[Serializable]
public class DialogueChoice
{
    public string choiceId;

    [TextArea(1, 3)]
    public string text;

    public string nextNodeId;

    // 필요하면 조건/효과를 여기에 확장
    // public string conditionKey;
    // public string effectKey;
}
