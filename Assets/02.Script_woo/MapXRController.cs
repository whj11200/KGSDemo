using UnityEngine;
using UnityEngine.InputSystem;

// XRI 3.x
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// XRI 2.x라면 위 using을 지우고 아래 사용
// using UnityEngine.XR.Interaction.Toolkit;

public class MapXRController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Teleporter teleporter;
    [SerializeField] private Transform mapRoot;
    [SerializeField] private Transform rightController;

    [Tooltip("실제 Ray가 발사되는 Transform. 비워두면 RayInteractor Transform 사용")]
    [SerializeField] private Transform rayOrigin;

    [Header("Input")]
    [Tooltip("오른쪽 컨트롤러 Trigger")]
    [SerializeField] private InputActionReference triggerAction;

    [Tooltip("오른쪽 컨트롤러 Thumbstick")]
    [SerializeField] private InputActionReference zoomAction;

    [Tooltip("맵 위치 초기화 버튼")]
    [SerializeField] private InputActionReference resetAction;

    [Header("Map Detection")]
    [SerializeField] private LayerMask layerMask;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 3f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1f;

    private Vector3 basePosition;
    private Vector3 baseScale;

    private float targetZoom = 1f;

    private bool isMapActive;
    private bool isDragging;

    private Plane dragPlane;
    private Vector3 previousDragPoint;

    private void Awake()
    {
        if (mapRoot == null)
        {
            Debug.LogError(
                $"{nameof(MapXRController)}: Map Root가 없습니다.",
                this
            );

            enabled = false;
            return;
        }

        if (rightController == null)
        {
            Debug.LogError(
                $"{nameof(MapXRController)}: Right Controller가 없습니다.",
                this
            );

            enabled = false;
            return;
        }

        // 별도 Ray Origin이 없으면 컨트롤러를 사용
        if (rayOrigin == null)
        {
            rayOrigin = rightController;
        }
        basePosition = mapRoot.position;
        baseScale = mapRoot.localScale;
        targetZoom = 1f;
    }

    private void OnEnable()
    {
        EnableAction(triggerAction);
        EnableAction(zoomAction);
        EnableAction(resetAction);

        if (resetAction != null)
        {
            resetAction.action.performed += OnResetPerformed;
        }
    }

    private void OnDisable()
    {
        if (resetAction != null)
        {
            resetAction.action.performed -= OnResetPerformed;
        }

        DisableAction(triggerAction);
        DisableAction(zoomAction);
        DisableAction(resetAction);

        EndMapInteraction();
    }

    private void Update()
    {
        if (mapRoot == null ||
            rightController == null ||
            rayOrigin == null ||
            triggerAction == null)
        {
            return;
        }

        Ray debugRay = GetControllerRay();

        Debug.DrawRay(
            debugRay.origin,
            debugRay.direction * 100f,
            Color.red
        );

        if (triggerAction.action.WasPressedThisFrame())
        {
            Debug.Log("오른쪽 Trigger 입력 감지");
        }

        bool triggerPressed = triggerAction.action.IsPressed();

        if (!triggerPressed)
        {
            EndMapInteraction();
            return;
        }

        if (!isDragging)
        {
            TryBeginMapInteraction();
        }

        if (!isMapActive)
        {
            return;
        }

        HandleZoom();
        HandlePan();
    }

    private void TryBeginMapInteraction()
    {
        Ray ray = GetControllerRay();

        Debug.DrawRay(
            ray.origin,
            ray.direction * 100f,
            Color.red
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                100f,
                layerMask,
                QueryTriggerInteraction.Collide))
        {
            Debug.Log("Ray가 아무 Collider도 감지하지 못함");
            return;
        }

        Debug.Log(
            $"Ray 충돌 성공: {hit.collider.name} / " +
            $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}"
        );

        GlobalBoxClipController mapController =
            hit.collider.GetComponentInParent<GlobalBoxClipController>();

        if (mapController == null)
        {
            Debug.LogWarning(
                $"{hit.collider.name}은 맞았지만 " +
                $"부모에 GlobalBoxClipController가 없습니다."
            );

            return;
        }

        isMapActive = true;
        isDragging = true;

        dragPlane = new Plane(
            mapRoot.up,
            hit.point
        );

        if (!TryGetDragPoint(out previousDragPoint))
        {
            previousDragPoint = hit.point;
        }

        Debug.Log("맵 조작 시작");
    }

    private void HandleZoom()
    {
        if (zoomAction == null)
        {
            return;
        }

        Vector2 stickValue = zoomAction.action.ReadValue<Vector2>();
        float zoomInput = stickValue.y;

        if (Mathf.Abs(zoomInput) < 0.1f)
        {
            return;
        }

        targetZoom += zoomInput * zoomSpeed * Time.deltaTime;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        mapRoot.localScale = baseScale * targetZoom;
    }

    private void HandlePan()
    {
        if (!isDragging)
        {
            return;
        }

        if (!TryGetDragPoint(out Vector3 currentDragPoint))
        {
            return;
        }
        if(teleporter.isButtonActive)
        {
            return; // 텔레포트 UI가 켜져 있을 때는 패닝 방지
        }


        Vector3 dragDelta = currentDragPoint - previousDragPoint;

        // 드래그 평면의 수직 방향 이동 제거
        dragDelta = Vector3.ProjectOnPlane(
            dragDelta,
            dragPlane.normal
        );

        float zoomFactor = 1f / Mathf.Max(targetZoom, 0.0001f);

        mapRoot.position += dragDelta * panSpeed * zoomFactor;

        previousDragPoint = currentDragPoint;

        Debug.Log("XR 맵 움직임");
    }

    private bool TryGetDragPoint(out Vector3 dragPoint)
    {
        Ray ray = GetControllerRay();

        if (dragPlane.Raycast(ray, out float distance))
        {
            dragPoint = ray.GetPoint(distance);
            return true;
        }

        dragPoint = default;
        return false;
    }

    private void EndMapInteraction()
    {
        isMapActive = false;
        isDragging = false;
    }

    private void OnResetPerformed(InputAction.CallbackContext context)
    {
        ResetMapPosition();
    }

    public void ResetMapPosition()
    {
        mapRoot.position = basePosition;
        mapRoot.localScale = baseScale;

        targetZoom = 1f;

        EndMapInteraction();
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null &&
            !actionReference.action.enabled)
        {
            actionReference.action.Enable();
        }
    }

    private static void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null &&
            actionReference.action.enabled)
        {
            actionReference.action.Disable();
        }
    }
    private Ray GetControllerRay()
    {
        Transform origin = rayOrigin != null
            ? rayOrigin
            : rightController;

        return new Ray(
            origin.position,
            origin.forward
        );
    }
}