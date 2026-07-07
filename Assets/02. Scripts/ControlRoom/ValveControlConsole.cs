using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class ValveControlConsole : MonoBehaviour
{
    [SerializeField] List<string> TargetValves_SI = new();
    [SerializeField] List<string> TargetValves_SV = new();

    [SerializeField] private ScenarioType ScenarioType;
    [SerializeField] List<RemoteValveControlButton> Buttons = new();
    [SerializeField] List<ButtonRoot> ButtonRootsList = new();
    Dictionary<ScenarioType, GameObject> ButtonRoots = new();

    [SerializeField] public Color OpenColor;
    [SerializeField] public Color CloseColor;

    public event Action<int> OnPhaseComplete;
    public event Action OnControlComplete;

    [SerializeField] private ValvePhase currentPhase = ValvePhase.None;
    public ValvePhase CurrentPhase => currentPhase;

    public ValveOperation CurrentOperation { get; private set; }
    [SerializeField] private bool CurrentTargetState = true; // true: open, false: close
    private Dictionary<string, bool> valveStates = new();

    int nodeID = -1;

    private void Awake()
    {
        foreach (var roots in ButtonRootsList)
        {
            ButtonRoots[roots.ScenarioType] = roots.Root;
        }
    }

    private void OnEnable()
    {
        nodeID = -1;
    }

    public void SetTargetValve(ScenarioAsset asset, int nodeId, ValveOperation operation)
    {
        ScenarioType = asset.Template.ScenarioType;

        var root = ButtonRoots[ScenarioType];
        Buttons.Clear();
        Buttons = root.GetComponentsInChildren<RemoteValveControlButton>(true).ToList();

        CurrentOperation = operation;
        CurrentTargetState = operation == ValveOperation.Restore;

        nodeID = nodeId;
        valveStates.Clear();

        TargetValves_SI = asset.Valves_SectionIsolation;
        TargetValves_SV = asset.Valves_SectionVent;

        foreach (var button in Buttons)
        {
            button.TargetState = CurrentTargetState;

            if (TargetValves_SI.Contains(button.name))
            {
                button.Phase = ValvePhase.SectionIsolation;
                button.gameObject.SetActive(true);

                valveStates[button.name] = button.IsTargetState;
            }
            else if (TargetValves_SV.Contains(button.name))
            {
                button.Phase = ValvePhase.SectionVent;
                button.gameObject.SetActive(true);

                valveStates[button.name] = button.IsTargetState;
            }
            else
            {
                button.Phase = ValvePhase.None;
                button.gameObject.SetActive(false);
            }
        }

        switch (CurrentOperation)
        {
            case ValveOperation.Isolate:
                if (TargetValves_SI.Count > 0)
                    currentPhase = ValvePhase.SectionIsolation;
                else if (TargetValves_SV.Count > 0)
                    currentPhase = ValvePhase.SectionVent;
                else
                    currentPhase = ValvePhase.Complete;
                break;

            case ValveOperation.Restore:
                if (TargetValves_SV.Count > 0)
                    currentPhase = ValvePhase.SectionVent;
                else if (TargetValves_SI.Count > 0)
                    currentPhase = ValvePhase.SectionIsolation;
                else
                    currentPhase = ValvePhase.Complete;
                break;
        }
    }

    List<string> VentValves = new();
    public void ConfirmVent(List<string> _VentValves)
    {
        if (Buttons == null || Buttons.Count == 0)
            return;

        VentValves = _VentValves;

        CurrentOperation = ValveOperation.Confirm;
        currentPhase = ValvePhase.ConfirmVent;

        bool hasFault = UnityEngine.Random.value < 0.3f;

        var targets = Buttons.Where(b => VentValves.Contains(b.name)).ToList();

        if (hasFault)
        {
            var randomButton = targets[UnityEngine.Random.Range(0, targets.Count)];
            randomButton.SetValveState(false);
        }

        foreach (var target in targets)
        {
            target.gameObject.SetActive(true);
            target.Phase = currentPhase;
            target.TargetState = true;
        }
    }

    public void OnValveStateChanged(string valveName, bool isTargetState)
    {
        if (CurrentOperation == ValveOperation.Confirm)
        {
            if (!VentValves.Contains(valveName)) return;

            valveStates[valveName] = isTargetState;
            bool confirmComplete = Buttons.Where(b => VentValves.Contains(b.name))
                                          .All(b => b.IsTargetState);

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

            bool siComplete = TargetValves_SI.Count == 0 ||
                          TargetValves_SI.All(v => valveStates[v]);

            bool svComplete = TargetValves_SV.Count == 0 ||
                              TargetValves_SV.All(v => valveStates[v]);

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
            btn.gameObject.SetActive(false);
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
