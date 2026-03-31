using UnityEngine;

public class DialogueModeul : MonoBehaviour
{
    [SerializeField] DialogueController controller;
    [SerializeField] DialogueAsset asset;
    [SerializeField] string startNodeId = "N0";

    public void StartDialogue()
    {
        if (controller.IsPlaying) return;
        controller.Play(asset, startNodeId);
    }

    //  추가: 특정 노드로 시작
    public void StartDialogueFrom(string nodeId)
    {
        if (controller.IsPlaying) return;
        controller.Play(asset, nodeId);
    }

    // 안전이 ,행복이용 디얄로그 시작 (일회용함수)
    public void StartExplainDialogue(DialogueAsset explainAsset, string explainStartNodeId)
    {
        controller.Play(explainAsset, explainStartNodeId);
    }
}
