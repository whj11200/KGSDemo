using System;
using System.Collections.Generic;
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

    [SerializeField] NPC_Interaction NPC;
    [SerializeField] NPC_Controller npcController;
    [SerializeField] List<Transform> TargetPoses = new();
    [SerializeField] CameraController PlayerController;

    public string currentNodeID { get; private set; } = "";
    private bool isScenarioFinished = false;

    private void OnEnable()
    {
        NPC_Controller.OnAnyDialogueStarted += HandleDialogueStart;
        DialogueEventBus.Subscribe(KGS_EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
        DialogueEventBus.Subscribe(KGS_EnvEventType.StudyClear.ToString(), AllClear);
    }

    private void OnDisable()
    {
        NPC_Controller.OnAnyDialogueStarted -= HandleDialogueStart;
        DialogueEventBus.Unsubscribe(KGS_EnvEventType.GasLeakStart.ToString(), StartGasLeakAction);
        DialogueEventBus.Unsubscribe(KGS_EnvEventType.StudyClear.ToString(), AllClear);
    }

    private void Start()
    {
        var SceneReq = SceneRequest.Request;

        if (SceneReq != null)
            MovePlayer(SceneReq.ContentID);
    }

    private void StartGasLeakAction() => valve?.StartLeak();

    // 감지기 및 벨브 ppe 등등 미션 성공 시 해당 노드로 이동
    public void CompleteMission(KGS_EnvEventType successType)
    {
        if (isScenarioFinished) return;

        if (dialogueController == null || scenarioAsset == null) return;
        string targetNodeId = "";

        switch (successType)
        { 
            //벨브를 열어야하는 조건이 있어서 막지 않았음
            case KGS_EnvEventType.DectecorClear:
                targetNodeId = "S2";
                break;

            case KGS_EnvEventType.VavleCloseClear:
                targetNodeId = "S3";
                break;
            case KGS_EnvEventType.PPE_Clear:
                if(currentNodeID == "A13")
                {
                    targetNodeId = "S7";
                }
                else
                {
                    Debug.Log($"현재 노드가 {currentNodeID}이므로 PPE 미션 처리를 무시합니다.");
                    return; // A13이 아니면 함수를 여기서 종료
                }
                break;
        }

        if (!string.IsNullOrEmpty(targetNodeId))
        {
            dialogueController.Play(scenarioAsset, targetNodeId);
        }
    }

    public void HandleDialogueStart(string nodeID)
    {
        // 여기서 S0, S1 등을 판단해서 매니저 상태를 동기화!
        this.currentNodeID = nodeID;
        Debug.Log($" {currentNodeID}");
    }

    public void AllClear()
    {
       
        isScenarioFinished = true;
        doorController.canOpen = true;
        valve.ResetValve();

        Debug.Log("모든 시나리오 종료. 이제 미션 완료 대사가 나오지 않습니다.");
        PlayHistoryManager.Instance?.ClearStage(EScenarioCategory.Study, 0);
    }

    public void MovePlayer(int id)
    {
        PlayerController.MoveForObject(TargetPoses[id]);
    }

    public void InitializeNPC()
    {
        NPC.HandleHello();
    }

    public void OpenContent(ContentRequest contentRequest)
    {
        MovePlayer(contentRequest.ContentID);

        AllClear();

        //if (contentRequest.ContentID == 0)
        //{
        //    InitializeNPC();

              // 가스맨 이벤트 초기화
        //    NPC.ResetAllEvents();
        //}
        //else
        //{
        //    AllClear();
        //}
    }

    // 가스맨 이벤트 초기화
    public void TestRest()
    {
       if(NPC != null)
        {
            NPC.ResetAllEvents();
        }
       else
        {
            Debug.LogWarning("NPC 잃음");
        }
           
    }
}