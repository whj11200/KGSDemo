using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScenarioPlayerBase<T> : MonoBehaviour
{
    [SerializeField] protected List<T> ScenarioAssets;
    [SerializeField] protected GameObject Player;
    [SerializeField] protected MessageUI MessageUI;
    [SerializeField] protected SimpleDialogueViewUGUI DialogueUI;
    [SerializeField] protected AudioSource VoiceSource;

    protected int CurrentNodeIndex = 0;
    protected GameNode CurrentGameNode;
    protected bool IsScenarioRunning = false;
    protected bool IsProcessBlocked = false;

    public abstract void InitializeScenario(int assetIdx);
    public abstract void StartScenario();
    public abstract void ProcessScenario();
    public abstract void EndScenario();
}

public class GameNode
{
    public ScenarioNode Node;
    public Action OnStart;
    public Action OnProcess;
    public Action OnEnd;
}
