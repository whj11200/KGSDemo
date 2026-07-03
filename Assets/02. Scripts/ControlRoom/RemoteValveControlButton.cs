using System;
using UnityEngine;
using UnityEngine.UI;

public class RemoteValveControlButton : MonoBehaviour
{
    ValveControlConsole parent;
    [SerializeField] Button button;
    [SerializeField] Image Image;
    [SerializeField] bool IsOpen = true;
    [SerializeField] public int phase = -1; // 언제 조작하는 밸브인지

    private void Awake()
    {
        if (parent == null)
            parent  = GetComponentInParent<ValveControlConsole>();

        button = GetComponent<Button>();
        Image = GetComponent<Image>();

        button.onClick.AddListener(OnValveClick);
    }

    private void OnValveClick()
    {
        if (phase >= 0 && !parent.OnClickValve(name, phase))
            return;

        // Open/Close 상태 변경
        IsOpen = !IsOpen;

        // 색상 변경 
        var applyColor = IsOpen ? parent.OpenColor : parent.CloseColor;
        Image.color = applyColor;
    }
}
