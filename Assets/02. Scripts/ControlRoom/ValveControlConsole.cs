using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class ValveControlConsole : MonoBehaviour
{
    [SerializeField] List<ValveInfo> TargetValves = new();
    [SerializeField] List<ValveInfo> VentValves = new();

    [SerializeField] private ScenarioType ScenarioType;
    [SerializeField] List<RemoteValveControlButton> Buttons = new();
    [SerializeField] List<ButtonRoot> ButtonRootsList = new();
    Dictionary<ScenarioType, GameObject> ButtonRoots = new();
    Dictionary<string, ValveInfo> valveInfos_Control = new();
    Dictionary<string, ValveInfo> valveInfos_Vent = new();

    [SerializeField] public Color OpenColor;
    [SerializeField] public Color CloseColor;

    public event Action<int> OnPhaseComplete;
    public event Action OnControlComplete;

    [SerializeField] private ValvePhase currentPhase = ValvePhase.None;
    public ValvePhase CurrentPhase => currentPhase;

    public ValveOperation CurrentOperation { get; private set; }
    private Dictionary<string, bool> valveStates = new();

    private void Awake()
    {
        foreach (var roots in ButtonRootsList)
        {
            ButtonRoots[roots.ScenarioType] = roots.Root;
        }
    }

    private void OnEnable()
    {
        
    }

    public void InitValveConsole(ScenarioAsset asset)
    {
        ScenarioType = asset.Template.ScenarioType;

        var root = ButtonRoots[ScenarioType];
        root.gameObject.SetActive(true);

        Buttons.Clear();
        Buttons = root.GetComponentsInChildren<RemoteValveControlButton>(true).ToList();

        TargetValves = asset.Valves_SectionIsolation
                        .Concat(asset.Valves_SectionVent)
                        .ToList();

        VentValves =  asset.OverrideVentValveList ? asset.Valves_SectionVent : asset.Template.VentValves;

        valveInfos_Control.Clear(); 
        valveInfos_Vent.Clear();

        foreach (var valve in TargetValves)
            valveInfos_Control[valve.Name] = valve;

        foreach (var valve in VentValves)
            valveInfos_Vent[valve.Name] = valve;
    }

    public void SetTargetValve(ValveOperation operation)
    {
        CurrentOperation = operation;

        valveStates.Clear();

        foreach (var button in Buttons)
        {
            if (!valveInfos_Control.TryGetValue(button.name, out var info))
            {
                button.Phase = ValvePhase.None;
                button.GetComponent<Image>().raycastTarget = false;
                continue;
            }

            button.GetComponent<Image>().raycastTarget = true;

            button.InitValve(info, operation);
            button.Phase = info.Phase;

            valveStates[button.name] = button.IsTargetState;
        }

        SetStartPhase();
    }

    private void SetStartPhase()
    {
        bool hasSI = TargetValves.Any(v => v.Phase == ValvePhase.SectionIsolation);
        bool hasSV = TargetValves.Any(v => v.Phase == ValvePhase.SectionVent);

        switch (CurrentOperation)
        {
            case ValveOperation.Isolate:
                if (hasSI)
                    currentPhase = ValvePhase.SectionIsolation;
                else if (hasSV)
                    currentPhase = ValvePhase.SectionVent;
                else
                    currentPhase = ValvePhase.Complete;
                break;

            case ValveOperation.Restore:
                if (hasSV)
                    currentPhase = ValvePhase.SectionVent;
                else if (hasSI)
                    currentPhase = ValvePhase.SectionIsolation;
                else
                    currentPhase = ValvePhase.Complete;
                break;
        }
    }

    public void ConfirmVent()
    {
        if (Buttons == null || Buttons.Count == 0)
            return;

        CurrentOperation = ValveOperation.Confirm;
        currentPhase = ValvePhase.ConfirmVent;

        var targets = Buttons.Where(b => VentValves.Any(v => v.Name == b.name))
                             .ToList();

        valveStates.Clear();

        bool hasFault = UnityEngine.Random.value < 0.3f;

        foreach (var button in targets)
        {
            var info = valveInfos_Vent[button.name];

            button.GetComponent<Image>().raycastTarget = true;
            button.Phase = ValvePhase.ConfirmVent;

            button.TargetState = info.TargetState;
            button.SetValveState(info.InitialState);

            valveStates[button.name] = button.IsTargetState;
        }

        if (hasFault && targets.Count > 0)
        {
            var random = targets[UnityEngine.Random.Range(0, targets.Count)];
            random.SetValveState(!random.TargetState);

            valveStates[random.name] = random.IsTargetState;
        }
    }

    public void OnValveStateChanged(string valveName, bool isTargetState)
    {
        if (CurrentOperation == ValveOperation.Confirm)
        {
            if (!valveInfos_Vent.ContainsKey(valveName))
                return;

            valveStates[valveName] = isTargetState;
            bool confirmComplete = valveInfos_Vent.Keys.All(v => valveStates[v]);

            if (confirmComplete)
                CompleteControl();
        }
        else
        {
            // 대상 밸브가 아니면 무시
            if (!valveStates.ContainsKey(valveName))
                return;

            // 현재 상태 갱신
            valveStates[valveName] = isTargetState;

            bool siComplete = TargetValves
                .Where(v => v.Phase == ValvePhase.SectionIsolation)
                .All(v => valveStates[v.Name]);

            bool svComplete = TargetValves
                .Where(v => v.Phase == ValvePhase.SectionVent)
                .All(v => valveStates[v.Name]);

            switch (CurrentOperation)
            {
                case ValveOperation.Isolate:
                    HandleIsolate(siComplete, svComplete);
                    break;

                case ValveOperation.Restore:
                    HandleRestore(siComplete, svComplete);
                    break;
            }
        }
    }

    private void HandleIsolate(bool siComplete, bool svComplete)
    {
        if (!siComplete)
        {
            currentPhase = ValvePhase.SectionIsolation;
            return;
        }

        if (!svComplete)
        {
            if (currentPhase != ValvePhase.SectionVent)
            {
                currentPhase = ValvePhase.SectionVent;
                OnPhaseComplete?.Invoke(2);
            }
            return;
        }

        CompleteControl();
    }

    private void HandleRestore(bool siComplete, bool svComplete)
    {
        if (!svComplete)
        {
            currentPhase = ValvePhase.SectionVent;
            return;
        }

        if (!siComplete)
        {
            if (currentPhase != ValvePhase.SectionIsolation)
            {
                currentPhase = ValvePhase.SectionIsolation;
                OnPhaseComplete?.Invoke(1);
            }
            return;
        }

        CompleteControl();
    }

    private void CompleteControl()
    {
        if (currentPhase != ValvePhase.Complete)
        {
            currentPhase = ValvePhase.Complete;
            OnControlComplete?.Invoke();
        }
    }

    public void AllButtonsDisable()
    {
        foreach(var btn in Buttons)
        {
            btn.GetComponent<Image>().raycastTarget = false;
        }
    }

    public enum ValveOperation
    {
        Isolate,    // 차단
        Restore,    // 복구
        Confirm
    }

    [System.Serializable]
    public struct ButtonRoot
    {
        public ScenarioType ScenarioType;
        public GameObject Root;
    }
}
