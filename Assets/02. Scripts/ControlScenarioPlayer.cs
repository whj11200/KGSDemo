using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class ControlScenarioPlayer : ScenarioPlayerBase<ScenarioAsset>
{
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] List<CinemachineCamera> MonitorCameras = new();

    [SerializeField] int CameraIdx = 0;
    [SerializeField] string EnterControlRoomText = "가스 누출 조치 훈련을 시작합니다." +
                                                   "\r\n통제실에 입장 후 관제석에 착석하십시오.";
    [SerializeField] AudioClip EnterControlRoomVoice;
    [SerializeField] AudioClip AlarmClip;
    IContentSimulation simulation;

    private void Awake()
    {
        CameraIdx = MonitorCameras.Count / 2; // 중앙 카메라로 초기화
    }

    private void Start()
    {
        Invoke("EnterControlRoom", 1.5f);
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

        simulation = selectedAsset.Template.CreateSimulation(selectedAsset);
        simulation.Initialize();

        IsScenarioInitialized = true;
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
}

public interface IContentSimulation
{
    public event Action OnSimulationCompleted;

    public event Action<AudioClip> OnPlayAudio;
    public event Action<string> OnShowDialogue;
    public event Action<int> OnSwitchCamera;

    public void Initialize();
    public void StartSimulation();
    public void ProcessSimulationStep();
}

public abstract class SimulationBase : IContentSimulation
{
    protected ScenarioAsset Asset;
    protected List<GameNode> GameNodes;
    protected int CurrentNodeIndex = 0;
    protected GameNode CurrentGameNode;

    public event Action OnSimulationCompleted;

    public event Action<AudioClip> OnPlayAudio;
    public event Action<string> OnShowDialogue;
    public event Action<int> OnSwitchCamera;

    public SimulationBase(ScenarioAsset asset)
    {
        Asset = asset;
        GameNodes = new();
    }

    public abstract void Initialize();
    public abstract void StartSimulation();
    public abstract void ProcessSimulationStep();
    protected void EndSimulation()
    {
        OnSimulationCompleted?.Invoke();
    }
    protected void RaisePlayAudio(AudioClip clip)
    {
        OnPlayAudio?.Invoke(clip);
    }

    protected void RaiseShowDialogue(string text)
    {
        OnShowDialogue?.Invoke(text);
    }

    protected void RaiseSwitchCamera(int index)
    {
        OnSwitchCamera?.Invoke(index);
    }
}