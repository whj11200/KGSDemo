using DG.Tweening.Core.Easing;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum EScenarioState
{
    None,
    Dialogue,
    Scenario,
}

public class GameManager : MonoBehaviour
{
    private DataManager dataManager;

    private void Awake()
    {
        SetSingleton();
    }
    private void Start()
    {
        Intialization();
    }
    private void Update()
    {

    }
    private void Intialization()
    {
        dataManager = DataManager.Instance;
    }

    #region Singleton
    public static GameManager Instance { get; private set; }
    private void SetSingleton()
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

    }
    #endregion

    #region Scenario System

    /// <summary>
    /// 현재 진행중인 시나로오 스텝의 ID
    /// </summary>
    private string currentScenarioStepID;
    public EScenarioState state = EScenarioState.None;
    /// <summary>
    /// isScenarioActive : 시나리오 진행 여부
    /// </summary>
    private bool isScenarioActive = false;
    /// <summary>
    /// CurrentScenarioName : 현재 진행 중인 시나리오 이름
    /// </summary>
    private string currentScenarioName = string.Empty;
    /// <summary>
    /// CurrentScenarioIndex : 현재 진행 중인 시나리오 인덱스. CurrentScenario에서 ScenarioDatas의 index로 사용됨
    /// </summary>
    private int currentScenarioIndex = 0;
    /// <summary>
    /// currentScenarioStep : 현재 진행 중인 시나리오의 단계별 모든 데이터를 저장하는 클레스
    /// </summary>
    private CurrentScenario currentScenario;

    /// <summary>
    /// 현재 시작하는 시나리오 스텝의 종료 조건을 각 오브젝트로 전달하기 위한 이벤트
    /// </summary>
    public event Action<string, DialogueData, Dictionary<string, ConditionData>> StartScenarioStep;



    /// <summary>
    /// 시나리오를 시작할 때 시나리오 관련 데이터를 초기화 및 시작하는 코드
    /// </summary>
    /// <param name="scenarioCategory"></param>
    public void StartScenario(string scenarioCategory)
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
        ScenarioEventGenerator(currentScenarioStepID);
    }

    /// <summary>
    /// 외부 오브젝트에서 시나리오, 어떠한 절차의 공정과 관련된 상태를 전달하는 함수
    /// </summary>
    /// <param name="ResultID"></param>
    /// <param name="result"> 0 = false, 1 = ture, 2 = Dialogue End </param>
    public void ReportResult(string ResultID , int result = 0)
    {
        // 예외 처리
        if (string.IsNullOrEmpty(ResultID))
        {
            Debug.LogError("Object name is null or empty.");
            return;
        }
        if (ResultID == "SceneMove")
        {
            OnSceneMove(result);
            return;
        }
        if (result == 0) currentScenario.ConditionReport(currentScenarioStepID, ResultID, false);
        else if (result == 1) currentScenario.ConditionReport(currentScenarioStepID, ResultID, true);
        
        else
        {
            Debug.LogError($"Invalid result value:{ResultID} / {result}. Expected 0, 1, or 2.");
            return;
        }

        //현재 진행중인 시나리오 스텝 중 모든 조건이 완료 되었다면 다음 내용을 확인 후 진행
        if (currentScenario.IsScenarioCoditionConfirm(currentScenarioStepID))
        {
            Debug.Log("시나리오 리포트 후 시나리오 체크");
            ScenarioEventGenerator(currentScenarioStepID);
        }
    }

    /// <summary>
    /// 현재 시나리오 스텝이 완료되었을 때 다음 시나리오가 있는지, 시나리오를 진행해야 하는지 판단하는 코드
    /// </summary>
    private void ScenarioEventGenerator(string scenarioStepID)
    {
        // 2. 현재 시나리오 스텝과 시나리오를 파악해서 시나리오 조건 진행
        if (currentScenario.ConditionDatas[scenarioStepID] != null && currentScenario.ScenarioDatas.Count >= currentScenarioIndex)
        {
            currentScenarioIndex++;
            StartScenarioStep?.Invoke(scenarioStepID, currentScenario.DialogueDatas[scenarioStepID], currentScenario.ConvertCondtionListToDic(scenarioStepID));
            Debug.Log("[GameManager] Scenario Generated : " + scenarioStepID);
            return;
        }
        else
        {
            Debug.Log("시나리오 진행하지않음");
            isScenarioActive = false;
            return;
        }
    }
    #endregion

    #region GameObject Management
    /// <summary>
    /// 각 씬 내부에 있는 ObjectBase를 상속받은 오브젝트들을 관리하는 딕셔너리
    /// Key: ObjectID, Value: ObjectBase를 상속받은 오브젝트의 컴포넌트
    /// </summary>
    private Dictionary<string, ObjectBase> Objects = new Dictionary<string, ObjectBase>();
    public void RegisterObject(object obj)
    {
        if (obj is ObjectBase objectBase && !Objects.ContainsKey(objectBase.objectID))
        {
            Objects.Add(objectBase.objectID, objectBase);
        }
    }

    public void UnregisterObject(object obj)
    {
        if (obj is ObjectBase objectBase && !Objects.ContainsKey(objectBase.objectID))
        {
            Objects.Remove(objectBase.objectID);
        }
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

    bool IsPaused = false;

    public event Action<string> CallSceneMove;

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
    private bool CheckPause()
    {
        return IsPaused;
    }
    private bool CheckScenarioActive()
    {
        return isScenarioActive;
    }
    private void OnSceneMove(string name)
    {
        SceneManager.LoadScene(name);
    }

    private void OnSceneMove(int number)
    {
        SceneManager.LoadScene(number);
    }

    #endregion

}
