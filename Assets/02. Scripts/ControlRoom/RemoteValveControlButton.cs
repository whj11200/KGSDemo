using System;
using UnityEngine;
using UnityEngine.UI;

public class RemoteValveControlButton : MonoBehaviour
{
    ValveControlConsole parent;
    [SerializeField] Button button;
    [SerializeField] Image Image;
    [SerializeField] bool IsOpen = true;
    [SerializeField] public ValvePhase Phase;
    [SerializeField] public bool TargetState = false;

    public bool IsTargetState => TargetState == IsOpen;

    private void Awake()
    {
        if (parent == null)
            parent  = GetComponentInParent<ValveControlConsole>();

        button = GetComponent<Button>();
        Image = GetComponent<Image>();

        button.onClick.AddListener(OnValveClick);

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Image.color = parent.OpenColor;
        IsOpen = true;
    }

    private void OnValveClick()
    {
        if (parent.CurrentPhase != Phase)
            return;

        // Open/Close 상태 변경
        IsOpen = !IsOpen;

        // 색상 변경 
        var applyColor = IsOpen ? parent.OpenColor : parent.CloseColor;
        Image.color = applyColor;

        parent.OnValveStateChanged(name, IsTargetState);
    }
}

public enum ValvePhase
{
    None,
    SectionIsolation,   // SI
    SectionVent,        // SV
    Complete
}
