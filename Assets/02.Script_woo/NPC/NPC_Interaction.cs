using System.Collections;
using UnityEngine;

public class NPC_Interaction : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private NPC_Controller controller;
    [SerializeField] private NPC_AnimatorDrivers animDriver;
    [SerializeField] private DialogueModeul dialogueModule;
    [SerializeField] private KGSSceneFadeUi fadeUi;

    [Header("Ending Timing")]
    [SerializeField, Min(0f)] private float stopFacingAfterEndingSeconds = 3f;
    [SerializeField, Min(0f)] private float returnTimer = 3f;

    private Coroutine endingRoutine;

    // 첫 가이드 시작 여부
    private bool guideSequenceStarted;

    // 중복 구독 방지
    private bool eventsSubscribed;

    private void Awake()
    {
        if (!controller)
            controller = GetComponent<NPC_Controller>();

        if (!animDriver)
            animDriver = GetComponent<NPC_AnimatorDrivers>();
    }

    private void OnEnable()
    {
        SubscribeEvents();
        guideSequenceStarted = false;
    }

    private void OnDisable()
    {
        StopAllNpcCoroutines();
        UnsubscribeEvents();
    }

    // ==================================================
    // 이벤트 구독
    // ==================================================

    private void SubscribeEvents()
    {
        if (eventsSubscribed)
            return;

        if (controller != null)
        {
            controller.OnArrivedAtGuide += HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear += HandleGuideArrivedPlayerNear;
        }

        DialogueEventBus.Subscribe(
            NPCActionType.StartGuide.ToString(),
            OnNpcStartGuide);

        DialogueEventBus.Subscribe(
            NPCActionType.Succeed.ToString(),
            Succeed);

        DialogueEventBus.Subscribe(
            NPCActionType.Explain.ToString(),
            Explaining);

        DialogueEventBus.Subscribe(
            NPCActionType.Hello.ToString(),
            HandleHello);

        DialogueEventBus.Subscribe(
            NPCActionType.Sad.ToString(),
            PlaySadAnimation);

        DialogueEventBus.Subscribe(
            NPCActionType.EndGuide.ToString(),
            EndingGuide);

        DialogueEventBus.Subscribe(
            "DIALOGUE_SKIP",
            OnDialogueSkip);

        eventsSubscribed = true;
    }

    // ==================================================
    // 이벤트 구독 해제
    // ==================================================

    private void UnsubscribeEvents()
    {
        if (!eventsSubscribed)
            return;

        if (controller != null)
        {
            controller.OnArrivedAtGuide -= HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear -= HandleGuideArrivedPlayerNear;
        }

        DialogueEventBus.Unsubscribe(
            NPCActionType.StartGuide.ToString(),
            OnNpcStartGuide);

        DialogueEventBus.Unsubscribe(
            NPCActionType.Succeed.ToString(),
            Succeed);

        DialogueEventBus.Unsubscribe(
            NPCActionType.Explain.ToString(),
            Explaining);

        DialogueEventBus.Unsubscribe(
            NPCActionType.Hello.ToString(),
            HandleHello);

        DialogueEventBus.Unsubscribe(
            NPCActionType.Sad.ToString(),
            PlaySadAnimation);

        DialogueEventBus.Unsubscribe(
            NPCActionType.EndGuide.ToString(),
            EndingGuide);

        DialogueEventBus.Unsubscribe(
            "DIALOGUE_SKIP",
            OnDialogueSkip);

        eventsSubscribed = false;
    }

    // ==================================================
    // 전체 초기화
    // ==================================================

    public void ResetAllEvents()
    {
        Debug.Log("[NPC_Interaction] 전체 이벤트를 초기화합니다.");

        // 이 스크립트에서 실행 중인 코루틴 정지
        StopAllNpcCoroutines();

        // 가이드 진행 상태 초기화
        guideSequenceStarted = false;

        // 현재 이벤트 구독을 초기화
        UnsubscribeEvents();

        // 오브젝트가 활성화된 상태라면 다시 구독
        if (isActiveAndEnabled)
            SubscribeEvents();

        // NPC를 처음 위치로 복귀
        controller?.ForceReturnHome();

        // 초기화 직후 다시 인사 및 대화 시작
        ReStart();
    }

    private void StopAllNpcCoroutines()
    {
        StopAllCoroutines();
        endingRoutine = null;
    }

    // 초기화 후 다시 시작
    public void ReStart()
    {
        HandleHello();
    }

    // ==================================================
    // Controller 이벤트
    // ==================================================

    public void HandleHello()
    {
        Debug.Log("[NPC_Interaction] HandleHello called");

        animDriver?.PlayHello();

        if (dialogueModule != null)
            dialogueModule.StartDialogue();
    }

    public void HandleArrivedAtGuide()
    {
        controller?.StopMoveAndFacePlayer();
    }

    public void HandleGuideArrivedPlayerNear(string dialogueKey)
    {
        if (dialogueModule != null &&
            !string.IsNullOrEmpty(dialogueKey))
        {
            dialogueModule.StartDialogueFrom(dialogueKey);
        }
    }

    // ==================================================
    // EventBus 이벤트
    // ==================================================

    private void OnNpcStartGuide()
    {
        if (controller == null)
            return;

        if (!guideSequenceStarted)
        {
            guideSequenceStarted = true;
            controller.StartGuide();
        }
        else
        {
            controller.AdvanceToNextTarget();
        }
    }

    public void EndingGuide()
    {
        if (endingRoutine != null)
        {
            StopCoroutine(endingRoutine);
            endingRoutine = null;
        }

        controller?.StartEnding();
        animDriver?.PlayEnding();

        endingRoutine = StartCoroutine(EndingSequence());
    }

    private IEnumerator EndingSequence()
    {
        controller?.ForceReturnHome();

        endingRoutine = null;
        yield break;
    }

    public void Explaining()
    {
        animDriver?.PlayExplain();
    }

    public void Succeed()
    {
        animDriver?.PlaySucceed();
    }

    public void PlaySadAnimation()
    {
        animDriver?.PlaySad();
    }

    private void OnDialogueSkip()
    {
        StopAllNpcCoroutines();

        guideSequenceStarted = false;

        controller?.ForceReturnHome();
    }
}