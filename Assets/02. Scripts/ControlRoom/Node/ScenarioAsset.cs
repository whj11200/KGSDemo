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
    public List<string> ControlValves = new();
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
    [TextArea(2, 3)]
    public string Content;
    public AudioClip Voice;
    public List<ScenarioEvent> Events = new();
    public bool NoCondition = false;
}

public enum ScenarioEventType
{
    None,

    Alarm,
    Audio,
    Camera,
    Animation,
    Monitor,
    UI,
    ShowMessage,
    Effect,
}

public class ScenarioEvent
{
    public ScenarioEventType EventType;

    public string EventId;
    public float Delay;

    public UnityEngine.Object ObjectValue;
    public int intValue;
    public string StringValue;
    public Action Callback;
}