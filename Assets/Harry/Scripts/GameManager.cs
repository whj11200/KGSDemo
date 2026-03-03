using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EScenarioState
{
    None,
    Dialogue,
    Scenario,
}

public class GameManager : MonoBehaviour
{

    private DataManager dataManager;

    #region Parameters
    public static GameManager Instance { get; private set; }
    public EScenarioState state = EScenarioState.None;

    /// <summary>
    /// isScenarioActive : 시나리오 진행 여부
    /// </summary>
    private bool isScenarioActive = false;
    /// <summary>
    /// currentScenarioStep : 현재 진행 중인 시나리오의 단계별 모든 데이터를 저장하는 클레스
    /// </summary>
    private CurrentScenario currentScenario;
    /// <summary>
    /// CurrentScenarioName : 현재 진행 중인 시나리오 이름
    /// </summary>
    private string currentScenarioName = string.Empty;
    /// <summary>
    /// CurrentScenarioIndex : 현재 진행 중인 시나리오 인덱스. CurrentScenario에서 ScenarioDatas의 index로 사용됨
    /// </summary>
    private int currentScenarioIndex = 0;
    /// <summary>
    /// 현재 진행중인 시나로오 스텝의 ID
    /// </summary>
    private string currentScenarioStepID;
    /// <summary>
    /// CurrentDialogueIndex : 현재 진행 중인 대사 인덱스
    /// </summary>
    private int currentDialogueIndex = 0;
    /// <summary>
    /// 진행중인 대사의 종료 여부를 나타내는 변수. 이벤트 발생 시 최신화 됨
    /// </summary>
    private bool currentDialogueEnd = false;


    /// <summary>
    /// 각 씬 내부에 있는 ObjectBase를 상속받은 오브젝트들을 관리하는 딕셔너리
    /// Key: ObjectID, Value: ObjectBase를 상속받은 오브젝트의 컴포넌트
    /// </summary>
    private Dictionary<string, ObjectBase> Objects = new Dictionary<string, ObjectBase>();

    bool IsPaused = false;

    #endregion

    #region Events
    public event Action<string> ScenarioLoadFail;

    /// <summary>
    /// 대화 시작 이벤트, string은 대상 ObjectID, DialogueData는 대화에 필요한 모든 데이터를 담고 있는 클래스(string List도 가지고 있어야 함.)
    /// </summary>
    public event Action<string, DialogueData> OnDialogueStart;
    /// <summary>
    /// 현재 시작하는 시나리오 스텝의 종료 조건을 각 오브젝트로 전달하기 위한 이벤트
    /// </summary>
    public event Action<string, Dictionary<string, ConditionData>> StartScenarioStep;
    /// <summary>
    /// 현재 시나리오 스텝이 종료되었다고 알려주는 이벤트. 현재 시나리오 ID를 전달 함. 
    /// </summary>
    public event Action<string> EndScenarioStep;

    #endregion


    private void Awake()
    {
        // 이미 인스턴스가 존재하면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 유지
        DontDestroyOnLoad(gameObject);

        dataManager = DataManager.Instance;
    }
    private void Start()
    {
    }
    private void Update()
    {
        // 1. 프로그램 작동 중 일시정지 상태가 변경되었는지 체크
        //if (IsPaused) return;

        //ScenarioProcessController()
    }


    #region 시나리오 제어 함수
    /// <summary>
    /// 시나리오를 시작할 때 시나리오 관련 데이터를 초기화 및 시작하는 코드
    /// </summary>
    /// <param name="scenarioCategory"></param>
    public void SetScenarioInfo(string scenarioCategory)
    {
        //예외처리
        if (scenarioCategory == null || scenarioCategory == EScenarioCategory.None.ToString())
        {
            Debug.LogError("Scenario category is null.");
            return;
        }

        // 1. 시나리오 초기화
        currentScenarioName = scenarioCategory;
        currentScenarioIndex = 0;
        currentDialogueIndex = 0;
        Debug.Log("시나리오 초기화");

        // 2. DataManger에서 시나리오 데이터 획득
        currentScenario = dataManager.CallData<CurrentScenario>(scenarioCategory);
        if (currentScenario == null)
        {
            Debug.LogError($"Scenario data for category '{scenarioCategory}' not found.");
            isScenarioActive = false;
            return;
        }
        isScenarioActive = true;
        currentScenarioStepID = currentScenario.CallScenarioID(currentScenarioIndex);

        // 3. 대화가 있을경우 대화 시작 이벤트 발생, 없을 경우 시나리오 시작 이벤트 발생
        Debug.Log("초기화 후 시나리오 체크");
        ScenarioChecker(currentScenarioStepID);
    }

