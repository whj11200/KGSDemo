using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private Dictionary<string, ScenarioData> ScenarioData;
    private Dictionary<string, DialogueData> DialogueData;
    private Dictionary<string, StringData> StringData;
    private Dictionary<string, ConditionData> ConditionData;


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
    }

    #region 외부 접근 함수
    /// <summary>
    /// 게임 시작 시 필요한 데이터를 초기화하는 메서드
    /// </summary>
    public void Initialization() => LoadDocument();

    /// <summary>
    /// Key에 해당하는 데이터를 반환하는 제네릭 메서드.
    /// ScenarioData, DialogueData, StringData, ConditionData, CurrentScenario 중 하나의 타입을 T로 지정하여 호출할 수 있다.
    /// </summary>
    /// <typeparam name="T">Table 종류</typeparam>
    /// <param name="key">Table에서 검색할 ID</param>
    /// <returns></returns>
    public T CallData<T>(string key) 
    { 
        switch(typeof(T).Name)
        {
            case nameof(ScenarioData):
                if (ScenarioData.TryGetValue(key, out var scenarioData))
                    return (T)(object)scenarioData;
                break;
            case nameof(DialogueData):
                if (DialogueData.TryGetValue(key, out var dialogueData))
                    return (T)(object)dialogueData;
                break;
            case nameof(StringData):
                if (StringData.TryGetValue(key, out var stringData))
                    return (T)(object)stringData;
                break;
            case nameof(ConditionData):
                if (ConditionData.TryGetValue(key, out var conditionData))
                    return (T)(object)conditionData;
                break;
            case nameof(CurrentScenario):
                if (Enum.TryParse<EScenarioCategory>(key, true, out var category))
                {
                    var scenario = GetCurrentScenario(category);
                    return (T)(object)scenario;
                }

                Debug.LogWarning($"Key '{key}' cannot be converted to EScenarioCategory.");
                break;
        }

        Debug.LogWarning($"Data with key '{key}' not found for type '{typeof(T).Name}'.");
        return default;
    }
    #endregion


    #region 내부 함수
    /// <summary>
    /// CurrentScenario 객체를 생성하는 메서드입니다. scenarioName을 기반으로,
    /// ScenarioData, DialogueData, StringData, ConditionData에서 필요한 데이터를 추출하여 CurrentScenario 객체를 구성한다.
    /// </summary>
    /// <param name="scenarioName">
    /// CurrentScenario 객체를 생성할 때 사용할 시나리오 이름
    /// </param>
    /// <returns></returns>
    private CurrentScenario GetCurrentScenario(EScenarioCategory category)
    {
        // 1. 해당 Category에 해당하는 ScenarioData 전체 수집
        var scenarioSteps = ScenarioData.Values
            .Where(s => s.Category == category)
            .OrderBy(s => s.ScenarioID) // 필요 시 StepOrder 필드 권장
            .ToList();

        if (scenarioSteps.Count == 0)
        {
            Debug.LogWarning($"No scenario steps found for category: {category}");
            return null;
        }

        // 2. CurrentScenario 생성
        var currentScenario = new CurrentScenario
        {
            ScenarioName = category,
            ScenarioDatas = scenarioSteps,
            DialogueDatas = new Dictionary<string, DialogueData>(),
            ConditionDatas = new Dictionary<string, List<ConditionData>>()
        };

        // 3. Dialogue + String 연결
        foreach (var step in scenarioSteps)
        {
            if (string.IsNullOrEmpty(step.DialogueID))
                continue;

            if (DialogueData.TryGetValue(step.DialogueID, out var dialogue))
            {
                // 실행용 복사본 생성 (원본 보호)
                var dialogueCopy = new DialogueData
                {
                    DialogueID = dialogue.DialogueID,
                    SpeakerID = dialogue.SpeakerID,
                    DialogueType = dialogue.DialogueType,
                    StringDatas = StringData.Values
                        .Where(s => s.DialogueID == dialogue.DialogueID)
                        .OrderBy(s => s.Sequence)
                        .ToList()
                };

                currentScenario.DialogueDatas[step.ScenarioID] = dialogueCopy;
            }
        }

        // 4. Condition 연결 (실행용 복사본 생성)
        foreach (var step in scenarioSteps)
        {
            var conditions = ConditionData.Values
                .Where(c => c.ScenarioID == step.ScenarioID)
                .Select(c => new ConditionData
                {
                    ConditionID = c.ConditionID,
                    ScenarioID = c.ScenarioID,
                    ConditionPrecedent = c.ConditionPrecedent,
                    TargetID = c.TargetID,
                    ConditionType = c.ConditionType,
                    ConditionValue = c.ConditionValue,
                    Result = false // 실행용 초기화
                })
                .ToList();

            currentScenario.ConditionDatas[step.ScenarioID] = conditions;
        }

        return currentScenario;
    }
    private void LoadDocument(string scenarioName = null)
    {
        string path = Path.Combine(Application.streamingAssetsPath, "DataTable.xlsx");

        using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = ExcelReaderFactory.CreateOpenXmlReader(stream))
        {
            var dataSet = reader.AsDataSet();
            var tables = dataSet.Tables;

            ScenarioData = LoadTable<ScenarioData>(tables["ScenarioTable"], "ScenarioID");

            DialogueData = LoadTable<DialogueData>(tables["DialogueTable"], "DialogueID");

            StringData = LoadTable<StringData>(tables["StringTable"], "StringID");

            ConditionData = LoadTable<ConditionData>(tables["ConditionTable"], "ConditionID");
        }

        Debug.Log("Excel Load Complete");
    }
    private Dictionary<string, T> LoadTable<T>(DataTable table, string keyColumnName) where T : new()
    {
        var dict = new Dictionary<string, T>();

        // 1행 = 헤더
        var headers = new List<string>();
        for (int col = 0; col < table.Columns.Count; col++)
        {
            headers.Add(table.Rows[0][col].ToString());
        }

        // 2행부터 데이터 시작
        for (int rowIndex = 1; rowIndex < table.Rows.Count; rowIndex++)
        {
            var row = table.Rows[rowIndex];

            T obj = new T();
            string key = null;

            for (int col = 0; col < headers.Count; col++)
            {
                string header = headers[col];
                var prop = typeof(T).GetProperty(header);

                if (prop == null) continue;

                object value = row[col];
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                    continue;

                // 타입 변환 처리
                object convertedValue = ConvertValue(prop.PropertyType, value);

                prop.SetValue(obj, convertedValue);

                if (header == keyColumnName)
                {
                    key = value.ToString();
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                dict[key] = obj;
            }
        }

        return dict;
    }
    private object ConvertValue(Type targetType, object value)
    {
        if (value == null) return null;

        string stringValue = value.ToString().Trim();

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, stringValue);
        }

        if (targetType == typeof(int))
        {
            if (int.TryParse(stringValue, out int intResult))
                return intResult;

            if (double.TryParse(stringValue, out double doubleResult))
                return (int)doubleResult;

            Debug.LogError($"Int 변환 실패: {stringValue}");
            return 0;
        }

        if (targetType == typeof(bool))
        {
            if (bool.TryParse(stringValue, out bool boolResult))
                return boolResult;

            if (int.TryParse(stringValue, out int intBool))
                return intBool != 0;

            return false;
        }

        if (targetType == typeof(string))
            return stringValue;

        return Convert.ChangeType(value, targetType);
    }
    #endregion
}
