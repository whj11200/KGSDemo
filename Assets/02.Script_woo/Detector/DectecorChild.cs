using UnityEngine;

public class DectecorChild : MonoBehaviour, IMouseInteractable
{
    [SerializeField] LNG_Detector parentDetector;
    public void ClickCancle()
    {
      
    }

    public void ClickEnter()
    {
        parentDetector.TurnOnAndEquip();
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
