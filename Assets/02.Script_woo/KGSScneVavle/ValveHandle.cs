using UnityEngine;

public class ValveHandle : SupportXRInteractable
{
    [SerializeField] ValveController valveController;
    public override void ClickCancle()
    {
     
    }

    public override void ClickEnter()
    {
        valveController.OnInteract();
    }

    public override void ClickExit()
    {
    }

    public  override void HoverEnter()
    {
      
    }

    public override void HoverExit()
    {
     
    }
}
