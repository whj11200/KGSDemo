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
    [SerializeField] protected AudioSource SESource;

    protected bool IsScenarioInitialized = false;
    protected bool IsScenarioRunning = false;

    public abstract void InitializeScenario(int assetIdx);
    public abstract void StartScenario();
    public abstract void CheckStep(int step);
    public abstract void ProcessScenario();
    public abstract void EndScenario();

    public abstract void PlayAudio(AudioClip clip);
    public abstract void StopAudio();
    public abstract void ShowMessage(ScenarioEvent e);
}

public class GameNode
{
    public ScenarioNode Node;
    public Action OnStart;
    public Action OnTextEnd;
    public Action OnEnd;
}
