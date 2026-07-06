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

    [SerializeField] List<RemoteValveControlButton> Buttons = new();

    [SerializeField] public Color OpenColor;
    [SerializeField] public Color CloseColor;

    public event Action<int> OnPhaseComplete;
    public event Action OnControlComplete;

    private ValvePhase currentPhase = ValvePhase.None;
    public ValvePhase CurrentPhase => currentPhase;

    private bool CurrentTargetState = true; // true: open, false: close
    private Dictionary<string, bool> valveStates = new();

    int nodeID = -1;

    private void OnEnable()
    {
        nodeID = -1;
    }

    public void SetTargetValve(ScenarioAsset asset, int nodeId)
    {
        CurrentTargetState = nodeID == -1 ? false : true;

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
            }
            else if (TargetValves_SV.Contains(button.name))
            {
                button.Phase = ValvePhase.SectionVent;
                button.gameObject.SetActive(true);
            }
            else
            {
                button.Phase = ValvePhase.None;
                button.gameObject.SetActive(false);
            }
        }

        foreach (var valve in TargetValves_SI)
            valveStates[valve] = false;

        foreach (var valve in TargetValves_SV)
            valveStates[valve] = false;

        if (TargetValves_SI.Count > 0)
            currentPhase = ValvePhase.SectionIsolation;
        else if (TargetValves_SV.Count > 0)
            currentPhase = ValvePhase.SectionVent;
        else
            currentPhase = ValvePhase.Complete;
    }

    public void OnValveStateChanged(string valveName, bool isTargetState)
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

        // SI가 아직 완료되지 않았으면 항상 SI 단계
        if (!siComplete)
        {
            currentPhase = ValvePhase.SectionIsolation;
            return;
        }

        // SI는 완료됐지만 SV가 남아있으면 SV 단계
        if (!svComplete)
        {
            if (currentPhase != ValvePhase.SectionVent)
            {
                currentPhase = ValvePhase.SectionVent;
                OnPhaseComplete?.Invoke(2);
            }

            return;
        }

        // 모두 완료
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
}
