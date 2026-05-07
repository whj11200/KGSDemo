using UnityEngine;

public class AnimationButton : MonoBehaviour, IMouseInteractable
{
    [Header("조정할 대상 (부모 애니메이터)")]
    public Animator targetAnimator;

    [Header("이 버튼을 누르면 재생될 컨트롤러 파일")]
    public RuntimeAnimatorController myController;


    public void ClickEnter()
    {
        PlayMyAnimation();
    }



    private void PlayMyAnimation()
    {
        if (targetAnimator == null || myController == null)
        {
            Debug.LogWarning($"{gameObject.name}: 애니메이터나 컨트롤러가 설정되지 않았습니다!");
            return;
        }

        // 복잡한 인덱스 체크 없이, 내가 가진 파일을 바로 할당!
        targetAnimator.runtimeAnimatorController = myController;
        targetAnimator.Play(0, -1, 0f);

        Debug.Log($"{gameObject.name} 클릭됨: {myController.name} 재생 시작!");
    }

    // 나머지 인터페이스 (비워둠)
    public void HoverEnter() { }
    public void HoverExit() { }
    public void ClickExit() { }
    public void ClickCancle() { }
}