using UnityEngine;

public class ClickForwarder : MonoBehaviour, IMouseInteractable
{
    public IMouseInteractable Recevier;

    public void SetRecevier(IMouseInteractable recevier)
    {
        Recevier = recevier;
    }

    public void ClickEnter()
    {
        if (Recevier != null)
        {
            Recevier.ClickEnter();
        }
    }

    public void ClickExit()
    {
        if (Recevier != null)
        {
            Recevier.ClickExit();
        }
    }

    public void HoverEnter()
    {
        if (Recevier != null)
        {
            Recevier.HoverEnter();
        }
    }

    public void HoverExit()
    {
        if (Recevier != null)
        {
            Recevier.HoverExit();
        }
    }

    public void ClickCancle()
    {
         if (Recevier != null)
         {
            Recevier.ClickCancle();
         }
    }
}
