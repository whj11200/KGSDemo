using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MapMouseController : MonoBehaviour
{
    public Transform mapRoot;

    public InputActionReference zoomAction;     // Mouse Scroll
    public InputActionReference panAction;      // Mouse Delta
    public InputActionReference panButtonAction;// Right Click
    public InputActionReference resetAction;    // Keyboard R

    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;
    public float panSpeed = 0.01f;
    Vector3 basePosition;
    Vector3 baseScale;
    float baseY;
    private Mouse mouse;
    private Camera mainCamera;
    float targetZoom = 1f;

    private void Awake()
    {
        if(mapRoot != null)
        {
            baseScale = mapRoot.localScale;
            basePosition = mapRoot.position;
            baseY = basePosition.y;
            mainCamera = Camera.main;
        }
      
    }

    void OnEnable()
    {
        mouse = Mouse.current;

        zoomAction.action.Enable();
        panAction.action.Enable();
        panButtonAction.action.Enable();
        resetAction.action.Enable();    

        resetAction.action.performed += ctx => ResetMapPosition();
    }

    void OnDisable()
    {
        zoomAction.action.Disable();
        panAction.action.Disable();
        panButtonAction.action.Disable();
        resetAction.action.Disable();

        resetAction.action.performed -= ctx => ResetMapPosition();
    }

    void Update()
    {
        if (mouse == null || mainCamera == null)
            return;

        if (IsPointerOverUI())
            return;

        UpdateMapActiveState();

        if (!isMapActive)
            return;

        HandleZoom();

        if (panButtonAction.action.IsPressed())
        {
            HandlePan();
        }
    }

    void HandleZoom()
    {
        if (!mouse.rightButton.isPressed) return;
        Debug.Log("HandleZoom called");

        float scroll = zoomAction.action.ReadValue<Vector2>().y;
        Debug.Log($"Zoom Scroll Value: {scroll}");  
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom += scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            mapRoot.localScale = baseScale * targetZoom;
        }
    }

    void HandlePan()
    {
        if (!panButtonAction.action.IsPressed() || !mouse.rightButton.isPressed)
            return;

        if (targetZoom <= 1f)
            return;

        Vector2 delta = panAction.action.ReadValue<Vector2>();

        Camera cam = Camera.main;

        // 카메라 기준 평면 방향 벡터
        Vector3 right = cam.transform.right;
        Vector3 forward = cam.transform.forward;

        right.y = 0f;
        forward.y = 0f;
        right.Normalize();
        forward.Normalize();

        Vector3 move =
            (right * delta.x) +
            (forward * delta.y);

        float zoomFactor = 1f / targetZoom;
        mapRoot.position += move * panSpeed * zoomFactor;
    }

    void ResetMapPosition()
    {
     
        mapRoot.position = basePosition;
        mapRoot.localScale = baseScale;
        targetZoom = 1f;
    }

    bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    bool isMapActive;
    [SerializeField] LayerMask layerMask;
    void UpdateMapActiveState()
    {
        if (!mouse.rightButton.isPressed)
        {
            isMapActive = false;
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 100f, layerMask))
        {
            isMapActive = hit.collider.GetComponent<GlobalBoxClipController>() != null;
        }
        else
        {
            isMapActive = false;
        }
    }
}
