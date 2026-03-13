using UnityEngine;

public class SuitInteractable : MonoBehaviour, IMouseInteractable
{
    [SerializeField] PPEOneSuit ppeOneSuit;


    public void ClickCancle()
    {
      
    }

    public void ClickEnter()
    {
        ppeOneSuit.ToggleSuit();
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
