using UnityEngine;

public class ValveHandle : SupportXRInteractable
{
    [SerializeField] ValveController valveController;
    public override void ClickCancle()
    {
     
    }

    public override void ClickEnter()
    {
        
    }

    public override void ClickExit()
    {
        valveController.OnInteract();
    }

    public  override void HoverEnter()
    {
      
    }

    public override void HoverExit()
    {
     
    }
}
