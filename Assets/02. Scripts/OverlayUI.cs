using UnityEngine;

public abstract class OverlayUI : MonoBehaviour
{
    [SerializeField] protected Canvas Canvas;
    [SerializeField] protected Camera DesktopCam;
    [SerializeField] protected Camera VRCam;
    [SerializeField] protected RectTransform DialougeScaler;
    [SerializeField] protected float PlaneDistance = 0.45f;
    [SerializeField] protected float LocalScale = 0.5f;

    private EPlayDevice Device => PlayerDeviceManager.PlayDevice;

    protected virtual void Awake()
    {
        if (Canvas == null)
            Canvas = GetComponent<Canvas>();

        if (Device == EPlayDevice.VR && VRCam != null)
        {
            Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            Canvas.worldCamera = VRCam;
            Canvas.planeDistance = PlaneDistance;

            if (DialougeScaler != null)
            {
                SetDialougeScaler(DialougeScaler);
            }
        }
        else
        {
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    public virtual void SetDialougeScaler(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        rt.localScale = new Vector3(LocalScale, LocalScale, LocalScale);
    }
}
