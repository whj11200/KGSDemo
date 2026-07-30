using UnityEngine;

public class OverlayUI : MonoBehaviour
{
    [SerializeField] protected Canvas Canvas;
    [SerializeField] protected Camera DesktopCam;
    [SerializeField] protected Camera VRCam;
    [SerializeField] protected RectTransform DialougeScaler;
    [SerializeField] protected float PlaneDistance = 0.45f;
    [SerializeField] protected Vector3 LocalScale = new Vector3(0.4f, 0.4f, 0.4f);
    [SerializeField] protected Vector2 Anchor = new Vector2(0.5f, 0.5f);
    [SerializeField] protected Vector2 Pivot = new Vector2(0.5f, 0.5f);
    [SerializeField] protected Vector2 AnchoredPos = new Vector2(0.5f, 0.5f);

    // 월드 캔버스용 설정
    [SerializeField] protected bool IsReadOnly = true;
    [SerializeField] protected Vector3 InteractPos = new Vector3(0, 0, 2);
    [SerializeField] protected Vector3 InteractScale = new Vector3(0.002f, 0.002f, 0.002f);

    private EPlayDevice Device => PlayerDeviceManager.PlayDevice;

    protected virtual void Awake()
    {
        if (Canvas == null)
            Canvas = GetComponent<Canvas>();

        if (Device == EPlayDevice.VR && VRCam != null)
        {
            Canvas.renderMode 
                = IsReadOnly ? RenderMode.ScreenSpaceCamera : RenderMode.WorldSpace;

            Canvas.worldCamera = VRCam;
            Canvas.planeDistance = PlaneDistance;

            Canvas.transform.SetParent(VRCam.transform, false);

            if (IsReadOnly)
            {
                if (DialougeScaler != null)
                {
                    SetDialougeScaler(DialougeScaler);
                }
            }
            else 
            {
                Canvas.transform.localPosition = InteractPos;
                Canvas.transform.localScale = InteractScale;
            }
        }
        else
        {
            Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    public virtual void SetDialougeScaler(RectTransform rt)
    {
        rt.anchorMin = rt.anchorMax = Anchor;
        rt.pivot = Pivot;
        rt.anchoredPosition = AnchoredPos;

        rt.localScale = LocalScale;
    }
}
