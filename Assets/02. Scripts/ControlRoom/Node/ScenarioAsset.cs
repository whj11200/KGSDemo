using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioAsset", menuName = "Scriptable Objects/ScenarioAsset")]
public class ScenarioAsset : ScriptableObject
{
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
}
