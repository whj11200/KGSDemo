using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 오른쪽 XR 컨트롤러 조이스틱을 위로 미는 동안 포물선 텔레포트 라인을 표시하고,
/// 조이스틱을 놓았을 때 유효한 바닥이면 XR Origin을 이동합니다.
///
/// 이 스크립트는 XR Interaction Toolkit의 Teleportation Provider를 사용하지 않고,
/// Unity Input System + LineRenderer + Physics.SphereCast만 사용합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class XRTeleportController : MonoBehaviour
{
    [Header("XR References")]
    [Tooltip("이동시킬 XR Origin 또는 Player Root")]
    [SerializeField] private Transform xrOrigin;

    [Tooltip("XR Origin 아래의 Main Camera")]
    [SerializeField] private Transform headCamera;

    [Tooltip("오른쪽 컨트롤러의 라인 시작 위치. 보통 Right Controller 또는 Aim Pose")]
    [SerializeField] private Transform rayOrigin;

    [Tooltip("XR Origin에 붙어 있는 CharacterController. 없으면 비워도 됩니다.")]
    [SerializeField] private CharacterController characterController;

    [Header("Input System")]
    [Tooltip("Value / Vector2 타입의 오른쪽 Primary2DAxis 액션")]
    [SerializeField] private InputActionReference teleportInputAction;

    [Range(0.1f, 1f)]
    [SerializeField] private float activationThreshold = 0.65f;

    [Tooltip("위쪽 입력이 좌우 입력보다 클 때만 텔레포트를 시작합니다.")]
    [SerializeField] private bool requireNorthDominance = true;

    [Header("Arc Settings")]
    [Min(2)]
    [SerializeField] private int sampleCount = 30;

    [Min(0.1f)]
    [SerializeField] private float launchSpeed = 9f;

    [Min(0.1f)]
    [SerializeField] private float maxFlightTime = 2f;

    [SerializeField] private Vector3 arcGravity = new Vector3(0f, -9.81f, 0f);

    [Tooltip("포물선 충돌 판정 반지름")]
    [Min(0.001f)]
    [SerializeField] private float sphereCastRadius = 0.04f;

    [Tooltip("포물선이 충돌 검사할 레이어. Player/XR Controller 레이어는 제외하세요.")]
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Teleport Surface")]
    [Tooltip("실제로 텔레포트를 허용할 바닥 레이어")]
    [SerializeField] private LayerMask teleportableFloorMask;

    [Range(0f, 89f)]
    [SerializeField] private float maximumSlopeAngle = 40f;

    [Tooltip("XR Origin을 바닥보다 살짝 위에 배치할 값")]
    [Min(0f)]
    [SerializeField] private float destinationHeightOffset = 0.02f;

    [Header("Visual Settings")]
    [Min(0.001f)]
    [SerializeField] private float lineWidth = 0.018f;

    [SerializeField] private Color validLineColor = Color.cyan;
    [SerializeField] private Color invalidLineColor = Color.red;

    [Tooltip("유효한 도착 지점에 표시되는 원의 반지름")]
    [Min(0.01f)]
    [SerializeField] private float landingCircleRadius = 0.25f;

    [Min(8)]
    [SerializeField] private int landingCircleSegments = 40;

    [Min(0.001f)]
    [SerializeField] private float landingCircleWidth = 0.02f;

    [Min(0f)]
    [SerializeField] private float landingCircleSurfaceOffset = 0.015f;

    private LineRenderer arcLine;
    private LineRenderer landingCircle;
    private GameObject landingCircleObject;

    private Vector3[] arcPoints;
    private bool isAiming;
    private bool hasValidDestination;
    private bool actionEnabledByThisComponent;

    private Vector3 destinationPosition;
    private Vector3 destinationNormal = Vector3.up;

    private void Awake()
    {
        arcLine = GetComponent<LineRenderer>();

        if (rayOrigin == null)
            rayOrigin = transform;

        if (headCamera == null && Camera.main != null)
            headCamera = Camera.main.transform;

        if (characterController == null && xrOrigin != null)
            characterController = xrOrigin.GetComponent<CharacterController>();

        sampleCount = Mathf.Max(2, sampleCount);
        landingCircleSegments = Mathf.Max(8, landingCircleSegments);
        arcPoints = new Vector3[sampleCount + 1];

        ConfigureArcLine();
        CreateLandingCircle();
        HideVisuals();
    }

    private void OnEnable()
    {
        if (teleportInputAction == null || teleportInputAction.action == null)
            return;

        teleportInputAction.action.performed += OnTeleportInputPerformed;
        teleportInputAction.action.canceled += OnTeleportInputCanceled;

        // Input Action Manager나 PlayerInput이 이미 활성화했다면 다시 관리하지 않습니다.
        if (!teleportInputAction.action.enabled)
        {
            teleportInputAction.action.Enable();
            actionEnabledByThisComponent = true;
        }
    }

    private void OnDisable()
    {
        if (teleportInputAction != null && teleportInputAction.action != null)
        {
            teleportInputAction.action.performed -= OnTeleportInputPerformed;
            teleportInputAction.action.canceled -= OnTeleportInputCanceled;

            if (actionEnabledByThisComponent)
            {
                teleportInputAction.action.Disable();
                actionEnabledByThisComponent = false;
            }
        }

        CancelTeleportAim();
    }

    private void OnDestroy()
    {
        if (landingCircleObject != null)
            Destroy(landingCircleObject);
    }

    private void Update()
    {
        if (!isAiming)
            return;

        UpdateTeleportArc();
    }

    private void OnTeleportInputPerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        if (IsNorthInput(input))
        {
            if (!isAiming)
                BeginTeleportAim();

            return;
        }

        // 스틱을 천천히 놓을 때 y 값이 임계값 아래로 내려가더라도
        // canceled 이벤트가 올 때까지 조준 상태를 유지합니다.
        // 단, 위쪽에서 좌우 또는 아래쪽으로 강하게 돌리면 텔레포트를 취소합니다.
        if (isAiming && IsStrongCancelDirection(input))
            CancelTeleportAim();
    }

    private void OnTeleportInputCanceled(InputAction.CallbackContext context)
    {
        if (!isAiming)
            return;

        if (hasValidDestination)
            ExecuteTeleport();

        CancelTeleportAim();
    }

    private bool IsNorthInput(Vector2 input)
    {
        if (input.y < activationThreshold)
            return false;

        if (requireNorthDominance && input.y < Mathf.Abs(input.x))
            return false;

        return true;
    }

    private bool IsStrongCancelDirection(Vector2 input)
    {
        bool strongSideInput =
            Mathf.Abs(input.x) >= activationThreshold &&
            Mathf.Abs(input.x) > Mathf.Max(0f, input.y);

        bool strongDownInput = input.y <= -activationThreshold;

        return strongSideInput || strongDownInput;
    }

    private void BeginTeleportAim()
    {
        isAiming = true;
        hasValidDestination = false;
        arcLine.enabled = true;
        UpdateTeleportArc();
    }

    private void CancelTeleportAim()
    {
        isAiming = false;
        hasValidDestination = false;
        HideVisuals();
    }

    private void UpdateTeleportArc()
    {
        if (rayOrigin == null)
        {
            CancelTeleportAim();
            return;
        }

        hasValidDestination = false;
        landingCircle.enabled = false;

        Vector3 startPosition = rayOrigin.position;
        Vector3 startVelocity = rayOrigin.forward * launchSpeed;

        arcPoints[0] = startPosition;
        int pointCount = 1;

        Vector3 previousPoint = startPosition;
        bool hitSomething = false;
        RaycastHit finalHit = default;

        for (int i = 1; i <= sampleCount; i++)
        {
            float time = maxFlightTime * i / sampleCount;

            Vector3 nextPoint =
                startPosition +
                startVelocity * time +
                0.5f * arcGravity * time * time;

            Vector3 segment = nextPoint - previousPoint;
            float segmentDistance = segment.magnitude;

            if (segmentDistance > Mathf.Epsilon &&
                Physics.SphereCast(
                    previousPoint,
                    sphereCastRadius,
                    segment / segmentDistance,
                    out RaycastHit hit,
                    segmentDistance,
                    collisionMask,
                    QueryTriggerInteraction.Ignore))
            {
                finalHit = hit;
                hitSomething = true;
                arcPoints[pointCount++] = hit.point;
                break;
            }

            arcPoints[pointCount++] = nextPoint;
            previousPoint = nextPoint;
        }

        arcLine.positionCount = pointCount;
        arcLine.SetPositions(arcPoints);

        if (!hitSomething)
        {
            SetArcColor(invalidLineColor);
            return;
        }

        hasValidDestination = IsValidTeleportSurface(finalHit);

        if (!hasValidDestination)
        {
            SetArcColor(invalidLineColor);
            return;
        }

        destinationPosition = finalHit.point;
        destinationNormal = finalHit.normal;

        SetArcColor(validLineColor);
        ShowLandingCircle(destinationPosition, destinationNormal);
    }

    private bool IsValidTeleportSurface(RaycastHit hit)
    {
        int hitLayerMask = 1 << hit.collider.gameObject.layer;
        bool isTeleportLayer = (teleportableFloorMask.value & hitLayerMask) != 0;

        if (!isTeleportLayer)
            return false;

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        return slopeAngle <= maximumSlopeAngle;
    }

    private void ExecuteTeleport()
    {
        if (xrOrigin == null || headCamera == null)
        {
            Debug.LogWarning("XRTeleportController: XR Origin 또는 Head Camera가 연결되지 않았습니다.", this);
            return;
        }

        // 사용자가 룸스케일로 움직인 만큼의 HMD 수평 오프셋을 유지한 채,
        // HMD의 바닥 투영점이 목표 위치에 오도록 XR Origin을 보정합니다.
        Vector3 headHorizontalOffset = headCamera.position - xrOrigin.position;
        headHorizontalOffset.y = 0f;

        Vector3 newOriginPosition = destinationPosition - headHorizontalOffset;
        newOriginPosition.y = destinationPosition.y + destinationHeightOffset;

        bool controllerWasEnabled =
            characterController != null && characterController.enabled;

        if (controllerWasEnabled)
            characterController.enabled = false;

        xrOrigin.position = newOriginPosition;

        if (controllerWasEnabled)
            characterController.enabled = true;
    }

    private void ConfigureArcLine()
    {
        arcLine.useWorldSpace = true;
        arcLine.positionCount = 0;
        arcLine.startWidth = lineWidth;
        arcLine.endWidth = lineWidth;
        arcLine.numCornerVertices = 4;
        arcLine.numCapVertices = 4;
        arcLine.enabled = false;
    }

    private void CreateLandingCircle()
    {
        landingCircleObject = new GameObject("Teleport Landing Circle");
        landingCircle = landingCircleObject.AddComponent<LineRenderer>();

        landingCircle.useWorldSpace = false;
        landingCircle.loop = true;
        landingCircle.positionCount = landingCircleSegments;
        landingCircle.startWidth = landingCircleWidth;
        landingCircle.endWidth = landingCircleWidth;
        landingCircle.numCornerVertices = 4;
        landingCircle.numCapVertices = 4;
        landingCircle.sharedMaterial = arcLine.sharedMaterial;
        landingCircle.startColor = validLineColor;
        landingCircle.endColor = validLineColor;

        for (int i = 0; i < landingCircleSegments; i++)
        {
            float angle = Mathf.PI * 2f * i / landingCircleSegments;
            Vector3 localPoint = new Vector3(
                Mathf.Cos(angle) * landingCircleRadius,
                0f,
                Mathf.Sin(angle) * landingCircleRadius);

            landingCircle.SetPosition(i, localPoint);
        }

        landingCircle.enabled = false;
    }

    private void ShowLandingCircle(Vector3 position, Vector3 normal)
    {
        landingCircleObject.transform.SetPositionAndRotation(
            position + normal * landingCircleSurfaceOffset,
            Quaternion.FromToRotation(Vector3.up, normal));

        landingCircle.enabled = true;
    }

    private void SetArcColor(Color color)
    {
        arcLine.startColor = color;
        arcLine.endColor = color;
    }

    private void HideVisuals()
    {
        if (arcLine != null)
        {
            arcLine.positionCount = 0;
            arcLine.enabled = false;
        }

        if (landingCircle != null)
            landingCircle.enabled = false;
    }
}
