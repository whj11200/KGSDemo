using UnityEngine;

public class PPE_AllResetButton : MonoBehaviour, IMouseInteractable
{
    [SerializeField] PPEGroupController ppeGroup;
    public void ClickCancle()
    {
       
    }

    public void ClickEnter()
    {
       ppeGroup.UnequipAllInGroup();
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
