using System.Collections;
using UnityEngine;

public enum EShipAnimation
{
    Left,
    Right
}

public class ShipAnimatorController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animator Trigger")]
    [SerializeField] private string leftTrigger = "Left";
    [SerializeField] private string rightTrigger = "Right";

    [Header("Animator State")]
    [SerializeField] private string leftStateName = "Left";
    [SerializeField] private string rightStateName = "Right";

    [Header("Default State")]
    [SerializeField] private string idleStateName = "Idle";

    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayAnimation(EShipAnimation animation)
    {
        if (animator == null)
            return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        ResetTriggers();

        string triggerName;
        string stateName;

        switch (animation)
        {
            case EShipAnimation.Left:
                triggerName = leftTrigger;
                stateName = leftStateName;
                break;

            case EShipAnimation.Right:
                triggerName = rightTrigger;
                stateName = rightStateName;
                break;

            default:
                return;
        }

        animator.SetTrigger(triggerName);

        animationCoroutine = StartCoroutine(
            WaitAnimationEnd(stateName)
        );
    }

    private IEnumerator WaitAnimationEnd(string stateName)
    {
        // 해당 애니메이션 State에 들어갈 때까지 대기
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }

        // 애니메이션 종료 대기
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        ResetTriggers();

        // Idle로 초기화
        animator.Play(idleStateName, 0, 0f);

        animationCoroutine = null;
    }

    private void ResetTriggers()
    {
        animator.ResetTrigger(leftTrigger);
        animator.ResetTrigger(rightTrigger);
    }
}