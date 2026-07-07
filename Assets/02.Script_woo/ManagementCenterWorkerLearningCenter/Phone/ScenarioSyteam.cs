using UnityEngine;

public class ScenarioSyteam : MonoBehaviour
{
    [SerializeField] DialogueModeul dialogueModeul;
    [SerializeField] DialogueAsset scenarioAsset;
    [SerializeField] DialogueController dialogueController;

    void OnEnable()
    {
       
    }

    private void Start()
    {
        if (dialogueModeul == null)
        {
            Debug.LogError("[ScenarioManager] DialogueModeul이 할당되지 않았습니다.");
            return;
        }
        // 예시: 게임 시작 시 특정 노드에서 대화 시작
        dialogueModeul.StartDialogue();
    }
    public void HandleDialogueStart(string nodeID)
    {
        // 여기서 S0, S1 등을 판단해서 매니저 상태를 동기화!
        this.currentNodeID = nodeID;
        Debug.Log($"[Tutorial] NPC 대화 감지됨. 현재 단계: {currentNodeID}");
    }
    public string currentNodeID { get; private set; } = "";
    public void CompleteMisson(TutorialEventType type)
    {
        if (dialogueController == null || scenarioAsset == null) return;
        if (dialogueController._requestSkip) return;
        string nodeid = "";

        switch (type)
        {


            case TutorialEventType.ObjectClear:
                nodeid = "S1"; // S1 미션 완료 시

                break;
            case TutorialEventType.ScrollzoominoutClear:
                nodeid = "S2"; // S2 미션 완료 시
                break;


        }

        if (!string.IsNullOrEmpty(nodeid))
        {
            currentNodeID = nodeid; // 상태 업데이트
            dialogueController.Play(scenarioAsset, nodeid);
        }
    }
}

