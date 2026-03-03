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

    //  추가: 설명용 에셋/노드로 시작(권장 패턴)
    public void StartExplainDialogue(DialogueAsset explainAsset, string explainStartNodeId)
    {
        if (controller.IsPlaying) return;
        controller.Play(explainAsset, explainStartNodeId);
    }
}
