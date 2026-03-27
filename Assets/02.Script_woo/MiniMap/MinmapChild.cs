using UnityEngine;

public class MinmapChild : Minimapfuntioni, IMouseInteractable
{
    [SerializeField] RegionalmapInteraction reginonalmap;

    public void ClickEnter()
    {
        if (reginonalmap != null)
        {
            reginonalmap.ToggleMap();

           
            UpdateVisual();
        }
        // 클릭 시 상태 반전 (선택됨/해제됨)
        isSelected = !isSelected;
    }

    public void ClickCancle()
    {
        isSelected = false;
        UpdateVisual();
    }

    public void ClickExit() { }

    public void HoverEnter()
    {
        isHovered = true;
        UpdateVisual();
    }

    public void HoverExit()
    {
        isHovered = false;
        UpdateVisual();
    }
}