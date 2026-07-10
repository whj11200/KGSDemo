using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static ValveControlConsole;

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

    [SerializeField] InputActionReference SkipAction;

    [SerializeField] NPC_AnimationController Manager;

    private void Awake()
    {
        CameraIdx = MonitorCameras.Count / 2; // 중앙 카메라로 초기화
    }

    private void OnEnable()
    {
        SkipAction.action.Enable();
        SkipAction.action.performed += NextAction;
    }

    private void OnDisable()
    {
        SkipAction.action.Disable();
        SkipAction.action.performed -= NextAction;
    }

    private void NextAction(InputAction.CallbackContext context)
    {
        StopAudio();
        Manager.StopAudio();
        DialogueUI.Complete();
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

        var e = new ScenarioEvent
        {
            EventType = ScenarioEventType.ShowMessage,
            StringValue = EnterControlRoomText,
            ObjectValue = EnterControlRoomVoice,
            FloatValue = EnterControlRoomVoice != null ? EnterControlRoomVoice.length : 3.5f,
            Callback = () => DialogueUI.Show(false),
        };

        DialogueUI.SetBodyTextWithTyping(EnterControlRoomText, e, e.Callback);
        DialogueUI.Show(true);
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
        var type = selectedAsset.Template.ScenarioType;
        var part = selectedAsset.BrokenPart;

        var valveList = new HashSet<ValveInfo>();
        var vlaveDict = new Dictionary<string, ValveInfo>();

        foreach (var asset in ScenarioAssets)
        {
            if (asset.Template.ScenarioType != type) continue;

            valveList.UnionWith(asset.Valves_SectionIsolation);
            valveList.UnionWith(asset.Valves_SectionVent);
            valveList.UnionWith(asset.Template.VentValves);
        }

        foreach (var v in valveList)
        {
            vlaveDict[v.Name] = v;
        }

        simulation = selectedAsset.Template.CreateSimulation(selectedAsset);

        var mainMonitor = Monitors[1];

        mainMonitor.OnProcessBtn += ProcessScenario;

        ValveConsole.InitValveConsole(selectedAsset);
        ValveConsole.AllButtonsDisable(vlaveDict);

        ScenarioEventBus = simulation.EventBus;

        simulation.OnSimulationCompleted += EndScenario;
        ValveConsole.OnControlComplete += ProcessScenario;

        SubscribeEvent(ScenarioEventType.Audio, e =>
        {
            if (e.ObjectValue is AudioClip clip)
            {
                if (string.IsNullOrEmpty(e.StringValue))
                    PlayAudio(clip);
                else 
                    Manager.PlayAudio(clip);
            }
        });

        SubscribeEvent(ScenarioEventType.Alarm, e =>
        {
            if (e.EventId == "On")
                SESource.PlayOneShot(AlarmClip);

            else if (e.EventId == "Off")
                SESource.Stop();
        });

        SubscribeEvent(ScenarioEventType.ShowMessage, e =>
        {
            if (!string.IsNullOrEmpty(e.StringValue))
                ShowMessage(e);
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
            switch(e.EventId)
            {
                case "Valve_Close":
                    ValveConsole.SetTargetValve(ValveOperation.Isolate);
                    mainMonitor.ShowValves();
                    break;

                case "Valve_Revert":
                    if (e.StringValue == "IsolateOnly") 
                        ValveConsole.SetTargetValve(ValveOperation.Restore_IsolateOnly);
                    else 
                        ValveConsole.SetTargetValve(ValveOperation.Restore);

                    mainMonitor.ShowValves();
                    break;

                case "Valve_ConfirmVent":
                    ValveConsole.ConfirmVent();
                    break;
            }
        });

        SubscribeEvent(ScenarioEventType.Camera, e =>
        {
            reportID = e.NodeID;
            MonitorSwitcher.SetActive(false);
            Selector.ExitSelectionMode();
        });

        SubscribeEvent(ScenarioEventType.Animation, e =>
        {
            switch (e.StringValue)
            {
                case "SetBool":
                    Manager.SetLookAtPlayer(e.BoolValue);
                    Manager.SetBool(e.EventId, e.BoolValue);
                    break;

                case "SetTrigger":
                    Manager.SetLookAtPlayer(e.BoolValue);
                    Manager.SetTrigger(e.EventId);
                    break;
            }
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
        else
        {
            Debug.Log($"{step} == {CurrentNodeId}");
        }
    }

    public override void ProcessScenario()
    {
        simulation.CompleteStep();
    }

    public override void EndScenario()
    {
        MonitorSwitcher.SetActive(false);
        CameraSwitcher.Revert();

        SceneManager.LoadScene("KGSScene");
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

    public override void ShowMessage(ScenarioEvent e)
    {
        var message = e.StringValue;

        int index = message.IndexOf(':');

        string speaker = "";
        string body = message;

        if (index >= 0)
        {
            speaker = message[..index];
            body = message[(index + 1)..];
        }

        DialogueUI.Show(true);

        DialogueUI.SetSpeaker(speaker);
        DialogueUI.SetBodyTextWithTyping(body, e, () =>
        {
            DialogueUI.Show(false);
            e.Callback?.Invoke();
        });
    }

    void SubscribeEvent(ScenarioEventType type, Action<ScenarioEvent> handler)
    {
        ScenarioEventBus.Subscribe(type, e =>
        {
            handler(e);

            if (type != ScenarioEventType.ShowMessage && e.Callback != null)
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