using UnityEngine;
using UnityEngine.Events;

public class TriggerObject : SupportXRInteractable
{
    public UnityEvent OnClickExit;

    public override void ClickCancle()
    {
        
    }

    public override void ClickEnter()
    {
        OnClickExit?.Invoke();
    }

    public override void ClickExit()
    {
        
    }

    public override void HoverEnter()
    {
        
    }

    public override void HoverExit()
    {
        
    }
}
