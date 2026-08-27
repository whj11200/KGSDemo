using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimulationBase : IContentSimulation
{
    protected ScenarioAsset Asset;
    protected List<GameNode> GameNodes;
    public ScenarioEventBus<ScenarioEventType> EventBus;

    protected int CurrentNodeIndex = -1;
    protected bool IsProcessBlocked = false;
    protected bool IsRunning = false;

    protected GameNode CurrentGameNode;

    ScenarioEventBus<ScenarioEventType> IContentSimulation.EventBus => EventBus;

    public int SimulNodeID { get => CurrentNodeIndex; }

    public event Action OnSimulationCompleted;

    public SimulationBase(ScenarioAsset asset)
    {
        Asset = asset;
        GameNodes = new();
        EventBus = new ScenarioEventBus<ScenarioEventType>();
    }

    public abstract void Initialize();
    public virtual void StartSimulation()
    {
        ProcessSimulationStep();
    }

    public virtual void ProcessSimulationStep()
    {
        if (IsRunning) return;

        IsRunning = true;

        if (IsProcessBlocked)
        {
            IsRunning = false;
            return;
        }

        if (CurrentNodeIndex >= 0)
            GameNodes[CurrentNodeIndex].OnEnd?.Invoke();

        CurrentNodeIndex++;

        if (CurrentNodeIndex >= GameNodes.Count)
        {
            EndSimulation();
            return;
        }

        GameNodes[CurrentNodeIndex]?.OnStart?.Invoke();
        IsRunning = false;
    }

    public void CompleteStep()
    {
        if (!IsProcessBlocked)
            return;

        IsProcessBlocked = false;
        ProcessSimulationStep();
    }

    protected void EndSimulation()
    {
        OnSimulationCompleted?.Invoke();
    }
}
