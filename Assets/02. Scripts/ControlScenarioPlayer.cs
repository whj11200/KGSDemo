using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ControlScenarioPlayer : ScenarioPlayerBase<ScenarioAsset>
{
    [SerializeField] ScenarioSelector Selector;
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] GameObject MonitorSwitcher;
    [SerializeField] FadeUI FadeUI;
    [SerializeField] List<CinemachineCamera> MonitorCameras = new();
    [SerializeField] List<ControlRoomMonitor> Monitors = new();

    [SerializeField] ValveControlConsole ValveConsole;

    [SerializeField] int CameraIdx = 0;
    [SerializeField] string EnterControlRoomText = "가스 누출 조치 훈련을 시작합니다." +
                                                   "\r\n통제실에 입장 후 관제석에 착석하십시오.";
    [SerializeField] AudioClip EnterControlRoomVoice;
    [SerializeField] AudioClip AlarmClip;
    IContentSimulation simulation;
    ScenarioEventBus<ScenarioEventType> ScenarioEventBus = new();
    public int CurrentNodeId => simulation.SimulNodeID;

    private void Awake()
    {
        CameraIdx = MonitorCameras.Count / 2; // 중앙 카메라로 초기화
    }

    private void Start()
    {
        StartCoroutine(Delay(() =>
        {
            EnterControlRoom();
            PlayAudio(EnterControlRoomVoice);
        }, 1.5f));
    }

    public void EnterControlRoom()
    {
        DialogueUI.SetSpeaker();
        DialogueUI.SetBodyText(EnterControlRoomText);
        DialogueUI.Show(true);

        StartCoroutine(Delay(()=>
        {
            DialogueUI.Show(false);
        }, 5f));
    }

    IEnumerator Delay(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private int reportID;
    public override void InitializeScenario(int assetIdx)
    {
        var selectedAsset = ScenarioAssets[assetIdx];
        var type = selectedAsset.Template.ScenarioType.ToString();
        var part = selectedAsset.BrokenPart;

        simulation = selectedAsset.Template.CreateSimulation(selectedAsset);

        var mainMonitor = Monitors[1];

        mainMonitor.SetPopupText($"<color=#FF0000>{selectedAsset.ScenarioName}</color> 에서 \r\n" +
                                 $"LNG 가스 누출이 확인되었습니다.");

        mainMonitor.OnProcessBtn += ProcessScenario;

        ValveConsole.AllButtonsDisable();

        ScenarioEventBus = simulation.EventBus;

        simulation.OnSimulationCompleted += EndScenario;
        ValveConsole.OnControlComplete += ProcessScenario;

        SubscribeEvent(ScenarioEventType.Audio, e =>
        {
            if (e.ObjectValue is AudioClip clip)
                PlayAudio(clip);
        });

        SubscribeEvent(ScenarioEventType.Alarm, e =>
        {
            SESource.PlayOneShot(AlarmClip);
        });

        SubscribeEvent(ScenarioEventType.ShowMessage, e =>
        {
            if (!string.IsNullOrEmpty(e.StringValue))
                ShowMessage(e.StringValue, e.Delay);
        });

        SubscribeEvent(ScenarioEventType.Monitor, e =>
        {
            switch (e.EventId)
            {
                case "Warning_Flash":
                    mainMonitor.BlinkIcon(e.NodeID);
                    break;

                case "WP_Flash":
                    mainMonitor.ShowWaringPoint(e.NodeID);
                    break;
            }
        });

        SubscribeEvent(ScenarioEventType.ValveConsole, e =>
        {
            ValveConsole.SetTargetValve(selectedAsset, e.NodeID);
            mainMonitor.ShowValves();
        });

        SubscribeEvent(ScenarioEventType.Camera, e =>
        {
            reportID = e.NodeID;
            MonitorSwitcher.SetActive(false);
            Selector.ExitSelectionMode();
        });

        SubscribeEvent(ScenarioEventType.UI, e =>
        {
            e.Delay = 9f;

            FadeUI.FadeOutIn(3f, 3f, 3f,
                OnOutEnd: () =>  FadeUI.SetText(e.StringValue),
                OnInStart: () => FadeUI.HideText());
        });

        simulation.Initialize();
        mainMonitor.SetScreenInfo(selectedAsset.BluePrint, assetIdx, part);

        IsScenarioInitialized = true;

        StartScenario();
    }

    public override void StartScenario()
    {
        simulation.StartSimulation();
    }

    public override void CheckStep(int step)
    {
        if (step == CurrentNodeId) 
            ProcessScenario();
    }

    public override void ProcessScenario()
    {
        simulation.CompleteStep();
    }

    public override void EndScenario()
    {
        
    }

    public void ReportToManger()
    {
        CheckStep(reportID);
    }

    public void SwitchCameraLeft()
    {
        if (CameraIdx < MonitorCameras.Count - 1)
        {
            CameraIdx++;
            CameraSwitcher.SetCamera(MonitorCameras[CameraIdx]);
        }
    }

    public void SwitchCameraRight()
    {
        if (CameraIdx > 0)
        {
            CameraIdx--;
            CameraSwitcher.SetCamera(MonitorCameras[CameraIdx]);
        }
    }

    public override void PlayAudio(AudioClip clip)
    {
        if (clip == null) return;

        if (VoiceSource.isPlaying)
        {
            VoiceSource.Stop();
        }

        VoiceSource.PlayOneShot(clip);
    }

    public override void StopAudio()
    {
        if (VoiceSource.isPlaying)
        {
            VoiceSource.Stop();
        }
    }

    public override void ShowMessage(string message, float duration)
    {
        int index = message.IndexOf(':');

        string speaker = "";
        Debug.Log(speaker + "-" + message);

        string body = message;

        if (index >= 0)
        {
            speaker = message[..index];
            body = message[(index + 1)..];
        }

        DialogueUI.Show(true);

        DialogueUI.SetSpeaker(speaker);
        DialogueUI.SetBodyText(body);

        StartCoroutine(Delay(() =>
        {
            DialogueUI.Show(false);
        }, duration));
    }

    void SubscribeEvent(ScenarioEventType type, Action<ScenarioEvent> handler)
    {
        ScenarioEventBus.Subscribe(type, e =>
        {
            handler(e);

            if (e.Callback != null)
                StartCoroutine(Delay(e.Callback, e.Delay));
        });
    }
}

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
    public abstract void StartSimulation();
    public abstract void ProcessSimulationStep();
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