using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    private enum PetState
    {
        Idle,
        ReturnHome,
        GuideToTarget,
        StopMove,
        EndingGuide,
        Explaining,
        Succeeding
    }

    [Header("Script")]
    [SerializeField] private DialogueModeul dialogueModule;
    [SerializeField] private FadeUi fadeUi;
    [Header("Refs")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform player;
    [SerializeField] Transform returnPos;
    [Header("Guide Target")]
    [SerializeField] private Transform guideTarget;
    [SerializeField, Min(0f)] private float arriveGuideDistance = 0.4f;

    [Header("Return Home")]
    [SerializeField, Min(0f)] private float arriveHomeDistance = 0.3f;

    [Header("Hello")]
    [SerializeField, Min(0f)] private float helloDistance = 10f;

    [Header("Repath")]
    [SerializeField, Min(0.01f)] private float repathInterval = 0.15f;

    [Header("Facing Player When Arrived")]
    [SerializeField] private bool facePlayerOnArrive = true;
    [SerializeField, Min(0f)] private float faceTurnSpeed = 720f; // deg/sec
    [SerializeField] private bool keepFacingWhileExplaining = true;

    [Header("Animator Params")]
    [SerializeField] private string walkBool = "walk";
    [SerializeField] private string helloTrigger = "hello";
    [SerializeField] private string explainTrigger = "explain";
    [SerializeField] private string succedTrigger = "succeed";  
    [SerializeField] private string correctTrigger = "correct";
    [SerializeField] private string endingTrigger = "ending";

    [Header("Explain Condition")]
    [SerializeField, Min(0f)] private float explainPlayerDistance = 2f;
    private float explainPlayerSqr;

    private Vector3 OrginPos;
    private Quaternion initialRotation;


    private Vector3 homePos;
    private Quaternion homeRot;
    
    private bool helloTriggered;
    private bool allowFacing = true;
    private float nextRepathTime;

    private float helloSqr;
    private float arriveHomeSqr;
    private float arriveGuideSqr;

    private PetState state = PetState.Idle;
    [SerializeField, Min(0f)] private float stopFacingAfterEndingSeconds = 3f;
    [SerializeField] private float ReturnTimer = 3f;

    private Coroutine stopFacingRoutine;

    void Awake()
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
            homeRot = returnPos.rotation;   // ✅ 핵심: "집의 회전"
        }

        agent.updateRotation = true;
        CacheSqrDistances();
    }


    void OnEnable()
    {
        DialogueEventBus.Subscribe("NPC_START_GUIDE", StartGuideToTarget);
        DialogueEventBus.Subscribe("NPC_CORRECT", PlayCorrectAnimation);
        DialogueEventBus.Subscribe("NPC_End_GUIDE", EndingGuide);
        DialogueEventBus.Subscribe("NPC_Explain", Explaining);
        DialogueEventBus.Subscribe("NPC_Succed", Succeed);
        DialogueEventBus.Subscribe("DIALOGUE_SKIP", OnDialogueSkip);
    }

    void OnDisable()
    {
        DialogueEventBus.Unsubscribe("NPC_START_GUIDE", StartGuideToTarget);
        DialogueEventBus.Unsubscribe("NPC_CORRECT", PlayCorrectAnimation);
        DialogueEventBus.Unsubscribe("NPC_End_GUIDE", EndingGuide);
        DialogueEventBus.Unsubscribe("NPC_Explain", Explaining);
        DialogueEventBus.Unsubscribe("NPC_Succed", Succeed);
        DialogueEventBus.Unsubscribe("DIALOGUE_SKIP", OnDialogueSkip);
    }

    void OnValidate()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        CacheSqrDistances();
    }

    void Update()
    {
        if (!player || !agent) return;

        float distToPlayerSqr = (player.position - transform.position).sqrMagnitude;

        //  Hello 터지기 전에만 플레이어 보기
        UpdatePreHelloFacing(distToPlayerSqr);

        //  그 다음 Hello 트리거 판단
        CheckHello(distToPlayerSqr);

        UpdateMovementByState();
        UpdateFacing();
        UpdateAnimation();
    }

    private void OnDialogueSkip()
    {
        // 귀가로 전환
        allowFacing = false;

        if (stopFacingRoutine != null)
        {
            StopCoroutine(stopFacingRoutine);
            stopFacingRoutine = null;
        }

        state = PetState.ReturnHome;
        agent.isStopped = false;
        agent.updateRotation = true;

        // 목적지가 남아있으면 리셋
        agent.ResetPath();

     
    }
    private void CacheSqrDistances()
    {
        helloSqr = helloDistance * helloDistance;
        arriveHomeSqr = arriveHomeDistance * arriveHomeDistance;
        arriveGuideSqr = arriveGuideDistance * arriveGuideDistance;
        explainPlayerSqr = explainPlayerDistance * explainPlayerDistance;
    }
    // -----------------------
    // Hello
    // -----------------------
    private void CheckHello(float distToPlayerSqr)
    {
        //if(!fadeUi.isfinish) return;
        if (helloTriggered) return;
        if (distToPlayerSqr > helloSqr) return;

        helloTriggered = true;
        if (animator) animator.SetTrigger(helloTrigger);
        if (dialogueModule) dialogueModule.SendMessage("StartDialogue");
        
    }

    // -----------------------
    // Movement
    // -----------------------
    private void UpdateMovementByState()
    {
        switch (state)
        {
            case PetState.GuideToTarget:
                GuideToTarget();
                break;

            case PetState.ReturnHome:
                ReturnHome();
                break;

            case PetState.StopMove:
                StopMoving();
                break;
            case PetState.Succeeding:
                Succeed();
                break;
            case PetState.Explaining:
                Explaining();
                break;
            default:
                StopMoving();
                break;
        }
    }

    private void ReturnHome()
    {
        agent.isStopped = false;
        agent.updateRotation = true;

        MoveTowards(homePos);

        if (HasArrived())
        {
            OnArrivedAtHome();
            state = PetState.Idle;
        }
    }


    private void OnArrivedAtHome()
    {
        StopMoving();                   // path 정리
        agent.isStopped = true;
        agent.updateRotation = false;   //  우리가 회전 제어

        // 코루틴 중복 방지(권장)
        if (_rotateRoutine != null) StopCoroutine(_rotateRoutine);
        _rotateRoutine = StartCoroutine(RotateToHomeRotationRoutine());
    }

    private Coroutine _rotateRoutine;

    private IEnumerator RotateToHomeRotationRoutine()
    {
        while (Quaternion.Angle(transform.rotation, homeRot) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                homeRot,
                faceTurnSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        transform.rotation = homeRot;
    }

    private void GuideToTarget()
    {
        if (!guideTarget)
        {
            state = PetState.ReturnHome;
            return;
        }

        float distGuideSqr = (guideTarget.position - transform.position).sqrMagnitude;
        if (distGuideSqr <= arriveGuideSqr)
        {
            StopMoving();

       
            agent.updateRotation = false;
            agent.isStopped = true;
            FaceToPlayerYawOnly();

            // 설명은 거리 만족할 때만
            float distPlayerSqr = (player.position - transform.position).sqrMagnitude;
            if (distPlayerSqr <= explainPlayerSqr)
            {
                QuestSucced();
            }
            return;
        }

        MoveTowards(guideTarget.position);
    }


    private void MoveTowards(Vector3 target)
    {
        if (Time.time < nextRepathTime) return;
        nextRepathTime = Time.time + repathInterval;

        if (agent.hasPath && (agent.destination - target).sqrMagnitude < 0.01f) return;
        agent.SetDestination(target);
        
    }

    private void StopMoving()
    {
        if (agent.hasPath) agent.ResetPath();
    }

    // -----------------------
    // Facing (플레이어 바라보기)
    // -----------------------
    private void UpdateFacing()
    {
        if (state != PetState.StopMove && state != PetState.EndingGuide) return;
        if (!facePlayerOnArrive) return;
        if (!player) return;
        if (!allowFacing) return;      
        
        //  추가
        // "도착 후"에만 바라보기:
        // Explaining 상태에서만 지속 회전 (원하면 Idle에서도 가능)
        if (state != PetState.StopMove) return;
        
        if (!keepFacingWhileExplaining) return;

        FaceToPlayerYawOnly();
    }

    private void FaceToPlayerYawOnly()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f; //  Y축(수평) 회전만
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            faceTurnSpeed * Time.deltaTime
           
        );
        Debug.Log("플레이어 봄");
    }
    private void UpdatePreHelloFacing(float distToPlayerSqr)
    {
        if (!facePlayerOnArrive) return;
        if (!player) return;
        if (helloTriggered) return;

        FaceToPlayerYawOnly();
    }
    // -----------------------
    // Animation
    // -----------------------
    private void UpdateAnimation()
    {
        if (!animator) return;
        animator.SetBool(walkBool, agent.velocity.sqrMagnitude > 0.05f);
    }

    private void SetWalk(bool v)
    {
        if (!animator) return;
        animator.SetBool(walkBool, v);
    }

    // -----------------------
    // Guide / Explain / Correct
    // -----------------------
    public void StartGuideToTarget()
    {
        if (!guideTarget)
        {
            Debug.LogWarning("[SimplePetFollow] guideTarget이 비어있어서 안내를 시작할 수 없음");
            return;
        }

        state = PetState.GuideToTarget;
        Debug.Log("Guide started.");
    }

    public void EndingGuide()
    {
        state = PetState.EndingGuide;           //  상태 전환
        agent.isStopped = true;
        agent.updateRotation = false;
        agent.ResetPath();

        allowFacing = true;                     //  엔딩 직후는 보게
        FaceToPlayerYawOnly();                  // 즉시 1회 회전

        if (animator) animator.SetTrigger(endingTrigger);

        if (stopFacingRoutine != null) StopCoroutine(stopFacingRoutine);
        stopFacingRoutine = StartCoroutine(EndingSequence());
    }
    private IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(stopFacingAfterEndingSeconds);
        allowFacing = false;

        yield return new WaitForSeconds(ReturnTimer);
        state = PetState.ReturnHome; 
        agent.isStopped = false;
        agent.updateRotation = true;
    }
    public void StartGuideToTarget(Transform target)
    {
        guideTarget = target;
        CacheSqrDistances();
        state = PetState.GuideToTarget;
        Debug.Log("Guide started with target.");
    }
    public void Succeed()
    {
        animator.SetTrigger(succedTrigger);
    }
    private void QuestSucced()
    {
        if (state == PetState.StopMove) return;

        state = PetState.StopMove;

        agent.updateRotation = false;
        agent.isStopped = true;
        agent.ResetPath();

        if (facePlayerOnArrive) FaceToPlayerYawOnly();
        if (dialogueModule) dialogueModule.StartDialogueFrom("S3");

        Debug.Log("Explaining...");
    }
    public void Explaining()
    {
        animator.SetTrigger(explainTrigger);
    }
    public void PlayCorrectAnimation()
    {
        if (animator) animator.SetTrigger(correctTrigger);
        Debug.Log("Correct!");

        // 정답 맞춘 뒤 정책: 여기선 Idle로
        state = PetState.Idle;
    }

    private bool HasArrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f) return false;
        return true;
    }
}
