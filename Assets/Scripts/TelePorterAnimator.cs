using UnityEngine;
using UnityEngine.UI;
//레이로 Ui로 할라면 버튼 및 이미지에 하나의 스크립트씩 넣어야함
public class TelePorterAnimator : MonoBehaviour,IMouseInteractable
{
    [SerializeField] Button Fire_D;
    [SerializeField] private Animator animator;
    void Start()
    {
        Fire_D.onClick.AddListener(ToggleAni);
    }
    public void ToggleAni()
    {
        bool currentState = animator.GetBool("Toggle");
        animator.SetBool("Toggle", !currentState);
    }

    public void TrueAni()
    {
        animator.SetBool("Toggle", true);
    }
    public void FalseAni()
    {
        animator.SetBool("Toggle", false);
    }

    public void HoverEnter()
    {

    }

    public void HoverExit()
    {

    }

    public void ClickEnter()
    {
        ToggleAni();
    }

    public void ClickExit()
    {

    }

    public void ClickCancle()
    {

    }
}
