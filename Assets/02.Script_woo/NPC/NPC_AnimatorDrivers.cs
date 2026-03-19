using UnityEngine;

public class NPC_AnimatorDrivers : MonoBehaviour
{
    [SerializeField] private NPC_Controller controller;
    [SerializeField] private Animator animator;
    [SerializeField] private string walkBool = "walk";

    private void Update()
    {
        if (animator && controller && controller.Agent)
            animator.SetBool(walkBool, controller.Agent.velocity.sqrMagnitude > 0.05f);
    }

    public void PlaySucceed() => animator?.SetTrigger("succeed");
    public void PlayEnding() => animator?.SetTrigger("ending");
    public void PlayExplain() => animator?.SetTrigger("explain");
    public void PlayHello() => animator?.SetTrigger("hello");
    public void PlaySad() => animator?.SetTrigger("sad");
 
}