using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    protected GameManager gameManager;
    protected DataManager dataManager;
    protected UiManager uiManager;

    protected GameObject player;

    public bool isTest;

    public string objectID = string.Empty; // 오브젝트 식별자
    public EObjectType objectType; // 오브젝트 타입

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        player = GameObject.Find("Player");
    }

    protected virtual void Update()
    {
        foreach (var condition in scenarioConditions.Values)
        {
            //if (condition.Result) continue;
            // 조건이 만족되었는지 확인하는 로직 (예: 플레이어의 행동, 게임 상태 등)
            bool isConditionMet = CheckCondition(condition);
            if (isConditionMet)
            {
                ReportScenarioResult(condition.ConditionID ,1); // 조건이 만족되었음을 GameManager에 보고
            }
            else
            {
                ReportScenarioResult(condition.ConditionID, 0); // 조건이 만족되었음을 GameManager에 보고
            }
        }
    }

    protected virtual void OnEnable()
    {
        GetSingletonInstance();

        gameManager.RegisterObject(this);

        ObjectEventSubscribe();
    }
    protected virtual void OnDisable()
    {
        gameManager.UnregisterObject(this);

        ObjectEventUnsubscribe();
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

    protected virtual void GetSingletonInstance()
    {
        gameManager = GameManager.Instance;
        dataManager = DataManager.Instance;
        uiManager = UiManager.Instance;
    }


    #region Scenario System

    //시나리오에 필요한 데이터
    public bool isScenarioObject = false; // 시나리오 오브젝트 여부
    private Dictionary<string, ConditionData> scenarioConditions = new Dictionary<string, ConditionData>(); // 시나리오 조건 데이터
    protected string ScenarioDialogueID = string.Empty; // 시나리오에 의한 현재 대화 ID

    protected DialogueData ScenarioDialogueData; // 시나리오에 의한 현재 대화 데이터

    /// <summary>
    /// 각 오브젝트에 맞는 시나리오 이벤트 구독 메서드
    /// </summary>
    protected virtual void ObjectEventSubscribe()
    {
        gameManager.StartScenarioStep += ScenarioEventHandler;

        //오브젝트와 플레이어가 대화를 이어갈 경우를 대비하여 등록
        gameManager.OnDialogueStart += OnTriggerDialogue;
        uiManager.EndDialogue += OnEndDialogue;
    }

    /// <summary>
    /// 각 오브젝트에 맞는 시나리오 이벤트 해제 메서드
    /// </summary>
    protected virtual void ObjectEventUnsubscribe()
    {
        gameManager.StartScenarioStep -= ScenarioEventHandler;
        gameManager.OnDialogueStart -= OnTriggerDialogue;
        uiManager.EndDialogue -= OnEndDialogue;
    }

    /// <summary>
    /// 이벤트 처리 메서드
    /// </summary>
    protected virtual void ScenarioEventHandler(string scenarioStepID, Dictionary<string, ConditionData> conditionData)
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
        gameManager.ReportResult(id, result);
    }

    protected virtual bool CheckCondition(ConditionData condition)
    {
        if (condition == null) return false;

        if (condition.ConditionPrecedent != null && !scenarioConditions[condition.ConditionPrecedent].Result) return false;

        return true;
    }
    #endregion

    #region Dialogue System

    //기본 대화 데이터
    public string DefualtDialogueID; // 본 프로젝트에 의한 현재 대화 ID

    public DialogueData DefualtDialogueData; // 본 프로젝트에 의한 현재 대화 데이터

    public virtual void OnTriggerDialogue(string dialougeObjcetID, DialogueData dialogueData)
    {
        if (dialougeObjcetID != objectID) return;
        ScenarioDialogueData = dialogueData;
        Debug.Log($"Dialogue triggered for object: {objectID}, Dialogue ID: {dialogueData.DialogueID}");
        uiManager.OnStartDialogueLine(dialogueData);
        //만약 대화중 에니메이션 등의 효과가 필요하다면 이 메서드에서 처리
    }

    protected virtual void OnEndDialogue(string dialogueObjectID)
    {
        if (dialogueObjectID != objectID) return;
        if (ScenarioDialogueData != null)
        {
            var condition = scenarioConditions.Values.FirstOrDefault(c => c.ConditionType == EConditionType.Dialogue && c.ConditionValue == ScenarioDialogueData.DialogueID);

            if (condition != null) scenarioConditions[condition.ConditionID].Result = true;

            ScenarioDialogueData = null;

            gameManager.ReportResult(objectID, 2);
        }
        // 대화 종료 시 캐릭터가 수행할 행동을 정의하는 메서드
        Debug.Log($"Dialogue ended for object: {objectID}, Dialogue ID: {dialogueObjectID}");
    }

    #endregion


    /// <summary>
    /// Move 조건이 발생했을 때, ConditionValue로 전달된 ObjectID에 해당하는 오브젝트의 위치로 케릭터를 이동시키는 메서드
    /// 한번 작동 후 ConditionData의 IsProcessing을 true로 바꿔서 중복 작동 방지
    /// </summary>
    /// <param name="location">
    /// 이 위치로 케릭터를 이동시킨다.
    /// </param>
    protected virtual void OnMoveToTarget(Transform location)
    {
        if (location == null) return;
        this.gameObject.transform.position = location.position;
    }

    /// <summary>
    /// Checks the distance between the current object and the specified target and returns a value based on whether the
    /// distance is within the given threshold.
    /// </summary>
    /// <param name="target">The target GameObject to check the distance against.</param>
    /// <param name="value">The distance threshold to compare against.</param>
    /// <returns>1 if the distance to the target is less than or equal to the threshold; 2 if greater; 0 if the target is null.</returns>
    protected virtual int OnCheckDistance(GameObject target, float value)
    {
        if (target == null)
        {
            Debug.LogError("ObjectBase : Target GameObject is null.");
            return 0;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if(distance <= value) return 1;
        else  return 2;

    }

    protected virtual void OnAnimationMove(int state)
    {

    }


}
