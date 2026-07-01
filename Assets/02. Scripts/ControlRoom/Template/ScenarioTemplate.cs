using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioTemplate", menuName = "Scriptable Objects/ScenarioTemplate")]
public class ScenarioTemplate : ScriptableObject
{
    public ScenarioType ScenarioType;
    public List<ScenarioNode> Nodes;
    public int WaitTime;
}
