using UnityEngine;

public class AnjesonAnimatorDriver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnjesonController controller;
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [SerializeField] private string walkBool = "walk";
    [SerializeField] private string helloTrigger = "hello";
    [SerializeField] private string explainTrigger = "explain";
    [SerializeField] private string succeedTrigger = "succeed";
    [SerializeField] private string correctTrigger = "correct";
    [SerializeField] private string endingTrigger = "ending";

    private void Awake()
    {
        if (!controller) controller = GetComponent<AnjesonController>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (!controller) return;

        controller.OnHelloRangeEntered += PlayHello;
        controller.OnEndingStarted += PlayEnding;
        //controller.OnGuideArrivedPlayerNear += PlaySucceed; // “도착+가까움” 시 성공 연출
    }

    private void OnDisable()
    {
        if (!controller) return;

        controller.OnHelloRangeEntered -= PlayHello;
        controller.OnEndingStarted -= PlayEnding;
        //controller.OnGuideArrivedPlayerNear -= PlaySucceed;
    }

    private void Update()
    {
        if (!animator || controller == null || controller.Agent == null) return;
        animator.SetBool(walkBool, controller.Agent.velocity.sqrMagnitude > 0.05f);
    }

    // ------- public API (Interaction이 호출 가능) -------
    public void PlayHello()
    {
        if (animator) animator.SetTrigger(helloTrigger);
    }

    public void PlayExplain()
    {
        if (animator) animator.SetTrigger(explainTrigger);
    }

    public void PlayCorrect()
    {
        if (animator) animator.SetTrigger(correctTrigger);
    }

    public void PlaySucceed(string _)
    {
        PlaySucceed();
    }

    public void PlaySucceed()
    {
        if (animator) animator.SetTrigger(succeedTrigger);
    }

    public void PlayEnding()
    {
        if (animator) animator.SetTrigger(endingTrigger);
    }
}
