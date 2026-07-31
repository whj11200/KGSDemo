using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SupportXRInteractable : MonoBehaviour, IMouseInteractable
{
    [SerializeField]
    protected Collider Collider;
    protected XRSimpleInteractable XRSimpleInteractable;

    protected virtual void Awake()
    {
        if (!PlayerDeviceManager.IsVR) return;

        if (Collider == null)
            Collider = GetComponent<Collider>();

        if (Collider != null)
            Collider.isTrigger = false;

        XRSimpleInteractable = GetComponent<XRSimpleInteractable>();

        if (XRSimpleInteractable == null)
        {
            XRSimpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        XRSimpleInteractable.hoverEntered.AddListener(_ => HoverEnter());
        XRSimpleInteractable.hoverExited.AddListener(_ => HoverExit());
        XRSimpleInteractable.activated.AddListener(_ => ClickExit());
        XRSimpleInteractable.deactivated.AddListener(_ => ClickCancle());
    }

    public virtual void ClickCancle()
    {
        
    }

    public virtual void ClickEnter()
    {
        
    }

    public virtual void ClickExit()
    {
        
    }

    public virtual void HoverEnter()
    {

    }

    public virtual void HoverExit()
    {
        
    }
}
