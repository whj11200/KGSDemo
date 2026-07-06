using NUnit.Framework;
using System;
using System.Collections.Generic;
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

    int phase = 0;
    int currentPhase = 1;
    int valveControlCount = 0;

    int nodeID = -1;

    public void SetTargetValve(ScenarioAsset asset, int nodeId)
    {
        phase = 1;
        currentPhase = 1;
        valveControlCount = 0;

        nodeID = nodeId;

        TargetValves_SI = asset.Valves_SectionIsolation;
        TargetValves_SV = asset.Valves_SectionVent;

        foreach (var button in Buttons)
        {
            var bName = button.name.Trim();

            if (TargetValves_SI.Contains(button.name))
            {
                button.phase = 0; 
                button.gameObject.SetActive(true);
            }
            else if (TargetValves_SV.Contains(button.name))
            {
                button.phase = 1; 
                button.gameObject.SetActive(true);
            }
            else
            { 
                button.phase = -1; 
                button.gameObject.SetActive(false);
            }
        }

        phase = TargetValves_SV.Count + TargetValves_SI.Count;
    }

    public bool OnClickValve(string name, int _phase)
    {
        Debug.Log($"nodeID : {nodeID}");
        Debug.Log($"currentPhase : {currentPhase}");

        if (nodeID < 0)
            return false;

        if (_phase == currentPhase)
        {
            valveControlCount++;
        }

        if (currentPhase == 1 && valveControlCount == TargetValves_SV.Count)
        {
            valveControlCount = 0;
            currentPhase++;

            OnPhaseComplete?.Invoke(currentPhase);
        }
        else if (currentPhase == 1 && valveControlCount == TargetValves_SI.Count)
        {
            valveControlCount = 0;
            currentPhase++;

            OnPhaseComplete?.Invoke(currentPhase);
        }

        if (currentPhase >= phase)
        {
            OnControlComplete?.Invoke();
        }

        return _phase == currentPhase;
    }

    public void AllButtonsDisable()
    {
        foreach(var btn in Buttons)
        {
            btn.gameObject.SetActive(false);
        }
    }
}
