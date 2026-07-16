using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ValveControlConsole : MonoBehaviour
{
    [SerializeField] List<ValveInfo> TargetValves = new();
    [SerializeField] List<ValveInfo> VentValves = new();

    [SerializeField] private ScenarioType ScenarioType;
    [SerializeField] List<RemoteValveControlButton> Buttons = new();
    [SerializeField] List<ButtonRoot> ButtonRootsList = new();
    Dictionary<ScenarioType, GameObject> ButtonRoots = new();
    Dictionary<string, ValveInfo> valveInfos_Control = new();
    Dictionary<string, ValveInfo> valveInfos_Vent = new();

    [SerializeField] public Color OpenColor;
    [SerializeField] public Color CloseColor;

    public event Action<int> OnPhaseComplete;
    public event Action OnControlComplete;

    [SerializeField] private ValvePhase currentPhase = ValvePhase.None;
    public ValvePhase CurrentPhase => currentPhase;

    public ValveOperation CurrentOperation { get; private set; }
    private Dictionary<string, bool> valveStates = new();

    [SerializeField] private List<OperationPhase> OperationPhasesData = new();
    private Dictionary<ValveOperation, PhaseData[]> OperationPhases = new();

    private void Awake()
    {
        foreach (var roots in ButtonRootsList)
        {
            ButtonRoots[roots.ScenarioType] = roots.Root;
        }

        OperationPhases.Clear();
        foreach (var op in OperationPhasesData)
        {
            OperationPhases[op.operation] = op.phaseDatas;
        }
    }

    private void OnEnable()
    {
        
    }

    // 초기화 : 선택된 시나리오에서 사용되는 밸브 리스트화 및 각 버튼에 정보 주입
    public void InitValveConsole(ScenarioAsset asset)
    {
        ScenarioType = asset.Template.ScenarioType;

        var root = ButtonRoots[ScenarioType];
        root.gameObject.SetActive(true);

        Buttons.Clear();
        Buttons = root.GetComponentsInChildren<RemoteValveControlButton>(true).ToList();

        TargetValves = asset.Valves_SectionIsolation
                        .Concat(asset.Valves_SectionVent)
                        .ToList();

        VentValves = asset.OverrideVentValveList ? asset.Valves_SectionVent : asset.Template.VentValves;

        valveInfos_Control.Clear(); 
        valveInfos_Vent.Clear();

        foreach (var valve in TargetValves)
            valveInfos_Control[valve.Name] = valve;

        foreach (var valve in VentValves)
            valveInfos_Vent[valve.Name] = valve;
    }

    // 시나리오 노드별 조작이 필요한 밸브 세팅
    // 목표 상태 설정, 해당 단계 완료 후 다음 단계는 어느 단계인지
    public void SetTargetValve(ValveOperation operation) // true: open 목표, false: close 목표
    {
        CurrentOperation = operation;
        currentPhase = ValvePhase.Complete;

        if (Buttons == null || Buttons.Count == 0)
            return;

        if (valvePhases == null || valvePhases[0].phase == ValvePhase.None) return;

        if (valvePhases[0].phase == ValvePhase.SectionIsolation)
            SetPhase(0, valveInfos_Control);
        else
            SetPhase(0, valveInfos_Vent);

    }

    PhaseData[] valvePhases => OperationPhases[CurrentOperation];

    // 첫 단계 설정
    private void SetPhase(int idx, Dictionary<string, ValveInfo> TargetInfo, bool Switch = false)
    {
        if (idx >= valvePhases.Length)
        {
            return;
        }

        currentPhase = valvePhases[idx].phase;

        Debug.Log($"currentPhase : {CurrentPhase}");

        valveStates.Clear();

        foreach (var button in Buttons)
        {
            // 조작 대상 밸브 아님
            if (!TargetInfo.TryGetValue(button.name, out var info))
            {
                button.Phase = ValvePhase.None;
                button.GetComponent<Image>().raycastTarget = false;
                continue;
            }

            button.GetComponent<Image>().raycastTarget = true;
            button.Phase = currentPhase;
            button.TargetState = valvePhases[idx].IsOpen;
            valveStates[button.name] = button.IsTargetState;
        }
    }

    // 각 버튼 클릭 시 호출되는 함수
    // 해당 버튼이 타겟 상태를 만족하는지 확인
    // 현재 단계에 해당하는 버튼들이 모두 타겟 상태를 만족할 경우 다음 단계 진행 혹은 클리어
    public void OnValveStateChanged(string valveName, bool isTargetState)
    {
        if (!valveStates.ContainsKey(valveName))
            return;

        valveStates[valveName] = isTargetState;

        bool phaseComplete =
            Buttons
                .Where(b => b.Phase == currentPhase)
                .All(b => valveStates[b.name]);

        if (phaseComplete)
        {
            CompleteCurrentPhase();
        }
    }

    private void CompleteCurrentPhase()
    {
        int index = Array.FindIndex(
            valvePhases,
            p => p.phase == currentPhase);

        if (index < valvePhases.Length - 1)
        {
            var next = index + 1;

            if (valvePhases[next].phase == ValvePhase.SectionIsolation)
                SetPhase(next, valveInfos_Control);
            else
                SetPhase(next, valveInfos_Vent);

            OnPhaseComplete?.Invoke(next);
        }
        else
        {
            Debug.Log($"CompleteControl : {currentPhase}");

            CompleteControl();
        }
    }

    private void CompleteControl()
    {
        if (currentPhase != ValvePhase.Complete)
        {
            currentPhase = ValvePhase.Complete;
            OnControlComplete?.Invoke();
        }
    }

    public void AllButtonsDisable(Dictionary<string, ValveInfo> valveDict)
    {
        foreach(var btn in Buttons)
        {
            btn.GetComponent<Image>().raycastTarget = false;

            if (valveDict.TryGetValue(btn.name, out ValveInfo valveInfo))
            {
                btn.InitValve(valveInfo, ValveOperation.None);
            }
        }
    }

    public enum ValveOperation
    {
        None = 0,
        Isolate = 1,                // 차단
        Vent = 2,                   // 방산
        Restore = 3,                // 복구
        Restore_IsolateOnly = 4,    // Isolate에 있는 밸브들만 복구
        Confirm = 5
    }

    [System.Serializable]
    public struct ButtonRoot
    {
        public ScenarioType ScenarioType;
        public GameObject Root;
    }

    [System.Serializable]
    public struct PhaseData
    {
        public ValvePhase phase;
        public bool IsOpen;
    }

    [System.Serializable]
    public struct OperationPhase
    {
        public ValveOperation operation;
        public PhaseData[] phaseDatas;
    }
}
