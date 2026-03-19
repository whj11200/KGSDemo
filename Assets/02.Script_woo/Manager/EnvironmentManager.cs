using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private DialogueController dialogueController;

    [Header("Dialogue Data")]
    [SerializeField] private DialogueAsset scenarioAsset;

    [Header("Environment Objects")]
    [SerializeField] private ValveController valve;

    [SerializeField] NPC_Controller npcController;

    private void OnEnable()
    {
        DialogueEventBus.Subscribe(EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
    }

    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
    }

    private void StartGasLeakAction() => valve?.StartLeak();
    // 감지기 및 벨브 ppe 등등 미션 성공 시 해당 노드로 이동
    public void CompleteMission(EnvEventType successType)
    {
        if (dialogueController == null || scenarioAsset == null) return;

        string targetNodeId = "";

        switch (successType)
        {
            case EnvEventType.DectecorClear:
                targetNodeId = "S2";
               
                if (npcController != null) npcController.AdvanceToNextTarget();
                break;

            case EnvEventType.VavleCloseClear:
                targetNodeId = "S3";
                if (npcController != null) npcController.AdvanceToNextTarget();
                break;
            case EnvEventType.PPE_Clear:
                targetNodeId = "S7"; // PPE 다 입었을 때 나올 NPC 대사 노드
                if (npcController != null) npcController.AdvanceToNextTarget(); // 다음으로 인덱스 밀기
                break;
        }

        if (!string.IsNullOrEmpty(targetNodeId))
        {
            dialogueController.Play(scenarioAsset, targetNodeId);
        }
    }
}