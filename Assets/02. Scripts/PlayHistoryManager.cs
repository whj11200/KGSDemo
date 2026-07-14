using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayHistoryManager : MonoBehaviour
{
    public static PlayHistoryManager Instance;
    public Dictionary<EScenarioCategory, PlayHistory> PlayHistoryDict = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        else
        {
            if (Instance != this) 
                Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        PlayHistoryDict.Clear();

        PlayHistoryDict[EScenarioCategory.Study] = new PlayHistory
        {
            Mode = EScenarioCategory.Study,
            History = new Dictionary<int, bool>()
            {
                { 0, false }
            }
        };

        var ctrlData = new PlayHistory
        {
            Mode = EScenarioCategory.ControlRoom,
            History = new Dictionary<int, bool>()
        };

        for (int i = 0; i < 6; i++) 
        {
            ctrlData.History[i] = false;
        }

        PlayHistoryDict[EScenarioCategory.ControlRoom] = ctrlData;

        PlayHistoryDict[EScenarioCategory.GovernorStationRoom] = new PlayHistory
        {
            Mode = EScenarioCategory.GovernorStationRoom,
            History = new Dictionary<int, bool>()
            {
                { 0, false }
            }
        };
    }

    public PlayHistory GetHistory(EScenarioCategory mode)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
            return history;
        else return null;
    }

    public bool IsAllClear(EScenarioCategory mode)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
            return history.IsAllClear;
        else return false;
    }

    public void ClearStage(EScenarioCategory mode, int stage = 0)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
        {
            history.History[stage] = true;
        }
    }
}

[System.Serializable]
public class PlayHistory
{
    public EScenarioCategory Mode;
    public Dictionary<int, bool> History;
    public bool IsAllClear => History.Count > 0 && History.Values.All(v => v);
}
