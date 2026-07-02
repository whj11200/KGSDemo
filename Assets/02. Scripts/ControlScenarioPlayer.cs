using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class ControlScenarioPlayer : ScenarioPlayerBase<ScenarioAsset>
{
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] List<CinemachineCamera> MonitorCameras = new();
    [SerializeField] List<ControlRoomMonitor> Monitors = new();

    [SerializeField] int CameraIdx = 0;
    [SerializeField] string EnterControlRoomText = "가스 누출 조치 훈련을 시작합니다." +
                                                   "\r\n통제실에 입장 후 관제석에 착석하십시오.";
    [SerializeField] AudioClip EnterControlRoomVoice;
    [SerializeField] AudioClip AlarmClip;
    IContentSimulation simulation;
    ScenarioEventBus<ScenarioEventType> ScenarioEventBus = new();

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

    public override void InitializeScenario(int assetIdx)
    {
        var selectedAsset = ScenarioAssets[assetIdx];
        var type = selectedAsset.Template.ScenarioType.ToString();

        simulation = selectedAsset.Template.CreateSimulation(selectedAsset);
        Monitors[1].SetPopupText($"<color=#FF0000>{selectedAsset.ScenarioName}</color> 에서 \r\n" +
                                 $"LNG 가스 누출이 확인되었습니다.");
        ScenarioEventBus = simulation.EventBus;

        simulation.OnSimulationCompleted += EndScenario;

        ScenarioEventBus.Subscribe(ScenarioEventType.Audio, (seArgs) =>
        {
            if (seArgs.ObjectValue is AudioClip clip)
            {
                PlayAudio(clip);

            }
        });

        ScenarioEventBus.Subscribe(ScenarioEventType.Alarm, (seArgs) =>
        {
            SESource.PlayOneShot(AlarmClip);
        });

        ScenarioEventBus.Subscribe(ScenarioEventType.ShowMessage, (seArgs) =>
        {
            var message = seArgs.StringValue;

            if (!string.IsNullOrEmpty(message)) 
            {
                ShowMessage(message, 3f);
            }
        });

        ScenarioEventBus.Subscribe(ScenarioEventType.Monitor, (seArgs) =>
        {
            Monitors[1].BlinkIcon();
        });

        simulation.Initialize();
        Monitors[CameraIdx].SetScreenImage(selectedAsset.BluePrint);

        IsScenarioInitialized = true;

        StartScenario();
    }

    public override void StartScenario()
    {
        simulation.StartSimulation();
    }

    public override void ProcessScenario()
    {
        simulation.ProcessSimulationStep();
    }

    public override void EndScenario()
    {
        
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
        DialogueUI.Show(true);
        DialogueUI.SetBodyText(message);
        StartCoroutine(Delay(()=>
        {
            DialogueUI.Show(false);
        }, duration));
    }
}

public interface IContentSimulation
{
    ScenarioEventBus<ScenarioEventType> EventBus { get; }

    public event Action OnSimulationCompleted;

    public void Initialize();
    public void StartSimulation();
    public void ProcessSimulationStep();
}

public abstract class SimulationBase : IContentSimulation
{
    protected ScenarioAsset Asset;
    protected List<GameNode> GameNodes;
    public ScenarioEventBus<ScenarioEventType> EventBus;

    protected int CurrentNodeIndex = -1;
    protected GameNode CurrentGameNode;

    ScenarioEventBus<ScenarioEventType> IContentSimulation.EventBus => EventBus;

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
    protected void EndSimulation()
    {
        OnSimulationCompleted?.Invoke();
    }
}