using UnityEngine;

public class TutorialObject : BaseEquippable,IMouseInteractable
{
    [SerializeField] TutorialManager tutorialManager;

    bool is_clear = false;
   
    
    protected override void Awake()
    {
        base.Awake();
        if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager가 할당되지 않았습니다!");
        }
    }
    public void ClickCancle() { }

    
    public void ClickEnter()
    {
        // 1. 잡고 놓는 기능은 상태와 상관없이 언제나 실행
       
        Child_ToggleEquip();
        if (is_clear) return;
        if (tutorialManager.currentNodeID == "S0")
        {
            tutorialManager.CompleteMisson(TutorialEventType.ObjectClear);
            is_clear = true; // 이 오브젝트의 미션은 완료됨
        }
    }
    public void ClickExit() { }

    public void HoverEnter() { }

    public void HoverExit() { }

   
}
