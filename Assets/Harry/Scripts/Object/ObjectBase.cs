using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    protected GameManager gameManager;
    protected DataManager dataManager;
    protected UiManager uiManager;

    public bool isTest;

    public string objectID = string.Empty; // 오브젝트 식별자
    public EObjectType objectType; // 오브젝트 타입

    //시나리오에 필요한 데이터
    public bool isScenarioObject = false; // 시나리오 오브젝트 여부
    private Dictionary<string, ConditionData> scenarioConditions = new Dictionary<string, ConditionData>(); // 시나리오 조건 데이터
    protected string ScenarioDialogueID = string.Empty; // 시나리오에 의한 현재 대화 ID
    protected DialogueData ScenarioDialogueData; // 시나리오에 의한 현재 대화 데이터

    //기본 대화 데이터
    public string DefualtDialogueID; // 본 프로젝트에 의한 현재 대화 ID
    public DialogueData DefualtDialogueData; // 본 프로젝트에 의한 현재 대화 데이터

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        if (!isTest) return;

        GetSingletonInstance();

        gameManager.RegisterObject(this);

        //오브젝트와 플레이어가 대화를 이어갈 경우를 대비하여 등록
        gameManager.OnDialogueStart += OnTriggerDialogue;
        uiManager.EndDialogue += OnEndDialogue;
    }

    protected virtual void Update()
    {
        foreach (var condition in scenarioConditions.Values)
        {
            if (condition.Result) continue;
            // 조건이 만족되었는지 확인하는 로직 (예: 플레이어의 행동, 게임 상태 등)
            // 이 부분은 실제 게임 로직에 맞게 구현해야 합니다.
            bool isConditionMet = CheckCondition(condition);
            if (isConditionMet)
            {
                condition.Result = true;
                ReportScenarioResult(condition.ConditionID ,1); // 조건이 만족되었음을 GameManager에 보고
            }
        }
    }

    protected virtual void OnEnable()
    {
        //활성화된 오브젝트만 GameManager의 오브젝트 딕셔너리에 등록
        if (isTest) return;
        gameManager.RegisterObject(this);

        //오브젝트와 플레이어가 대화를 이어갈 경우를 대비하여 등록
        gameManager.OnDialogueStart += OnTriggerDialogue;
        uiManager.EndDialogue += OnEndDialogue;


        if (isScenarioObject) ScenarioEventSubscribe();
    }
    protected virtual void OnDisable()
    {
        gameManager.UnregisterObject(this);
        gameManager.OnDialogueStart -= OnTriggerDialogue;
        uiManager.EndDialogue -= OnEndDialogue;

        ScenarioEventUnsubscribe();
    }

    /// <summary>
    /// 오브젝트가 수행하는 특정 동작
    /// </summary>
    /// <param name="obj">
    /// 추후 매개변수를 사용하여 동작의 종류나 세부사항을 전달할 수 있도록 설계
    /// </param>
    public virtual void OnFunction(object obj = null)
    {

    }

    public virtual void OnTriggerDialogue(string dialougeObjcetID, DialogueData dialogueData)
    {
        if (dialougeObjcetID != objectID) return;
        ScenarioDialogueData = dialogueData;
        Debug.Log($"Dialogue triggered for object: {objectID}, Dialogue ID: {dialogueData.DialogueID}");
        uiManager.OnStartDialogueLine(dialogueData);
        //만약 대화중 에니메이션 등의 효과가 필요하다면 이 메서드에서 처리
    }

    /// <summary>
    /// 각 오브젝트에 맞는 시나리오 이벤트 구독 메서드
    /// </summary>
    protected virtual void ScenarioEventSubscribe()
    {
        gameManager.StartScenarioStep += ScenarioEventHandler;
    }

    /// <summary>
    /// 각 오브젝트에 맞는 시나리오 이벤트 해제 메서드
    /// </summary>
    protected virtual void ScenarioEventUnsubscribe()
    {
        gameManager.StartScenarioStep -= ScenarioEventHandler;
    }

    /// <summary>
    /// 이벤트 처리 메서드
    /// </summary>
    protected virtual void ScenarioEventHandler( string scenarioStepID, Dictionary<string, ConditionData> conditionData)
    {
        if (conditionData == null)
            return;

        var matchedConditions = conditionData.Values
            .Where(c => c.TargetID == objectID)
            .ToList();

        if (matchedConditions.Count == 0)
            return;

        foreach (var condition in matchedConditions)
        {
            scenarioConditions[condition.ConditionID] = condition;

        }
    }

    /// <summary>
    /// GameManager에 시나리오 결과 보고 메서드
    /// </summary>
    /// <param name="result">
    /// 현재 시나리오 만족 조건의 결과값을 전달하는 매개변수
    /// </param>
    protected void ReportScenarioResult(string id, int result)
    {
        gameManager.ReportScenarioResult(id, result);
    }


    protected virtual void OnEndDialogue(string dialogueObjectID)
    {
        if(dialogueObjectID != objectID) return;
        if(ScenarioDialogueData != null)
        {
            ScenarioDialogueData = null;
            gameManager.ReportScenarioResult(objectID, 2);
        }
        // 대화 종료 시 캐릭터가 수행할 행동을 정의하는 메서드
        Debug.Log($"Dialogue ended for object: {objectID}, Dialogue ID: {dialogueObjectID}");

    }

    protected virtual void GetSingletonInstance()
    {
        gameManager = GameManager.Instance;
        dataManager = DataManager.Instance;
        uiManager = UiManager.Instance;
    }

    protected virtual bool CheckCondition(ConditionData condition)
    {
        // 시나리오 조건에 따른 행동을 정의하는 메서드
        // condition의 ConditionType과 ConditionValue를 기반으로 행동을 결정
        Debug.Log($"Scenario action triggered for object: {objectID}, Condition ID: {condition.ConditionID}");

        return true;
    }


}
