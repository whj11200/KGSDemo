using System.Collections;
using UnityEngine;

public class AnjesonInteraction : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private AnjesonController controller;
    [SerializeField] private AnjesonAnimatorDriver animDriver;
    [SerializeField] private DialogueModeul dialogueModule;
    [SerializeField] private FadeUi fadeUi;

    [Header("Dialogue Keys")]
    [SerializeField] private string startDialogueMessage = "StartDialogue";

    [Header("Ending Timing")]
    [SerializeField, Min(0f)] private float stopFacingAfterEndingSeconds = 3f;
    [SerializeField, Min(0f)] private float returnTimer = 3f;

    private Coroutine endingRoutine;

    //  첫 가이드 시작인지 여부
    private bool guideSequenceStarted;

    private void Awake()
    {
        if (!controller) controller = GetComponent<AnjesonController>();
        if (!animDriver) animDriver = GetComponent<AnjesonAnimatorDriver>();
    }

    private void OnEnable()
    {
        if (controller != null)
        {
            controller.OnHelloRangeEntered += HandleHello;
            controller.OnArrivedAtGuide += HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear += HandleGuideArrivedPlayerNear;
        }

        DialogueEventBus.Subscribe("NPC_START_GUIDE", OnNpcStartGuide);
        DialogueEventBus.Subscribe("NPC_CORRECT", PlayCorrectAnimation);
        DialogueEventBus.Subscribe("NPC_End_GUIDE", EndingGuide);
        DialogueEventBus.Subscribe("NPC_Explain", Explaining);
        DialogueEventBus.Subscribe("NPC_Succed", Succeed);
        DialogueEventBus.Subscribe("DIALOGUE_SKIP", OnDialogueSkip);

        // enable될 때마다 새 시퀀스로 본다
        guideSequenceStarted = false;
    }

    private void OnDisable()
    {
        if (controller != null)
        {
            controller.OnHelloRangeEntered -= HandleHello;
            controller.OnArrivedAtGuide -= HandleArrivedAtGuide;
            controller.OnGuideArrivedPlayerNear -= HandleGuideArrivedPlayerNear;
        }

        DialogueEventBus.Unsubscribe("NPC_START_GUIDE", OnNpcStartGuide);
        DialogueEventBus.Unsubscribe("NPC_CORRECT", PlayCorrectAnimation);
        DialogueEventBus.Unsubscribe("NPC_End_GUIDE", EndingGuide);
        DialogueEventBus.Unsubscribe("NPC_Explain", Explaining);
        DialogueEventBus.Unsubscribe("NPC_Succed", Succeed);
        DialogueEventBus.Unsubscribe("DIALOGUE_SKIP", OnDialogueSkip);
    }

    // ------------------------
    // Controller-driven
    // ------------------------
    public void HandleHello()
    {
        if (fadeUi != null && !fadeUi.isfinish) return;

        animDriver?.PlayHello();

        if (dialogueModule)
            dialogueModule.SendMessage(startDialogueMessage);
    }

    public void HandleArrivedAtGuide()
    {
        controller?.StopMoveAndFacePlayer();
    }

    public void HandleGuideArrivedPlayerNear(string dialogueKey)
    {
        if (dialogueModule && !string.IsNullOrEmpty(dialogueKey))
            dialogueModule.StartDialogueFrom(dialogueKey);

        animDriver?.PlaySucceed();
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
        yield return new WaitForSeconds(stopFacingAfterEndingSeconds);
        yield return new WaitForSeconds(returnTimer);

        controller?.ForceReturnHome();
    }

    public void Explaining()
    {
        animDriver?.PlayExplain();
    }

    public void Succeed()
    {
        animDriver?.PlaySucceed();
    }

    public void PlayCorrectAnimation()
    {
        animDriver?.PlayCorrect();
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
