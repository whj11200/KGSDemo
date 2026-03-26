using UnityEngine;

public class MinmapChild : Minimapfuntioni, IMouseInteractable
{
    [SerializeField] RegionalmapInteraction reginonalmap;
    public void ClickCancle()
    {

    }

    public void ClickEnter()
    {
        if(reginonalmap != null)
        {
            reginonalmap.ToggleMap();
        }
       
    }

    public void ClickExit()
    {

    }

    public void HoverEnter()
    {
        ToggleEmission();
    }

    public void HoverExit()
    {
        ToggleEmission();
    }


}
