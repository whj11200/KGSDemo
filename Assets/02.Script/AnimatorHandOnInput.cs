using UnityEngine;
using UnityEngine.InputSystem;

public class AnimatorHandOnInput : MonoBehaviour
{
    [SerializeField] InputActionProperty triggerValue;
    [SerializeField] InputActionProperty gripValue;

    [SerializeField] Animator handAnimator;




    void Update()
    {
        float gripValue = this.gripValue.action.ReadValue<float>();
        float triggerValue = this.triggerValue.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);
        handAnimator.SetFloat("Trigger", triggerValue);
    }

}
