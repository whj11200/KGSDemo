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
    }

    public override void ClickExit()
    {
        OnClickExit?.Invoke();
    }

    public override void HoverEnter()
    {
        
    }

    public override void HoverExit()
    {
        
    }
}
