using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueModeul : MonoBehaviour
{
    [Serializable]
    public class DialogueRoute
    {
        [Header("폰 채팅이 발생한 기존 Dialogue Node ID")]
        public string fromNodeId;

        [Header("다음에 시작할 Dialogue Asset")]
        public DialogueAsset nextAsset;

        [Header("다음에 시작할 Node ID")]
        public string nextStartNodeId = "N0";
    }

    [SerializeField] private DialogueController controller;
    [SerializeField] private DialogueAsset asset;
    [SerializeField] private string startNodeId = "N0";

    [Header("폰 채팅 종료 후 자동 진행 경로")]
    [SerializeField] private List<DialogueRoute> afterPhoneChatRoutes = new();

    private string lastStartedNodeId = "";

    public string CurrentDialogueNodeId
    {
        get
        {
            if (controller != null && !string.IsNullOrWhiteSpace(controller.LastExitedNodeId))
                return controller.LastExitedNodeId;

            if (controller != null && !string.IsNullOrWhiteSpace(controller.CurrentNodeId))
                return controller.CurrentNodeId;

            return lastStartedNodeId;
        }
    }

    public void StartDialogue()
    {
        if (controller == null)
        {
            Debug.LogError("DialogueModeul: DialogueController가 없습니다.");
            return;
        }

        if (controller.IsPlaying)
            return;

        lastStartedNodeId = startNodeId;
        controller.Play(asset, startNodeId);
    }

    public void StartDialogueFrom(string nodeId)
    {
        if (controller == null)
        {
            Debug.LogError("DialogueModeul: DialogueController가 없습니다.");
            return;
        }

        if (controller.IsPlaying)
            return;

        lastStartedNodeId = nodeId;
        controller.Play(asset, nodeId);
    }

    public void StartExplainDialogue(DialogueAsset explainAsset, string explainStartNodeId)
    {
        if (controller == null)
        {
            Debug.LogError("DialogueModeul: DialogueController가 없습니다.");
            return;
        }

        lastStartedNodeId = explainStartNodeId;
        controller.Play(explainAsset, explainStartNodeId);
    }

    /// <summary>
    /// 폰 채팅이 끝났을 때 현재 Dialogue Node ID 기준으로 다음 파트 시작.
    /// </summary>
    public void StartNextPartByCurrentId()
    {
        if (controller == null)
        {
            Debug.LogError("DialogueModeul: DialogueController가 없습니다.");
            return;
        }

        if (controller.IsPlaying)
        {
            Debug.LogWarning("DialogueModeul: 아직 Dialogue가 재생 중이라 다음 파트를 시작하지 않습니다.");
            return;
        }

        string currentId = CurrentDialogueNodeId;

        if (string.IsNullOrWhiteSpace(currentId))
        {
            Debug.LogWarning("DialogueModeul: 현재 Dialogue Node ID를 찾을 수 없습니다.");
            return;
        }

        DialogueRoute route = afterPhoneChatRoutes.FirstOrDefault(r =>
            r != null && r.fromNodeId == currentId
        );

        if (route == null)
        {
            Debug.LogWarning($"DialogueModeul: '{currentId}'에 해당하는 다음 파트 경로가 없습니다.");
            return;
        }

        DialogueAsset targetAsset = route.nextAsset != null ? route.nextAsset : asset;
        string targetNodeId = string.IsNullOrWhiteSpace(route.nextStartNodeId)
            ? startNodeId
            : route.nextStartNodeId;

        if (targetAsset == null)
        {
            Debug.LogError("DialogueModeul: 실행할 DialogueAsset이 없습니다.");
            return;
        }

        lastStartedNodeId = targetNodeId;

        Debug.Log($"DialogueModeul: {currentId} 이후 다음 파트 시작 → {targetNodeId}");

        controller.Play(targetAsset, targetNodeId);
    }
}