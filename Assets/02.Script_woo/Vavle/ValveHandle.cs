using UnityEngine;

public class ValveHandle : MonoBehaviour, IMouseInteractable
{
    [SerializeField]ValveController valveController;
    public void ClickCancle()
    {
     
    }

    public void ClickEnter()
    {
       valveController.OnInteract();
    }

    public void ClickExit()
    {
        
    }

    public void HoverEnter()
    {
      
    }

    public void HoverExit()
    {
     
    }
}
