using System;
using UnityEngine;

public interface IContentSimulation
{
    public int SimulNodeID { get; }
    ScenarioEventBus<ScenarioEventType> EventBus { get; }

    public event Action OnSimulationCompleted;

    public void Initialize();
    public void StartSimulation();
    public void CompleteStep();
    public void ProcessSimulationStep();
}
