using UnityEngine;
using UnityEngine.UI;
//레이로 Ui로 할라면 버튼 및 이미지에 하나의 스크립트씩 넣어야함
public class TelePorterAnimator : MonoBehaviour,IMouseInteractable
{
    [SerializeField] Button Fire_D;
    Image Fire_D_image;
    [SerializeField] private Animator animator;
    [SerializeField] Color orgincolor;
    [SerializeField] Color hovercolor;
    void Start()
    {
        Fire_D.onClick.AddListener(ToggleAni);
        orgincolor = GetComponent<Image>().color;
        hovercolor = Color.green;
        Fire_D_image = Fire_D.GetComponent<Image>();
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
        Fire_D_image.color = hovercolor;
    }

    public void HoverExit()
    {
        Fire_D_image.color = orgincolor;
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
