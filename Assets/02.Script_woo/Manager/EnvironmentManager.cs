using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private DialogueController dialogueController;

    [Header("Dialogue Data")]
    [SerializeField] private DialogueAsset scenarioAsset;

    [Header("Environment Objects")]
    [SerializeField] private ValveController valve;
    [SerializeField] DoorController doorController;

    [SerializeField] NPC_Controller npcController;

    private void OnEnable()
    {
        DialogueEventBus.Subscribe(KGS_EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
        DialogueEventBus.Subscribe(KGS_EnvEventType.StudyClear.ToString(), AllClear);
    }

    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(KGS_EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
        DialogueEventBus.Unsubscribe(KGS_EnvEventType.StudyClear.ToString(), AllClear);
    }

    private void StartGasLeakAction() => valve?.StartLeak();
    // 감지기 및 벨브 ppe 등등 미션 성공 시 해당 노드로 이동
    public void CompleteMission(KGS_EnvEventType successType)
    {
        if (dialogueController == null || scenarioAsset == null) return;
        if (dialogueController._requestSkip) return;
        string targetNodeId = "";

        switch (successType)
        {
            case KGS_EnvEventType.DectecorClear:
                targetNodeId = "S2";
               
                //if (npcController != null) npcController.AdvanceToNextTarget();
                break;

            case KGS_EnvEventType.VavleCloseClear:
                targetNodeId = "S3";
                //if (npcController != null) npcController.AdvanceToNextTarget();
                break;
            case KGS_EnvEventType.PPE_Clear:
                targetNodeId = "S7"; // PPE 다 입었을 때 나올 NPC 대사 노드
                //if (npcController != null) npcController.AdvanceToNextTarget(); // 다음으로 인덱스 밀기
                break;
        }

        if (!string.IsNullOrEmpty(targetNodeId))
        {
            dialogueController.Play(scenarioAsset, targetNodeId);
        }
    }

    public void AllClear()
    {
        doorController.canOpen= true;
        valve.ResetValve();
    }
}