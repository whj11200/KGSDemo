using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ValveQuestManager : MonoBehaviour
{
    [Serializable]
    public class ValveBlinkTarget
    {
        [Header("벨브 핸들")]
        public VavleHandle valveHandle;
        [Header("벨브 핸들 렌더러")]
        public Renderer valveRenderer;
        [Header("벨브 핸들 메테리얼")]
        [HideInInspector] public Material runtimeMaterial;
        [Header("벨브 Emission 깜빡임 코루틴")]
        [HideInInspector] public Coroutine blinkCoroutine;
       

        [HideInInspector] public Color originEmissionColor;
        [HideInInspector] public bool originEmissionEnabled;
        [HideInInspector] public bool hasSavedOrigin;
    }

    [Header("우측 상단 UI 매니저")]
    [SerializeField] private ManagerCenterUiManager managerCenterUiManager;
    [Header("아지랑이파티클 스크립트")]
    [SerializeField] private HazeControl hazeControl;
    [Header("디얄로그 모듈")]
    [SerializeField] DialogueModeul dialogueModeul;
    [Header("벨브 이름 캔버스")]
    [SerializeField] private GameObject valveNameCanvas;

    [Header("관리할 밸브 4개")]
    [SerializeField] private List<ValveBlinkTarget> valveTargets = new();
    [Header("벨브 네비 오브젝트들")]
    public List<GameObject> valveNavObjects = new();
    [Header("누출지점 위치표시 오브젝트")]
    [SerializeField] GameObject leakPosObject;
    [Header("탈출지점 위치표시 오브젝트")]
    [SerializeField] GameObject escapePosObject;
    [Header("Emission Blink Setting")]
    [SerializeField] private Color emissionColor = Color.green;
    [SerializeField] private float maxEmissionPower = 2f;
    [SerializeField] private float blinkDuration = 1f;

    [Header("Valve Close Count")]
    [SerializeField] private int targetValveCloseCount = 4;
    [SerializeField] private int currentClosedValveCount = 0;
    [Header("Valve Open Count")]
    [SerializeField] private int targetValveOpenCount = 4;
    [SerializeField] private int currentOpenedValveCount = 0;

    [Header("Open Complete Event")]
    [SerializeField] private UnityEvent onAllValveOpened;

    private readonly HashSet<VavleHandle> openedValves = new();

    private bool isValveOpenStageActive;
    private bool isAllValveOpened;
    [Header("Complete Event")]
    [SerializeField] private UnityEvent onAllValveClosed;


    private readonly HashSet<VavleHandle> closedValves = new();

    private bool isValveCloseStageActive;
    private bool isAllValveClosed;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        InitValveMaterials();
    }

    private void Start()
    {
        Init();


        StopAllValveEmissionAndReset();
    }
    public void Init()
    {
        escapePosObject.SetActive(false);
        leakPosObject.SetActive(false);
        if (valveNameCanvas != null)
            valveNameCanvas.SetActive(false);
    }
    private void OnEnable()
    {
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.VavleCloseStage.ToString(),
            CloseVavleStage);
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.StartExitStage.ToString(),
            EscapePosActive);
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.StartHazeStage.ToString(),
            StartHaze);
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.StartLeakStage.ToString(),
            LeakPosActive);
        DialogueEventBus.Subscribe(
            ManagerCenterWorkerLearningCenterEventType.VavleOpenStage.ToString(),
            OpenValveStage);
    }

    private void OnDisable()
    {
        DialogueEventBus.Unsubscribe(
            ManagerCenterWorkerLearningCenterEventType.VavleCloseStage.ToString(),
            CloseVavleStage);

        DialogueEventBus.Unsubscribe(
            ManagerCenterWorkerLearningCenterEventType.StartHazeStage.ToString(),
            StartHaze);
        DialogueEventBus.Unsubscribe(
            ManagerCenterWorkerLearningCenterEventType.StartLeakStage.ToString(),
            LeakPosActive);

        StopAllValveEmissionAndReset();
    }

    private void InitValveMaterials()
    {
        foreach (ValveBlinkTarget target in valveTargets)
        {
            if (target == null || target.valveRenderer == null)
                continue;

            // 중요: sharedMaterial이 아니라 material 사용
            // 그래야 이 밸브 하나만 따로 Emission 제어 가능
            target.runtimeMaterial = target.valveRenderer.material;

            SaveOriginEmission(target);
        }
    }

    public void CloseVavleStage()
    {
        isValveCloseStageActive = true;
        isAllValveClosed = false;

        closedValves.Clear();
        currentClosedValveCount = 0;

        if (managerCenterUiManager != null)
            managerCenterUiManager.ShowGuide("- CLOSE : M-31A,31B,21B,21A -");

        if (valveNameCanvas != null)
            valveNameCanvas.SetActive(true);

        StartAllValveEmissionBlink();

        Debug.Log("[Valve Stage] 밸브 차단 단계 시작");
        foreach(var valvenav in valveNavObjects)
        {
            if(valvenav != null)
                valvenav.SetActive(true);
        }
    }

    public void RegisterValveClosed(VavleHandle valveHandle)
    {
        if (!isValveCloseStageActive)
        {
            Debug.LogWarning("[Valve Stage] 밸브 차단 단계가 아닙니다.");
            return;
        }

        if (valveHandle == null)
            return;

        if (isAllValveClosed)
            return;

        if (!closedValves.Add(valveHandle))
        {
            Debug.Log($"[Valve Stage] 이미 잠긴 밸브입니다. {currentClosedValveCount}/{targetValveCloseCount}");
            return;
        }

        currentClosedValveCount = closedValves.Count;

        // 핵심: 방금 돌린 밸브 하나만 Emission 끄기
        StopValveEmissionAndReset(valveHandle);

        Debug.Log($"[Valve Stage] 밸브 잠금 완료: {currentClosedValveCount}/{targetValveCloseCount}");
        managerCenterUiManager.ShowTemporaryGuide($"{valveHandle.name} 차단완료 \n {currentClosedValveCount}/{targetValveCloseCount}");
        if (currentClosedValveCount >= targetValveCloseCount)
        {
            CompleteValveCloseStage();
        }
    }

    public void RegisterValveOpened(VavleHandle valveHandle)
    {
        if (valveHandle == null)
            return;

        // Open 퀘스트 중이면 Open 퀘스트 카운트로 처리
        if (isValveOpenStageActive)
        {
            RegisterValveOpenedForQuest(valveHandle);
            return;
        }

        // Close 퀘스트 중에 다시 열었을 때만 잠금 카운트에서 제거
        if (isValveCloseStageActive)
        {
            if (!closedValves.Remove(valveHandle))
                return;

            currentClosedValveCount = closedValves.Count;
            isAllValveClosed = false;

            Debug.Log($"[Valve Stage] 밸브 잠금 해제: {currentClosedValveCount}/{targetValveCloseCount}");

            StartValveEmissionBlink(valveHandle);
            return;
        }

        Debug.LogWarning("[Valve Stage] 현재 밸브 열림을 처리할 활성 단계가 없습니다.");
    }

    private void CompleteValveCloseStage()
    {
        isAllValveClosed = true;
        isValveCloseStageActive = false;

        // 여기서는 혹시 남아있는 코루틴 방지용으로 전체 정리
        StopAllValveEmissionAndReset();

        Debug.Log("[Valve Stage] 총 4개 밸브 차단 완료");
        managerCenterUiManager.HideGuide();
        dialogueModeul.StartDialogueFrom("M2");
        onAllValveClosed?.Invoke();
    }

    public void StartAllValveEmissionBlink()
    {
        foreach (ValveBlinkTarget target in valveTargets)
        {
            if (target == null || target.valveHandle == null)
                continue;

            // 닫기 단계에서는 이미 닫은 밸브는 깜빡이지 않음
            if (isValveCloseStageActive && closedValves.Contains(target.valveHandle))
                continue;

            // 열기 단계에서는 이미 연 밸브는 깜빡이지 않음
            if (isValveOpenStageActive && openedValves.Contains(target.valveHandle))
                continue;

            StartValveEmissionBlink(target.valveHandle);
        }
    }

    public void StartValveEmissionBlink(VavleHandle valveHandle)
    {
        ValveBlinkTarget target = FindTarget(valveHandle);

        if (target == null)
        {
            Debug.LogWarning($"[Valve Stage] {valveHandle.name}에 해당하는 ValveBlinkTarget을 찾지 못했습니다.");
            return;
        }

        if (target.runtimeMaterial == null)
            return;

        if (target.blinkCoroutine != null)
            StopCoroutine(target.blinkCoroutine);

        target.blinkCoroutine = StartCoroutine(EmissionBlinkCoroutine(target));
    }

    /// <summary>
    /// 특정 밸브 하나만 Emission 끄기
    /// </summary>
    public void StopValveEmissionAndReset(VavleHandle valveHandle)
    {
        ValveBlinkTarget target = FindTarget(valveHandle);

        if (target == null)
        {
            Debug.LogWarning($"[Valve Stage] {valveHandle.name}에 해당하는 ValveBlinkTarget을 찾지 못했습니다.");
            return;
        }

        StopValveEmissionAndReset(target);
    }

    /// <summary>
    /// 전체 밸브 Emission 끄기
    /// </summary>
    public void StopAllValveEmissionAndReset()
    {
        foreach (ValveBlinkTarget target in valveTargets)
        {
            if (target == null)
                continue;

            StopValveEmissionAndReset(target);
        }
    }

    private void StopValveEmissionAndReset(ValveBlinkTarget target)
    {
        if (target.blinkCoroutine != null)
        {
            StopCoroutine(target.blinkCoroutine);
            target.blinkCoroutine = null;
        }

        if (target.runtimeMaterial == null)
            return;

        SaveOriginEmission(target);

        target.runtimeMaterial.SetColor(EmissionColorID, target.originEmissionColor);

        if (target.originEmissionEnabled)
            target.runtimeMaterial.EnableKeyword("_EMISSION");
        else
            target.runtimeMaterial.DisableKeyword("_EMISSION");
    }
   
    private IEnumerator EmissionBlinkCoroutine(ValveBlinkTarget target)
    {
        if (target.runtimeMaterial == null)
            yield break;

        target.runtimeMaterial.EnableKeyword("_EMISSION");

        float safeDuration = Mathf.Max(0.01f, blinkDuration);
        float halfDuration = safeDuration * 0.5f;

        while (true)
        {
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / halfDuration);
                float power = Mathf.Lerp(0f, maxEmissionPower, t);

                target.runtimeMaterial.SetColor(
                    EmissionColorID,
                    emissionColor * power
                );

                yield return null;
            }

            elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / halfDuration);
                float power = Mathf.Lerp(maxEmissionPower, 0f, t);

                target.runtimeMaterial.SetColor(
                    EmissionColorID,
                    emissionColor * power
                );

                yield return null;
            }

            target.runtimeMaterial.SetColor(EmissionColorID, Color.black);
        }
    }

    private ValveBlinkTarget FindTarget(VavleHandle valveHandle)
    {
        if (valveHandle == null)
            return null;

        foreach (ValveBlinkTarget target in valveTargets)
        {
            if (target == null)
                continue;

            if (target.valveHandle == valveHandle)
                return target;
        }

        return null;
    }

    private void SaveOriginEmission(ValveBlinkTarget target)
    {
        if (target == null || target.runtimeMaterial == null)
            return;

        if (target.hasSavedOrigin)
            return;

        target.originEmissionEnabled = target.runtimeMaterial.IsKeywordEnabled("_EMISSION");

        if (target.runtimeMaterial.HasProperty(EmissionColorID))
            target.originEmissionColor = target.runtimeMaterial.GetColor(EmissionColorID);
        else
            target.originEmissionColor = Color.black;

        target.hasSavedOrigin = true;
    }


    public void OpenValveStage()
    {
        isValveOpenStageActive = true;
        isAllValveOpened = false;

        openedValves.Clear();
        currentOpenedValveCount = 0;

        if (managerCenterUiManager != null)
            managerCenterUiManager.ShowGuide("- OPEN : M-31A,31B,21B,21A -");

        if (valveNameCanvas != null)
            valveNameCanvas.SetActive(true);

        StartAllValveEmissionBlink();

        Debug.Log("[Valve Stage] 밸브 개방 단계 시작");

        foreach (var valvenav in valveNavObjects)
        {
            if (valvenav != null)
                valvenav.SetActive(true);
        }
    }
    public void RegisterValveOpenedForQuest(VavleHandle valveHandle)
    {
        if (!isValveOpenStageActive)
        {
            Debug.LogWarning("[Valve Stage] 밸브 개방 단계가 아닙니다.");
            return;
        }

        if (valveHandle == null)
            return;

        if (isAllValveOpened)
            return;

        if (!openedValves.Add(valveHandle))
        {
            Debug.Log($"[Valve Stage] 이미 열린 밸브입니다. {currentOpenedValveCount}/{targetValveOpenCount}");
            return;
        }

        currentOpenedValveCount = openedValves.Count;

        StopValveEmissionAndReset(valveHandle);

        Debug.Log($"[Valve Stage] 밸브 개방 완료: {currentOpenedValveCount}/{targetValveOpenCount}");

        if (managerCenterUiManager != null)
        {
            managerCenterUiManager.ShowTemporaryGuide(
                $"{valveHandle.name} 개방완료 \n {currentOpenedValveCount}/{targetValveOpenCount}"
            );
        }

        if (currentOpenedValveCount >= targetValveOpenCount)
        {
            CompleteValveOpenStage();
        }
    }
    private void CompleteValveOpenStage()
    {
        isAllValveOpened = true;
        isValveOpenStageActive = false;

        StopAllValveEmissionAndReset();

        Debug.Log("[Valve Stage] 총 4개 밸브 개방 완료");

        if (managerCenterUiManager != null)
            managerCenterUiManager.HideGuide();

        onAllValveOpened?.Invoke();

        // 필요하면 다음 대화 실행
        ChageM8();
    }
    #region 아지랑이 파티클 시나리오
    public void StartHaze()
    {
        if (hazeControl != null)
            hazeControl.StartHaze();
        managerCenterUiManager.PlayDarkFade("- 10분 경과 -");
    }
    public void StopHaze()
    {
        if (hazeControl != null)
            hazeControl.StopHaze();
    }
    #endregion

    #region 임의로 스테이지 바꿈
    public void ChageM4()
    {
        dialogueModeul.StartDialogueFrom("M4");
    }
    public void ChageM6()
    {
        dialogueModeul.StartDialogueFrom("M6");
    }
    public void ChageM8()
    {
        dialogueModeul.StartDialogueFrom("M8");
    }
    public void Change3_1()
    {
        dialogueModeul.StartDialogueFrom("M3_1");
    }
    #endregion

    #region 누출지점 위치표시

    public void LeakPosActive()
    {
        leakPosObject.SetActive(true);
    }

    #endregion

    #region 탈출지점 위치표시
    public void EscapePosActive()
    {
        escapePosObject.SetActive(true);
    }
    #endregion
}