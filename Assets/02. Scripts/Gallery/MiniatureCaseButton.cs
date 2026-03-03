using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class MiniatureCaseButton : MonoBehaviour, IMouseInteractable
{
    public Transform button;
    public MeshRenderer meshRenderer;
    private Material material;
    public Color HoverColor;
    public Color ClickColor;
    public Color OriginColor;
    public Color OffColor;
    
    public DOTweenAnimation DOTween;
    [SerializeField] GameObject gasObject;
    [SerializeField] GameObject spotLight;
    bool toggle;
    static int effectNum = 0;

    private void Awake()
    {
        material = meshRenderer.materials[0];   
        OriginColor = material.color;
        OffColor = Color.red;
        toggle = gasObject.activeSelf;
    }
     
    public void ClickEnter()
    {
        material.color = ClickColor;
        DOTween.DORestart();

        toggle = !toggle;
        gasObject.SetActive(toggle);
    }

    public void ClickExit()
    {
        if (toggle)
        {
            material.color = OffColor;
        }
        else
        {
            material.color = OriginColor;
        }
    }

    public void HoverEnter()
    {
        //if (!toggle)
        //{
        //    material.color = HoverColor;
        //}
    }

    public void HoverExit()
    {
        //if (!toggle)
        //{
        //    material.color = OriginColor;
        //}
    }

    public void ClickCancle()
    {
        
    }
}
