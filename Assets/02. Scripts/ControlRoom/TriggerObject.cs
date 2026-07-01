using UnityEngine;
using UnityEngine.Events;

public class TriggerObject : MonoBehaviour, IMouseInteractable
{
    public UnityEvent OnClickExit;

    public void ClickCancle()
    {
        
    }

    public void ClickEnter()
    {
        
    }

    public void ClickExit()
    {
        OnClickExit?.Invoke();
    }

    public void HoverEnter()
    {
        
    }

    public void HoverExit()
    {
        
    }
}