    /// <summary>
    /// 외부 오브젝트에서 시나리오와 관련된 상태를 전달하는 함수
    /// </summary>
    /// <param name="ResultID"></param>
    /// <param name="result"> 0 = false, 1 = ture, 2 = Dialogue End </param>
    public void ReportScenarioResult(string ResultID , int result = 0)
    {
        // 예외 처리
        if (string.IsNullOrEmpty(ResultID))
        {
            Debug.LogError("Object name is null or empty.");
            return;
        }

        if (result == 0) currentScenario.ConditionReport(currentScenarioStepID, ResultID, false);
        else if (result == 1) currentScenario.ConditionReport(currentScenarioStepID, ResultID, true);
        else if (ResultID == currentScenario.DialogueDatas[currentScenarioStepID].SpeakerID && result == 2) currentDialogueEnd = true;
        else
        {
            Debug.LogError($"Invalid result value:{ResultID} / {result}. Expected 0, 1, or 2.");
            return;
        }

        //현재 진행중인 시나리오 스텝 중 모든 조건이 완료 되었다면 다음 내용을 확인 후 진행
        if (currentScenario.IsScenarioCoditionConfirm(currentScenarioStepID))
        {
            Debug.Log("시나리오 리포트 후 시나리오 체크");
            ScenarioChecker(currentScenarioStepID);
        }
    }
    #endregion

    #region 게임오브젝트 관리
    public void RegisterObject(ObjectBase obj)
    {
        if (!Objects.ContainsKey(obj.objectID))
            Objects.Add(obj.objectID, obj);
    }

    public void UnregisterObject(ObjectBase obj)
    {
        if (Objects.ContainsKey(obj.objectID))
            Objects.Remove(obj.objectID);
    }
    public ObjectBase GetObject(string objectID)
    {
        if (Objects.TryGetValue(objectID, out ObjectBase obj))
            return obj;
        else
        {
            Debug.LogError($"Object with ID '{objectID}' not found.");
            return null;
        }
    }
    #endregion

    #region 프로그램 제어
    /// <summary>
    /// 일시정지 메서드
    /// </summary>
    public void Pause()
    {
        Time.timeScale = 0f;
        IsPaused = true;
    }
    /// <summary>
    /// 일시정지 해재 메서드
    /// </summary>
    public void Resume()
    {
        Time.timeScale = 1f;
        IsPaused = false;
    }
    /// <summary>
    /// 현재 일시정지 상태를 토글하는 메서드
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    #endregion


    #region 내부 기능 구현 함수
    private bool CheckPause()
    {
        return IsPaused;
    }
    private bool CheckScenarioActive()
    {
        return isScenarioActive;
    }

    /// <summary>
    /// 현재 시나리오 스텝이 완료되었을 때 다음 시나리오가 있는지, 시나리오를 진행해야 하는지 판단하는 코드
    /// </summary>
    private void ScenarioChecker(string scenarioStepID)
    {
        // 1. 현재 시나리오 스텝 중 대화가 진행 되었는지 체크하고 대화 진행
        if (currentScenario.IsDialogueConfirm(scenarioStepID) && !currentDialogueEnd)
        {
            ScenarioEventGenerator(scenarioStepID, (int)EScenarioState.Dialogue);
            return;
        }

        // 2. 현재 시나리오 스텝과 시나리오를 파악해서 시나리오 조건 진행
        if (currentScenario.ConditionDatas[scenarioStepID] != null && currentScenario.ScenarioDatas.Count >= currentScenarioIndex)
        {
            currentScenarioIndex++;
            ScenarioEventGenerator(scenarioStepID, (int)EScenarioState.Scenario);
            return;
        }
    }

    /// <summary>
    /// 시나리오 진행 시 이벤트 발생 및 분기점
    /// </summary>
    /// <param name="ScnearioState"></param>
    private void ScenarioEventGenerator(string scenarioStepID, int ScnearioState = 0)
    {
        switch (ScnearioState)
        {
            case (int)EScenarioState.None :
                break;
            case (int)EScenarioState.Dialogue:
                OnDialogueStart(currentScenario.DialogueDatas[scenarioStepID].SpeakerID, currentScenario.DialogueDatas[scenarioStepID]);
                Debug.Log($"Dialogue Start Event Triggered for Scenario Step ID: {scenarioStepID}");
                break;
            case (int)EScenarioState.Scenario:
                StartScenarioStep(scenarioStepID, currentScenario.ConvertCondtionListToDic(scenarioStepID));
                Debug.Log($"Scenario Step Start Event Triggered for Scenario Step ID: {scenarioStepID}");
                break;
        }
    }

    #endregion
}
