using System;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;
using static ValveControlConsole;

public class RemoteValveControlButton : MonoBehaviour
{
    [SerializeField] ValveControlConsole parent;
    [SerializeField] Button button;
    [SerializeField] Image Image;
    [SerializeField] bool IsOpen = true;
    [SerializeField] public ValvePhase Phase;
    [SerializeField] public bool TargetState = false;
    [SerializeField] ValveInfo ValveInfo;

    public bool IsInit = false;
    public bool IsTargetState => TargetState == IsOpen;

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
        if (parent.CurrentPhase != Phase)
            return;

        if (Phase != ValvePhase.ConfirmVent) SetValveState(!IsOpen);
        else
        {
            if (!IsOpen) SetValveState(true);
        }

        parent.OnValveStateChanged(name, IsTargetState);
    }

    public void InitValve(ValveInfo _ValveInfo, ValveOperation _valveOperation)
    {
        ValveInfo = _ValveInfo;
        TargetState = _valveOperation == ValveOperation.Isolate
            ? _ValveInfo.TargetState
            : _ValveInfo.InitialState;

        if (!IsInit) SetValveState(ValveInfo.InitialState);

        IsInit = true;
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
