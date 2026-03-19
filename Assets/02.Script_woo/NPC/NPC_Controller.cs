using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Controller : MonoBehaviour
{
    public enum State { Idle, ReturnHome, GuideToTarget, StopMove, EndingGuide }

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform returnPos;

    [Header("Guide Targets")]
    [SerializeField] private Transform[] guideTargets;
    [SerializeField] private int targetIndex = 0;
    [SerializeField, Min(0f)] private float arriveGuideDistance = 0.5f;

    [Header("Facing")]
    [SerializeField] private bool facePlayerOnArrive = true;
    [SerializeField, Min(0f)] private float faceTurnSpeed = 720f;

    [Header("Dialogue Keys Per Target")]
    [SerializeField] private string[] guideDialogueKeys;
    private string CurrentDialogueKey =>
        (guideDialogueKeys != null && targetIndex >= 0 && targetIndex < guideDialogueKeys.Length)
        ? guideDialogueKeys[targetIndex] : null;

    public State CurrentState { get; private set; } = State.Idle;
    public NavMeshAgent Agent => agent;

    private Vector3 homePos;
    private Quaternion homeRot;
    private float nextRepathTime;
    private bool arrivedAtGuide;
    private bool dialogueTriggered;

    public event Action OnArrivedAtGuide;
    public event Action<string> OnGuideArrivedPlayerNear;
    public event Action OnEndingStarted;
    public event Action OnReturnedHome;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        homePos = returnPos ? returnPos.position : transform.position;
        homeRot = returnPos ? returnPos.rotation : transform.rotation;
    }

    private void Update()
    {
        UpdateMovement();
        UpdateFacing();
    }

    // 트리거에서 호출됨
    public void OnPlayerEnteredZone()
    {
        // [중요] NPC가 목적지에 '도착'해서 '멈춤' 상태일 때만 플레이어 진입을 허용
        if (CurrentState == State.StopMove && arrivedAtGuide)
        {
            // 이미 대사가 실행 중이면 중복 실행 방지
            if (dialogueTriggered) return;

            var key = CurrentDialogueKey;
            if (!string.IsNullOrEmpty(key))
            {
                dialogueTriggered = true;
                OnGuideArrivedPlayerNear?.Invoke(key);
                Debug.Log($"[Trigger] NPC 대기 중 플레이어 도착: {key}");
            }
        }
        else
        {
            // NPC가 아직 이동 중이거나 다른 상태일 때는 트리거를 무시함
            Debug.Log("[Trigger] NPC가 아직 도착하지 않아 트리거를 무시합니다.");
        }
    }

    public void StartGuide()
    {
        if (targetIndex >= guideTargets.Length) return;
        arrivedAtGuide = false;
        dialogueTriggered = false;
        CurrentState = State.GuideToTarget;
        agent.isStopped = false;
        agent.SetDestination(guideTargets[targetIndex].position);
    }

    public void AdvanceToNextTarget()
    {
        targetIndex++;
        if (guideTargets == null || targetIndex >= guideTargets.Length) StartEnding();
        else StartGuide();
    }

    private void UpdateMovement()
    {
        if (CurrentState == State.GuideToTarget)
        {
            if (Vector3.Distance(transform.position, guideTargets[targetIndex].position) <= arriveGuideDistance)
            {
                agent.isStopped = true;
                arrivedAtGuide = true;
                CurrentState = State.StopMove;
                OnArrivedAtGuide?.Invoke();
            }
        }
    }

    private void UpdateFacing()
    {
        if (facePlayerOnArrive && (CurrentState == State.StopMove || CurrentState == State.EndingGuide))
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), faceTurnSpeed * Time.deltaTime);
        }
    }
    public void ForceSetTargetIndex(int index)
    {
        targetIndex = index;
        dialogueTriggered = false; // 트리거 잠금도 같이 풀어줌
        arrivedAtGuide = true;    // 도착 상태 유지
        Debug.Log($"[NPC] 인덱스가 강제로 {index}로 설정되었습니다.");
    }
    public void StartEnding() { CurrentState = State.EndingGuide; OnEndingStarted?.Invoke(); }
    public void ForceReturnHome() { CurrentState = State.ReturnHome; agent.isStopped = false; agent.SetDestination(homePos); }
    public void StopMoveAndFacePlayer() { CurrentState = State.StopMove; agent.isStopped = true; }
}