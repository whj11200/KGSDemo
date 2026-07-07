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

        if (Phase != ValvePhase.ConfirmVent) SetValveState(!IsOpen);
        else
        {
            if (!IsOpen) SetValveState(true);
        }

        parent.OnValveStateChanged(name, IsTargetState);
    }

    public void SetValveState(bool _IsOpen)
    {
        IsOpen = _IsOpen;

        var applyColor = IsOpen ? parent.OpenColor : parent.CloseColor;
        Image.color = applyColor;
    }
}

public enum ValvePhase
{
    None,
    SectionIsolation,   // SI
    SectionVent,        // SV
    Complete,
    ConfirmVent         // 벤트 밸브 재확인
}
