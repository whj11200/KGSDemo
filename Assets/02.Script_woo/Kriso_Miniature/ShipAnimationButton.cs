using UnityEngine;

public class ShipAnimationButton : MonoBehaviour, IMouseInteractable
{
    [Header("Controller")]
    [SerializeField] private ShipAnimatorController controller;

    [Header("Animation")]
    [SerializeField] private EShipAnimation animationType;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<ShipAnimatorController>();
    }

    public void ClickEnter()
    {
        if (controller == null)
            return;

        controller.PlayAnimation(animationType);
    }

    public void ClickExit()
    {
    }

    public void ClickCancle()
    {
    }

    public void HoverEnter()
    {
    }

    public void HoverExit()
    {
    }
}