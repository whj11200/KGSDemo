using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

/// <summary>
/// 각 시나리오에 맞게 정리된 데이터를 제공하기 위한 클래스
/// </summary>
public class CurrentScenario
{
    /// <summary>
    /// 현재 시나리오 이름(시나리오 카테고리)
    /// </summary>
    public EScenarioCategory ScenarioName { get; set; }

    /// <summary>
    /// 현재 시나리오의 정보(스텝 순서대로 정렬 - 연계 퀘스트 같은 느낌)
    /// </summary>
    public List<ScenarioData> ScenarioDatas { get; set; }

    /// <summary>
    /// 다이얼로그 리스트(시나리오 아이디(스텝)에 근거한 string List, 대화에 필요한 String내용도 포함하고 있음) 
    /// </summary>
    public Dictionary<string, DialogueData> DialogueDatas { get; set; }

    /// <summary>
    /// 현재 시나리오 스탭 조건 정보(시나리오 아이디(스텝)에 근거한 Condition List)
    /// </summary>
    public Dictionary<string, List<ConditionData>> ConditionDatas { get; set; }

    /// <summary>
    /// 해당 시나리오 스텝에 존재하는 대화가 있는지 확인한다.
    /// </summary>
    /// <param name="scenariID"></param>
    /// <returns></returns>
    public bool IsDialogueConfirm(string scenariID)
    {
        if (DialogueDatas[scenariID] != null && DialogueDatas[scenariID].StringDatas.Count > 0) return true;
        return false;
    }
    /// <summary>
    /// 현재 진행중인 ScenarioID로 조건을 검색하여 종료 조건이 완료 되었는지 검색
    /// </summary>
    /// <param name="currentScenarioID"></param>
    /// <returns></returns>
    public bool IsScenarioCoditionConfirm(string currentScenarioID)
    {
        foreach (var item in ConditionDatas[currentScenarioID])
        {
            if (item.Result == false) return false;
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="scenarioID"></param>
    /// <param name="conditionID"></param>
    /// <param name="report"></param>
    public void ConditionReport(string scenarioID, string conditionID, bool report)
    {
        // conditionDatas[ScenarioID] 중 ConditionID를 가지고 있는 조건에 결과를 적용
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string CallScenarioID(int index)
    {
        return ScenarioDatas[index].ScenarioID;
    }

    /// <summary>
    /// 현재의 조건 리스트를 ConditionID를 Key값으로 갖는 Dictionary로 바꾼다.
    /// </summary>
    /// <param name="scenarioID"></param>
    /// <returns></returns>
    public Dictionary<string, ConditionData> ConvertCondtionListToDic(string scenarioID)
    {
        if (!ConditionDatas.TryGetValue(scenarioID, out var conditionList))
            return new Dictionary<string, ConditionData>();

        return conditionList.ToDictionary(c => c.ConditionID, c => c);
    }
}

public class ScenarioData 
{
    public string ScenarioID { get; set; }
    public EScenarioCategory Category { get; set; }
    public string DialogueID { get; set; }
    public string Description { get; set; }
}
public class DialogueData
{
    public string DialogueID { get; set; }
    public string SpeakerID { get; set; }
    public EDialogueType DialogueType { get; set; }
    public int DialogueIndex { get; set; }
    public List<StringData> StringDatas { get; set; }

    public StringData GetCurrentStringData()
    {
        if (DialogueIndex < StringDatas.Count)
        {
            return StringDatas[DialogueIndex];
        }
        return null;
    }
}
public class StringData
{
    public string StringID { get; set; }
    public string StringType { get; set; }
    public string Text_Kr { get; set; }
    public string Text_En { get; set; }
    public string SpeakerID { get; set; }
    public ECharacterState SpeakerStatus { get; set; } //Enum으로 관리
    public string SoundPath { get; set; }
    public string DialogueID { get; set; }
    public int Sequence { get; set; }

}
public class  ConditionData
{
    public string ConditionID { get; set; }
    public string ScenarioID { get; set; }
    public string ConditionPrecedent { get; set; }
    public string ActorID { get; set; }
    public EActorType ActionType { get; set; } //Enum으로 관리
    public string TargetID { get; set; }
    public float Value { get; set; }
    /// <summary>
    /// 조건이 처리 중인지 여부를 나타내는 변수
    /// </summary>
    public bool IsStarted = false; 
    public bool Result { get; set; }
}
