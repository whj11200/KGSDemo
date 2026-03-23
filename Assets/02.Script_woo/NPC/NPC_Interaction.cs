using System.Collections;
using UnityEngine;

public class NPC_Interaction : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private NPC_Controller controller;
    [SerializeField] private NPC_AnimatorDrivers animDriver;
    [SerializeField] private DialogueModeul dialogueModule;
    [SerializeField] private FadeUi fadeUi;

    [Header("Ending Timing")]
    [SerializeField, Min(0f)] private float stopFacingAfterEndingSeconds = 3f;
    [SerializeField, Min(0f)] private float returnTimer = 3f;

    private Coroutine endingRoutine;

    //  첫 가이드 시작인지 여부
    private bool guideSequenceStarted;

    private void Awake()
    {
        if (!controller) controller = GetComponent<NPC_Controller>();
        if (!animDriver) animDriver = GetComponent<NPC_AnimatorDrivers>();
    }
    private void Start()
    {
        
        if (fadeUi == null)
        {
            Debug.Log("[NPC_Interaction] FadeUi가 없으므로 직접 HandleHello를 호출합니다.");
            HandleHello();
        }
    }
    private void OnEnable()
    {
        if (controller != null)
        {
            controller.OnArrivedAtGuide += HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear += HandleGuideArrivedPlayerNear;
        }

        // ----- [수정 포인트] Enum.ToString()을 사용하여 구독 -----
        // 이제 "NPC_Explain"이라고 직접 안 쓰고 Enum 값을 문자로 바꿔서 구독합니다.

        DialogueEventBus.Subscribe(NPCActionType.StartGuide.ToString(), OnNpcStartGuide);
        DialogueEventBus.Subscribe(NPCActionType.Succeed.ToString(), Succeed);
        DialogueEventBus.Subscribe(NPCActionType.Explain.ToString(), Explaining);
        DialogueEventBus.Subscribe(NPCActionType.Hello.ToString(), HandleHello); 
        DialogueEventBus.Subscribe(NPCActionType.Sad.ToString(), PlaySadAnimation);
        // 엔딩이나 스킵 같은 공통 시스템 이벤트는 그대로 유지 가능
        DialogueEventBus.Subscribe(NPCActionType.EndGuide.ToString(), EndingGuide);
        DialogueEventBus.Subscribe("DIALOGUE_SKIP", OnDialogueSkip);

        guideSequenceStarted = false;
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnArrivedAtGuide -= HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear -= HandleGuideArrivedPlayerNear;
        }

        // 언구독도 동일하게 Enum 기준으로 진행
        DialogueEventBus.Unsubscribe(NPCActionType.StartGuide.ToString(), OnNpcStartGuide);
        DialogueEventBus.Unsubscribe(NPCActionType.Hello.ToString(), HandleHello);
        DialogueEventBus.Unsubscribe(NPCActionType.Sad.ToString(), PlaySadAnimation);
        DialogueEventBus.Unsubscribe(NPCActionType.Succeed.ToString(), Succeed);
        DialogueEventBus.Unsubscribe(NPCActionType.Explain.ToString(), Explaining);
        DialogueEventBus.Unsubscribe(NPCActionType.EndGuide.ToString(), EndingGuide);
        DialogueEventBus.Unsubscribe("DIALOGUE_SKIP", OnDialogueSkip);
    }

    // ------------------------
    // Controller-driven
    // ------------------------
    public void HandleHello()
    {
        // 페이드 UI가 끝났는지 확인 (기존 로직 유지)
        if (fadeUi != null && !fadeUi.isfinish)
        {
            return;
        }
       
       
        Debug.Log("[NPC_Interaction] HandleHello called");
        animDriver?.PlayHello();

        if (dialogueModule)
            dialogueModule.StartDialogue();

    }

    public void HandleArrivedAtGuide()
    {
        controller?.StopMoveAndFacePlayer();
    }

    public void HandleGuideArrivedPlayerNear(string dialogueKey)
    {
        if (dialogueModule && !string.IsNullOrEmpty(dialogueKey))
            dialogueModule.StartDialogueFrom(dialogueKey);
    }

    // ------------------------
    // EventBus-driven
    // ------------------------

    //  핵심: NPC_START_GUIDE 이벤트가 올 때마다 다음 타겟 진행
    private void OnNpcStartGuide()
    {
        if (!controller) return;

        if (!guideSequenceStarted)
        {
            // 첫 시작은 0번 타겟으로
            guideSequenceStarted = true;
            controller.StartGuide(); // targetIndex=0 기준
        }
        else
        {
            // 이후부터는 다음 타겟으로
            controller.AdvanceToNextTarget();
        }
    }

    public void EndingGuide()
    {
        if (endingRoutine != null) StopCoroutine(endingRoutine);

        controller?.StartEnding();
        animDriver?.PlayEnding();

        endingRoutine = StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
       // yield return new WaitForSeconds(stopFacingAfterEndingSeconds);
        //yield return new WaitForSeconds(returnTimer);

        controller?.ForceReturnHome();
        yield break;
    }
    //설명
    public void Explaining()
    {
        animDriver?.PlayExplain();
    }
    //정답
    public void Succeed()
    {
        animDriver?.PlaySucceed();
    }
    //슬픔
    public void PlaySadAnimation()
    {
        animDriver?.PlaySad();
    }


    private void OnDialogueSkip()
    {
        if (endingRoutine != null)
        {
            StopCoroutine(endingRoutine);
            endingRoutine = null;
        }

        // 스킵하면 시퀀스도 초기화하는 게 안전함
        guideSequenceStarted = false;

        controller?.ForceReturnHome();
    }
}
