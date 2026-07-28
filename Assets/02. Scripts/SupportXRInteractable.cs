using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SupportXRInteractable : MonoBehaviour, IMouseInteractable
{
    protected XRSimpleInteractable XRSimpleInteractable;

    protected virtual void Awake()
    {
        if (XRSimpleInteractable == null)
        {
            XRSimpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }

        XRSimpleInteractable.hoverEntered.AddListener(_ => HoverEnter());
        XRSimpleInteractable.hoverExited.AddListener(_ => HoverExit());
        XRSimpleInteractable.selectEntered.AddListener(_ => ClickEnter());
        XRSimpleInteractable.selectExited.AddListener(_ => ClickExit());
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
