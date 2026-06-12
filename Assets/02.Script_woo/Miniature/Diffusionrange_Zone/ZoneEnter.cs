using UnityEngine;

public class ZoneEnter : MonoBehaviour,IMouseInteractable
{
    [SerializeField]
    GameObject Text_Object;

    void Start()
    {
        Text_Object.SetActive(false);
    }
    public void ClickCancle()
    {
       
    }

    public void ClickEnter()
    {
       
    }

    public void ClickExit()
    {
      
    }

    public void HoverEnter()
    {
        Text_Object.SetActive(true);
    }

    public void HoverExit()
    {
        Text_Object.SetActive(false);
    }


}
