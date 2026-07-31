using UnityEngine;

public class DoorToggle : SupportXRInteractable
{
    [SerializeField] DoorController doorController; // DoorController 참조
    public override void ClickCancle()
    {
       
    }

    public override void ClickEnter()
    {
        
    }

    public override void ClickExit()
    {
        doorController.RequestDoor(); // DoorController의 토글 함수 호출
    }

    public override void HoverEnter()
    {
      
    }

    public override void HoverExit()
    {
       
    }
}
