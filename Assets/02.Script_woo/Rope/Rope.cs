using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 베를레 적분(Verlet Integration) 기반의 물리 로프 시스템.
/// LineRenderer로 선을 그리고, 인스턴싱으로 각 마디에 메쉬를 배치합니다.
/// </summary>
[ExecuteAlways] // <-- 추가
[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    [Header("--- Anchors (고정점 설정) ---")]
    [Tooltip("로프가 시작되는 지점의 트랜스폼")]
    [SerializeField] private Transform startAnchor;
    [Tooltip("로프 중간이 걸칠 지점의 트랜스폼 (없으면 무시)")]
    [SerializeField] private Transform middleAnchor;

    [Tooltip("로프가 끝나는 지점의 트랜스폼")]
    [SerializeField] private Transform endAnchor;
    [Tooltip("체크 시 시작점을 앵커 위치에 강제로 고정합니다.")]
    [SerializeField] private bool lockStart = true;
    [Tooltip("체크 시 끝점을 앵커 위치에 강제로 고정합니다.")]
    [SerializeField] private bool lockEnd = true;
    [Tooltip("체크 시 중간 지점을 앵커 위치에 강제로 고정합니다.")]
    [SerializeField] private bool lockMiddle = true;

    [Header("--- Instanced Mesh (마디 메쉬 설정) ---")]
    [Tooltip("각 노드(마디) 위치에 생성될 메쉬 (예: 사슬 한 칸, 실린더 등)")]
    [SerializeField] private Mesh link;
    [Tooltip("인스턴싱 전용 쉐이더가 포함된 재질")]
    [SerializeField] private Material linkMaterial;

    [Header("--- Verlet Parameters (물리 계산 변수) ---")]
    [Tooltip("노드와 노드 사이의 간격 (로프의 전체 길이 결정)")]
    [SerializeField] private float nodeDistance = 0.35f;
    [Tooltip("충돌 판정에 사용할 구체(Sphere)의 반지름")]
    [SerializeField] private float nodeColliderRadius = 0.2f;
    [Tooltip("아래로 당기는 힘의 크기")]
    [SerializeField] private float gravityStrength = 2f;
    [Tooltip("로프를 구성하는 총 마디의 개수 (많을수록 정교하지만 무거움)")]
    [SerializeField] private int totalNodes = 100;
    [Tooltip("속도 감쇠 (0: 멈춤, 1: 에너지 무한 유지) - 공기 저항 느낌")]
    [SerializeField, Range(0, 1)] private float velocityDampen = 0.95f;
    [Tooltip("로프의 팽팽함 (1에 가까울수록 늘어나지 않는 단단한 줄이 됨)")]
    [SerializeField, Range(0, 0.99f)] private float stiffness = 0.8f;
    [Tooltip("물리 연산 1회당 충돌 체크를 몇 번 수행할지 (1이면 매번 수행)")]
    [SerializeField] private int iterateCollisionsEvery = 1;
    [Tooltip("거리 제약 조건을 맞추기 위한 반복 횟수 (높을수록 로프가 덜 늘어남)")]
    [SerializeField] private int iterations = 100;
    [Tooltip("한 노드에서 동시에 처리할 수 있는 최대 충돌체 수")]
    [SerializeField] private int colliderBufferSize = 8;

    [Header("--- Line Renderer (선 외형) ---")]
    [Tooltip("LineRenderer로 그려질 로프의 굵기")]
    [SerializeField] private float ropeWidth = 0.1f;

    // 중간 마디 인덱스 계산 속성
    private int MiddleIndex => totalNodes / 2;

    [Header("--- Axis Settings (메쉬 방향 설정) ---")]
    [Tooltip("메쉬의 위쪽 방향 기준")]
    [SerializeField] private Vector3 upAxis = Vector3.up;
    [Tooltip("메쉬가 로프 진행 방향으로 뻗어있는 로컬 축 (보통 Forward)")]
    [SerializeField] private Vector3 meshForwardAxis = Vector3.forward;

    // 내부 계산용 변수들
    private LineRenderer lineRenderer;
    private Vector3[] linePositions;          // LineRenderer에 전달할 위치 배열
    private Vector3[] previousNodePositions;  // 베를레 적분용: 직전 프레임 위치
    private Vector3[] currentNodePositions;   // 현재 프레임 위치
    private Quaternion[] currentNodeRotations; // 마디 메쉬의 회전값
    private Matrix4x4[] matrices;             // GPU 인스턴싱용 변환 행렬 배열
    private Vector3 gravity;

    // 충돌 처리용
    private SphereCollider nodeCollider;      // ComputePenetration 계산용 가상 콜라이더
    private GameObject nodeTester;            // 가상 콜라이더를 담을 임시 오브젝트
    private Collider[] colliderHitBuffer;     // OverlapSphere 결과 저장용 버퍼

    public Vector3[] Nodes => currentNodePositions;
    public int NodeCount => totalNodes;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // 배열 할당: 마디 개수만큼 메모리 확보
        currentNodePositions = new Vector3[totalNodes];
        previousNodePositions = new Vector3[totalNodes];
        currentNodeRotations = new Quaternion[totalNodes];
        linePositions = new Vector3[totalNodes];
        matrices = new Matrix4x4[totalNodes];

        colliderHitBuffer = new Collider[colliderBufferSize];
        gravity = new Vector3(0, -gravityStrength, 0);

        if (nodeTester == null)
        {
            nodeTester = new GameObject("Node Tester");
            nodeTester.hideFlags = HideFlags.HideAndDontSave;
            nodeCollider = nodeTester.AddComponent<SphereCollider>();
            nodeCollider.radius = nodeColliderRadius;
        }
    }

    private void Start()
    {
        // 시작 시 앵커 위치에 맞춰 로프를 일자로 배치
        RebuildFromAnchors();
    }

    private void OnDestroy()
    {
        if (nodeTester) Destroy(nodeTester);
    }

    /// <summary>
    /// 로프를 시작점과 끝점 사이에 균등하게 재배치합니다.
    /// </summary>
    public void RebuildFromAnchors()
    {
        Vector3 a = startAnchor ? startAnchor.position : transform.position;
        Vector3 b = endAnchor ? endAnchor.position : (transform.position + Vector3.down * nodeDistance * (totalNodes - 1));

        for (int i = 0; i < totalNodes; i++)
        {
            float t = (totalNodes <= 1) ? 0f : (float)i / (totalNodes - 1);
            Vector3 p = Vector3.Lerp(a, b, t);

            currentNodePositions[i] = p;
            previousNodePositions[i] = p;
            currentNodeRotations[i] = Quaternion.identity;
            matrices[i] = Matrix4x4.TRS(p, Quaternion.identity, Vector3.one);
        }

        if (lineRenderer)
        {
            lineRenderer.positionCount = totalNodes;
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }
    }

    private void Update()
    {
        // 1. 선 그리기
        DrawRope();

        // 2. 인스턴싱을 이용해 마디 메쉬들을 한 번에 렌더링 (매우 효율적)
        if (link && linkMaterial)
            Graphics.DrawMeshInstanced(link, 0, linkMaterial, matrices, totalNodes);
    }

    private void FixedUpdate()
    {
        // 1. 물리 시뮬레이션 (이동/가속도)
        Simulate();

        // 2. 제약 조건 해결 (거리 유지 및 충돌 처리)
        // iterations 횟수만큼 반복할수록 로프가 팽팽해집니다.
        for (int i = 0; i < iterations; i++)
        {
            ApplyConstraint();

            if (iterateCollisionsEvery > 0 && (i % iterateCollisionsEvery) == 0)
                AdjustCollisions();
        }

        // 3. 렌더링을 위한 회전 및 행렬 갱신
        SetAngles();
        TranslateMatrices();
    }

    /// <summary>
    /// 베를레 적분: 별도의 물리 엔진 없이 위치 변화량으로 속도를 계산합니다.
    /// </summary>
    private void Simulate()
    {
        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < totalNodes; i++)
        {
            // 1. 시작/끝 고정점 체크
            bool isStart = (i == 0 && lockStart && startAnchor);
            bool isEnd = (i == totalNodes - 1 && lockEnd && endAnchor);
            // 2. 중간 고정점 체크 (middleAnchor가 있을 때만)
            bool isMiddle = (i == MiddleIndex && lockMiddle && middleAnchor != null);

            if (isStart || isEnd || isMiddle)
            {
                Vector3 anchorPos;
                if (isStart) anchorPos = startAnchor.position;
                else if (isEnd) anchorPos = endAnchor.position;
                else anchorPos = middleAnchor.position;

                currentNodePositions[i] = anchorPos;
                previousNodePositions[i] = anchorPos;
                continue;
            }

            // 일반 물리 계산
            Vector3 velocity = (currentNodePositions[i] - previousNodePositions[i]) * velocityDampen;
            previousNodePositions[i] = currentNodePositions[i];
            Vector3 newPos = currentNodePositions[i] + velocity + gravity * dt;
            currentNodePositions[i] = newPos;
        }
    }

    /// <summary>
    /// 노드 사이의 거리가 일정하게 유지되도록 강제로 위치를 조정합니다.
    /// </summary>
    private void ApplyConstraint()
    {
        // 위치 강제 고정 (시작, 끝, 중간)
        if (lockStart && startAnchor) currentNodePositions[0] = startAnchor.position;
        if (lockEnd && endAnchor) currentNodePositions[totalNodes - 1] = endAnchor.position;
        if (lockMiddle && middleAnchor != null) currentNodePositions[MiddleIndex] = middleAnchor.position;

        for (int i = 0; i < totalNodes - 1; i++)
        {
            Vector3 p1 = currentNodePositions[i];
            Vector3 p2 = currentNodePositions[i + 1];

            float currentDistance = (p1 - p2).magnitude;
            if (currentDistance <= 0.000001f) continue;

            float difference = currentDistance - nodeDistance;
            Vector3 direction = (p1 - p2) / currentDistance;
            Vector3 movement = direction * difference;

            // 고정 상태 확인 (중간 노드 포함)
            bool p1Locked = (i == 0 && lockStart && startAnchor) ||
                            (i == MiddleIndex && lockMiddle && middleAnchor != null);
            bool p2Locked = (i + 1 == totalNodes - 1 && lockEnd && endAnchor) ||
                            (i + 1 == MiddleIndex && lockMiddle && middleAnchor != null);

            if (!p1Locked && !p2Locked)
            {
                currentNodePositions[i] -= movement * 0.5f * stiffness;
                currentNodePositions[i + 1] += movement * 0.5f * stiffness;
            }
            else if (p1Locked && !p2Locked)
            {
                currentNodePositions[i + 1] += movement * stiffness;
            }
            else if (!p1Locked && p2Locked)
            {
                currentNodePositions[i] -= movement * stiffness;
            }
        }
    }

    /// <summary>
    /// 노드가 다른 콜라이더 내부에 들어갔을 경우 밖으로 밀어냅니다.
    /// </summary>
    private void AdjustCollisions()
    {
        for (int i = 0; i < totalNodes; i++)
        {
            if ((i == 0 && lockStart && startAnchor) || (i == totalNodes - 1 && lockEnd && endAnchor))
                continue;

            int hitCount = Physics.OverlapSphereNonAlloc(
                currentNodePositions[i],
                nodeColliderRadius + 0.01f,
                colliderHitBuffer,
                ~(1 << 8) // 8번 레이어(자신) 제외
            );

            for (int n = 0; n < hitCount; n++)
            {
                Collider other = colliderHitBuffer[n];
                if (!other) continue;

                Vector3 dir;
                float dist;

                // 겹친 방향과 깊이를 계산하여 위치 보정
                bool overlapped = Physics.ComputePenetration(
                    nodeCollider, currentNodePositions[i], Quaternion.identity,
                    other, other.transform.position, other.transform.rotation,
                    out dir, out dist
                );

                if (overlapped && dist > 0f)
                {
                    currentNodePositions[i] += dir * dist;
                }
            }
        }
    }

    /// <summary>
    /// 다음 노드를 바라보도록 각 마디의 회전값을 계산합니다.
    /// </summary>
    private void SetAngles()
    {
        Quaternion fix = Quaternion.FromToRotation(Vector3.forward, meshForwardAxis.normalized);

        for (int i = 0; i < totalNodes - 1; i++)
        {
            Vector3 dir = currentNodePositions[i + 1] - currentNodePositions[i];
            if (dir.sqrMagnitude < 1e-8f) continue;

            Quaternion rot = Quaternion.LookRotation(dir.normalized, upAxis);
            currentNodeRotations[i] = rot * Quaternion.Inverse(fix);
        }

        if (totalNodes > 1)
            currentNodeRotations[totalNodes - 1] = currentNodeRotations[totalNodes - 2];
    }

    /// <summary>
    /// 계산된 위치와 회전 정보를 GPU 인스턴싱용 행렬로 변환합니다.
    /// </summary>
    private void TranslateMatrices()
    {
        for (int i = 0; i < totalNodes; i++)
            matrices[i].SetTRS(currentNodePositions[i], currentNodeRotations[i], Vector3.one);
    }

    /// <summary>
    /// LineRenderer에 좌표 배열을 전달하여 선을 그립니다.
    /// </summary>
    private void DrawRope()
    {
        if (!lineRenderer) return;

        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        for (int i = 0; i < totalNodes; i++)
            linePositions[i] = currentNodePositions[i];

        lineRenderer.positionCount = totalNodes;
        lineRenderer.SetPositions(linePositions);
    }

    // --- 에디터 디버그용 Gizmos (기존 코드 유지) ---
#if UNITY_EDITOR
    [SerializeField] private bool debugCollisions = true;
    private readonly List<CollisionDebugInfo> collisionDebugs = new();

    private void OnDrawGizmos()
    {
        if (!debugCollisions || collisionDebugs == null) return;
        foreach (var info in collisionDebugs)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(info.nodePos, 0.03f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(info.nodePos, info.nodePos + info.pushDir * info.pushDist * 2f);
        }
    }

    struct CollisionDebugInfo
    {
        public Vector3 nodePos;
        public Vector3 colliderPos;
        public Vector3 pushDir;
        public float pushDist;
    }
#endif
}