using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRUIHoverEvent : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("XR UI Hover Events")]
    [SerializeField] private UnityEvent hoverEntered;
    [SerializeField] private UnityEvent hoverExited;

    private bool isHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스는 무시하고 XR 컨트롤러 레이만 허용
        if (eventData is not TrackedDeviceEventData)
            return;

        if (isHovering)
            return;

        isHovering = true;
        hoverEntered?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스는 무시하고 XR 컨트롤러 레이만 허용
        if (eventData is not TrackedDeviceEventData)
            return;

        if (!isHovering)
            return;

        isHovering = false;
        hoverExited?.Invoke();
    }

    private void OnDisable()
    {
        if (!isHovering)
            return;

        isHovering = false;
        hoverExited?.Invoke();
    }
}