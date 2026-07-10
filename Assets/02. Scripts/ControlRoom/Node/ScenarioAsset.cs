using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioAsset", menuName = "Scriptable Objects/ScenarioAsset")]
public class ScenarioAsset : ScriptableObject
{
    public string ScenarioName;
    public string BrokenPart;
    public ScenarioTemplate Template;
    public List<OverrideVoice> OverrideVoices;
    public List<ValveInfo> Valves_SectionIsolation = new();
    public List<ValveInfo> Valves_SectionVent = new();
    public bool OverrideVentValveList = false;
    public List<ValveInfo> VentValves = new();
    public Sprite BluePrint;
}

public enum ScenarioType
{
    FacilityScenario,
    PipelineScenario
}

[System.Serializable]
public class ScenarioNode
{
    public string Speaker;
    [TextArea(2, 3)]
    public string Content;
    public AudioClip Voice;
    public List<ScenarioEvent> Events = new();
    public bool NoCondition = false;
}

[System.Serializable]
public class ValveInfo
{
    public string Name;
    public ValvePhase Phase;
    [Tooltip("Checked : Open")]
    public bool InitialState = true;    // 대부분 열림
    public bool TargetState = false;     // 대부분 차단
}

[System.Serializable]
public class OverrideVoice
{
    public int Id;
    public AudioClip AudioClip;
}

public enum ScenarioEventType
{
    None,

    Alarm,
    Audio,
    Camera,
    Animation,
    Monitor,
    ValveConsole,
    UI,
    ShowMessage,
    Effect,
}

public class ScenarioEvent
{
    public ScenarioEventType EventType;

    public int NodeID;
    public string EventId;
    public float Delay;

    public UnityEngine.Object ObjectValue;
    public int IntValue;
    public float FloatValue;
    public string StringValue;
    public bool BoolValue;
    public Action Callback;
}