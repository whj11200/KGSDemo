using UnityEngine;

public class SuitInteractable : MonoBehaviour, IMouseInteractable
{
    //[SerializeField] PPEOneSuit ppeOneSuit;
    [SerializeField] PPEPartController ppePartController;

    public void ClickCancle()
    {
      
    }

    public void ClickEnter()
    {
        //if(ppeOneSuit != null)
        //{
        //    ppeOneSuit.ToggleSuit();
        //}
        if(ppePartController != null)
        {
            ppePartController.TogglePart();
        }

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
