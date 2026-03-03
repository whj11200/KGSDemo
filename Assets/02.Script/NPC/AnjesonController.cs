using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AnjesonController : MonoBehaviour
{
    public enum State { Idle, ReturnHome, GuideToTarget, StopMove, EndingGuide }

    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform returnPos;

    [Header("Guide Targets")]
    [SerializeField] private Transform[] guideTargets;
    [SerializeField] private int targetIndex = 0;
    [SerializeField, Min(0f)] private float arriveGuideDistance = 0.4f;

    [Header("Return Home")]
    [SerializeField, Min(0f)] private float arriveHomeDistance = 0.3f;

    [Header("Hello")]
    [SerializeField, Min(0f)] private float helloDistance = 10f;

    [Header("Repath")]
    [SerializeField, Min(0.01f)] private float repathInterval = 0.15f;

    [Header("Facing")]
    [SerializeField] private bool facePlayerOnArrive = true;
    [SerializeField, Min(0f)] private float faceTurnSpeed = 720f;
    [SerializeField] private bool facePlayerBeforeHello = true;

    [Header("Explain Condition")]
    [SerializeField, Min(0f)] private float explainPlayerDistance = 2f;

    [Header("Dialogue Keys Per Target")]
    [SerializeField] private string[] guideDialogueKeys;
    private string CurrentDialogueKey =>
    (guideDialogueKeys != null &&
     targetIndex >= 0 &&
     targetIndex < guideDialogueKeys.Length)
        ? guideDialogueKeys[targetIndex]
        : null;
    // -------- runtime --------
    public State CurrentState { get; private set; } = State.Idle;
    public bool HelloTriggered { get; private set; }
    public Transform Player => player;
    public NavMeshAgent Agent => agent;

    private Vector3 homePos;
    private Quaternion homeRot;

    private float nextRepathTime;
    private float helloSqr;
    private float arriveHomeSqr;
    private float arriveGuideSqr;
    private float explainPlayerSqr;

    private Coroutine rotateHomeRoutine;

    // 도착 후 대기 플래그
    private bool arrivedAtGuide;
    private bool nearTriggered;

    private Transform CurrentTarget =>
        (guideTargets != null && targetIndex >= 0 && targetIndex < guideTargets.Length)
            ? guideTargets[targetIndex]
            : null;

    // -------- events --------
    public event Action OnHelloRangeEntered;
    public event Action OnArrivedAtGuide;
    public event Action<string> OnGuideArrivedPlayerNear;
    public event Action OnEndingStarted;
    public event Action OnReturnedHome;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        if (!returnPos)
        {
            homePos = transform.position;
            homeRot = transform.rotation;
        }
        else
        {
            homePos = returnPos.position;
            homeRot = returnPos.rotation;
        }

        CacheSqrDistances();
    }

    private void OnValidate()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        CacheSqrDistances();
    }

    private void CacheSqrDistances()
    {
        helloSqr = helloDistance * helloDistance;
        arriveHomeSqr = arriveHomeDistance * arriveHomeDistance;
        arriveGuideSqr = arriveGuideDistance * arriveGuideDistance;
        explainPlayerSqr = explainPlayerDistance * explainPlayerDistance;
    }

    private void Update()
    {
        if (!agent || !player) return;

        if (facePlayerBeforeHello && !HelloTriggered && CurrentState == State.Idle)
        {
            if (agent.updateRotation) agent.updateRotation = false;
            FaceToPlayerYawOnly();
        }

        UpdateMovement();
        UpdateFacing();
        TickNearAfterArrive();
    }

    // ---------- Public API ----------
    public void SetPlayer(Transform t) => player = t;

    public void StartGuide()
    {
        Debug.Log("이동");
        var t = CurrentTarget;
        if (!t)
        {
            Debug.LogWarning("[AnjesonController] CurrentTarget null (targets empty or index out of range)");
            return;
        }

        arrivedAtGuide = false;
        nearTriggered = false;

        CurrentState = State.GuideToTarget;
        agent.isStopped = false;
        agent.updateRotation = true;

        agent.SetDestination(t.position);
    }

    // 외부에서 특정 타겟 인덱스로 시작하고 싶으면
    public void StartGuideAt(int index)
    {
        targetIndex = Mathf.Clamp(index, 0, (guideTargets?.Length ?? 1) - 1);
        StartGuide();
    }

    public void AdvanceToNextTarget()
    {
        targetIndex++;

        if (guideTargets == null || targetIndex >= guideTargets.Length)
        {
            StartEnding();
            return;
        }

        StartGuide();
    }

    public void StartEnding()
    {
        CurrentState = State.EndingGuide;

        agent.isStopped = true;
        agent.updateRotation = false;
        if (agent.hasPath) agent.ResetPath();

        if (facePlayerOnArrive) FaceToPlayerYawOnly();
        OnEndingStarted?.Invoke();
    }

    public void ForceReturnHome()
    {
        StopRotateHomeRoutine();
        CurrentState = State.ReturnHome;

        arrivedAtGuide = false;
        nearTriggered = false;

        agent.isStopped = false;
        agent.updateRotation = true;
        if (agent.hasPath) agent.ResetPath();

        agent.SetDestination(homePos);
    }

    public void StopMoveAndFacePlayer()
    {
        CurrentState = State.StopMove;

        agent.isStopped = true;
        agent.updateRotation = false;
        if (agent.hasPath) agent.ResetPath();

        if (facePlayerOnArrive) FaceToPlayerYawOnly();
    }

    // ---------- Movement ----------
    private void UpdateMovement()
    {
        switch (CurrentState)
        {
            case State.GuideToTarget: TickGuide(); break;
            case State.ReturnHome: TickReturnHome(); break;
        }
    }

    private void TickGuide()
    {
        var t = CurrentTarget;
        if (!t)
        {
            Debug.LogWarning("[AnjesonController] CurrentTarget null -> ReturnHome");
            CurrentState = State.ReturnHome;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(homePos);
            return;
        }

        float distGuideSqr = (t.position - transform.position).sqrMagnitude;
        if (distGuideSqr <= arriveGuideSqr)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            if (agent.hasPath) agent.ResetPath();

            OnArrivedAtGuide?.Invoke();

            arrivedAtGuide = true;
            nearTriggered = false;

            CurrentState = State.StopMove;
            return;
        }

        MoveTowards(t.position);
    }

    private void TickReturnHome()
    {
        agent.isStopped = false;
        agent.updateRotation = true;

        MoveTowards(homePos);

        if (HasArrived())
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            if (agent.hasPath) agent.ResetPath();

            StopRotateHomeRoutine();
            rotateHomeRoutine = StartCoroutine(RotateToHomeRotationRoutine());

            CurrentState = State.Idle;
            OnReturnedHome?.Invoke();
        }
    }

    private void MoveTowards(Vector3 target)
    {
        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        if (agent.hasPath && (agent.destination - target).sqrMagnitude < 0.01f) return;
        agent.SetDestination(target);
    }

    private bool HasArrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f) return false;
        return true;
    }

    // ---------- Arrive -> Wait near ----------
    private void TickNearAfterArrive()
    {
        if (!arrivedAtGuide) return;
        if (nearTriggered) return;
        if (CurrentState != State.StopMove) return;

        float distPlayerSqr = (player.position - transform.position).sqrMagnitude;
        if (distPlayerSqr <= explainPlayerSqr)
        {
            nearTriggered = true;
            var key = CurrentDialogueKey;
            if (!string.IsNullOrEmpty(key))
            {
                nearTriggered = true;
                OnGuideArrivedPlayerNear?.Invoke(key);
                Debug.Log($"도착 후 대기 -> 플레이어 가까워짐 (DialogueKey: {key})");
            }
            else
            {
                Debug.LogWarning($"[NPC] DialogueKey 없음 (targetIndex={targetIndex})");
            }
         
            Debug.Log("도착 후 대기 -> 플레이어 가까워짐");
        }
    }

    // ---------- Facing ----------
    private void UpdateFacing()
    {
        if (!facePlayerOnArrive) return;
        if (!player) return;

        if (CurrentState != State.StopMove && CurrentState != State.EndingGuide) return;
        FaceToPlayerYawOnly();
    }

    public void FaceToPlayerYawOnly()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, faceTurnSpeed * Time.deltaTime);
    }

    private IEnumerator RotateToHomeRotationRoutine()
    {
        while (Quaternion.Angle(transform.rotation, homeRot) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, homeRot, faceTurnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = homeRot;
    }

    private void StopRotateHomeRoutine()
    {
        if (rotateHomeRoutine != null)
        {
            StopCoroutine(rotateHomeRoutine);
            rotateHomeRoutine = null;
        }
    }
}
