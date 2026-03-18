public enum ESceneName
{
    Loading,
    Tutorial,
    Study,
    Simulation,
    Loading_Test = 4,
    Tutorial_Test = 5,
}
public enum EUiName
{
    DialogueUI,
    MainLoadingUI,
}
public enum EScenarioCategory
{
    None,
    Tutorial,
    Study,
    Simulation
}
public enum  EDialogueType
{
    Conversation, // NPC와 대화할 경우(NPC의 상태값 등 제어가 필요할 경우에 사용)
    Monologue, // 독백의 경우
    Explain, //설명문이 띄어질 경우
}
public enum  EConditionType
{
    Dialogue,//대상 object와 특정 대화가 진행되었는지
    Move, //대상 object의 위치가 특정 위치에 도달했는지
    Distance, // 대상 object와 특정 object 사이의 거리가 특정 값보다 작아졌는지
    Clicked, // 대상 object가 클릭되었는지
}
public enum EObjectType
{
    Character,
    Button,
    Door,
    Location,
    Ui,
}

public enum ECharacterState
{
    Idle,
    Hello,
    Move,
}