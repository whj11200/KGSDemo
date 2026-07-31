using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SupportXRInteractable : MonoBehaviour, IMouseInteractable
{
    protected XRSimpleInteractable XRSimpleInteractable;

    protected virtual void Awake()
    {
        if (!PlayerDeviceManager.IsVR) return;

        XRSimpleInteractable = GetComponent<XRSimpleInteractable>();

        if (XRSimpleInteractable == null)
        {
            XRSimpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        XRSimpleInteractable.hoverEntered.AddListener(_ => HoverEnter());
        XRSimpleInteractable.hoverExited.AddListener(_ => HoverExit());
        XRSimpleInteractable.activated.AddListener(_ => ClickEnter());
        XRSimpleInteractable.deactivated.AddListener(_ => ClickExit());
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
