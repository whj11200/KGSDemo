using System;
using UnityEngine;

public class NPC_StaticSpeaker : MonoBehaviour
{
    [Header("Unique ID")]
    [SerializeField] private string npcID = "NPC_A"; // 인스펙터에서 각각 다르게 설정 (예: NPC_1, NPC_2)
    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    [SerializeField] private DialogueModeul dialogueModule; // 대화창 연결

    [Header("Animation Params")]
    [SerializeField] private string helloTrigger = "hello";
    [SerializeField] private string explainTrigger = "explain";

    [Header("Facing Settings")]
    [SerializeField] private bool lookAtPlayer = false;
    [SerializeField] private float faceTurnSpeed = 5f;

    [Header("Dialogue Settings")]
    [SerializeField] private string key = "A0"; // 시작 대사 키
    [SerializeField] private DialogueAsset node;

    private bool isDialogueActive = false;
    private bool hasTriggered = false; // 일회용으로 쓸 경우 대비

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        // 대화 중이거나 설정이 켜져 있으면 플레이어를 부드럽게 바라봄
        if (lookAtPlayer && player != null)
        {
            FacePlayer();
        }
    }

    // [핵심] GuideZone(충돌체)에서 이 함수를 호출하게 하세요!
    public void OnPlayerEnteredZone()
    {
        if (hasTriggered) return; // 이미 말을 걸었다면 무시 (반복하게 하려면 제거)

        StartInteraction();
    }

    private void StartInteraction()
    {
        lookAtPlayer = true;
        isDialogueActive = true;
        hasTriggered = true;

        // 1. 인사 애니메이션
        if (animator) animator.SetTrigger(helloTrigger);

        // 2. 대화 시작
        if (dialogueModule != null)
        {
            dialogueModule.StartExplainDialogue(node,key);
        }

        Debug.Log($"{gameObject.name}: 플레이어 감지, 대화 시작!");
    }
    public void PlayExplain()
    {
        if (!isDialogueActive) return;
      animator.SetTrigger(explainTrigger);
    }
    public void OnDialogueFinished()
    {
        lookAtPlayer = false;
        isDialogueActive = false;
        Debug.Log($"{gameObject.name}: 대화 종료 및 주시 해제.");
    }

    // 기존 OnDialogueSkip을 이 함수를 호출하도록 변경
    public void OnDialogueSkip()
    {
        OnDialogueFinished();
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // 위아래로 꺾이지 않게

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * faceTurnSpeed);
        }
    }

    // 이벤트 구독 (다른 스크립트에서 부를 수 있게)
    private void OnEnable()
    {
       
        DialogueEventBus.Subscribe(NPCActionType.EndGuide.ToString(), OnDialogueSkip);
        DialogueEventBus.Subscribe(NPCActionType.Explain.ToString(), PlayExplain);
    }

    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(NPCActionType.EndGuide.ToString(), OnDialogueSkip);
        DialogueEventBus.Unsubscribe(NPCActionType.Explain.ToString(), PlayExplain);
    }
}