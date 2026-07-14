using UnityEngine;

public class DoorToggle : MonoBehaviour,IMouseInteractable
{
    [SerializeField] DoorController doorController; // DoorController 참조
    public void ClickCancle()
    {
       
    }

    public void ClickEnter()
    {
        
    }

    public void ClickExit()
    {
        doorController.RequestDoor(); // DoorController의 토글 함수 호출
    }

    public void HoverEnter()
    {
      
    }

    public void HoverExit()
    {
       
    }
}
