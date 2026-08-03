using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] DialogueController dialogueController;
    [SerializeField] DialogueAsset scenarioAsset;
    [SerializeField] SceneChanger sceneChanger;
    [SerializeField] TutorialUIManager tutorialUIManager;
    [SerializeField] NPC_Interaction NPC;

    [SerializeField] MeshRenderer InputTutoPlane;
    [SerializeField] Material MatVR;
    [SerializeField] Material MatDesktop;

    // [추가] 현재 어떤 시나리오 노드가 진행 중인지 저장
    private void OnEnable()
    {
        // NPC가 대화를 시작하면 나한테 알려달라고 등록
        NPC_Controller.OnAnyDialogueStarted += HandleDialogueStart;
        DialogueEventBus.Subscribe(TutorialEventType.TutorilClaar.ToString(), AllClear_T);
        DialogueEventBus.Subscribe(TutorialEventType.CathingObject.ToString(), tutorialUIManager.OnCatchingObject);
        DialogueEventBus.Subscribe(TutorialEventType.None.ToString(), tutorialUIManager.HideGuide);
    }

    private void OnDisable()
    {
        // 씬이 끝나거나 파괴될 때 해제 (중요!)
        NPC_Controller.OnAnyDialogueStarted -= HandleDialogueStart;
        DialogueEventBus.Unsubscribe(TutorialEventType.TutorilClaar.ToString(), AllClear_T);
    }

    private void Start()
    {
        InputTutoPlane.material = PlayerDeviceManager.IsDesktop ? MatDesktop : MatVR;

        NPC.HandleHello();
    }

    public void HandleDialogueStart(string nodeID)
    {
        // 여기서 S0, S1 등을 판단해서 매니저 상태를 동기화!
        this.currentNodeID = nodeID;
        Debug.Log($"[Tutorial] NPC 대화 감지됨. 현재 단계: {currentNodeID}");
    }
    public string currentNodeID { get; private set; } ="";
    public void CompleteMisson(TutorialEventType type)
    {
        if (dialogueController == null || scenarioAsset == null) return;
        if (dialogueController._requestSkip) return;
        string nodeid = "";

        switch (type)
        {
            

            case TutorialEventType.ObjectClear:
                nodeid = "S1"; // S1 미션 완료 시
                tutorialUIManager.OnScroll();
                break;
            case TutorialEventType.ScrollzoominoutClear:
                nodeid = "S2"; // S2 미션 완료 시
                tutorialUIManager.OnClear();
                AllClear_T();
                break;
          
            
        }

        if (!string.IsNullOrEmpty(nodeid))
        {
            currentNodeID = nodeid; // 상태 업데이트
            dialogueController.Play(scenarioAsset, nodeid);
        }
    }
    public void AllClear_T()
    {
        sceneChanger.isClear = true;
    }
}
