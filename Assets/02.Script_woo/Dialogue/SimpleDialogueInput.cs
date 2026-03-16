using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleDialogueInput : MonoBehaviour, IDialogueInput
{
    [SerializeField] InputActionReference nextAction;
    [SerializeField] InputActionReference skipAction;

    void OnEnable()
    {
        nextAction?.action.Enable();
        skipAction?.action.Enable();
    }

    void OnDisable()
    {
        nextAction?.action.Disable();
        skipAction?.action.Disable();
    }

    public bool NextPressed()
    {
        return nextAction != null && nextAction.action.WasPressedThisFrame();
    }

    public bool SkipPressed()
    {
        return skipAction != null && skipAction.action.WasPressedThisFrame();
    }
}
