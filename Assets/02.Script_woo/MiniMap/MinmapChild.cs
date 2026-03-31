using UnityEngine;

public class MinmapChild : Minimapfuntioni, IMouseInteractable
{
    [SerializeField] RegionalmapInteraction reginonalmap;
    [SerializeField] StructureParent structureParent;
    public override void Deselect()
    {
        // 부모의 시각적 해제 실행
        base.Deselect();

        // 자식만의 특수한 해제 로직 (맵 끄기, 리셋)
        if (reginonalmap != null) reginonalmap.ToggleMap(); // 또는 강제로 끄는 함수 호출
        if (structureParent != null) structureParent.Reset_Structure();
    }
    public void ClickEnter()
    {
        // 부모에게 선택 로직 위임
        HandleSelection();

        // 맵 토글
        if (reginonalmap != null) reginonalmap.ToggleMap();

        // 만약 방금 해제되었다면 (isSelected가 false가 되었다면) 리셋 실행
        if (!isSelected && structureParent != null)
        {
            structureParent.Reset_Structure();
        }
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