using System.Collections.Generic;
using UnityEngine;

public abstract class ScenarioTemplate : ScriptableObject
{
    public ScenarioType ScenarioType;
    public List<ScenarioNode> Nodes;
    public int WaitTime;

    public abstract IContentSimulation CreateSimulation(ScenarioAsset asset);
}
