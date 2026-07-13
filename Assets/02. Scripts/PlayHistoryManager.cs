using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayHistoryManager : MonoBehaviour
{
    public static PlayHistoryManager Instance;
    public Dictionary<PlayMode, PlayHistory> PlayHistoryDict = new();

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

        PlayHistoryDict[PlayMode.StudyRoom] = new PlayHistory
        {
            Mode = PlayMode.StudyRoom,
            History = new Dictionary<int, bool>()
            {
                { 0, false }
            }
        };

        var ctrlData = new PlayHistory
        {
            Mode = PlayMode.ControlRoom,
            History = new Dictionary<int, bool>()
        };

        for (int i = 0; i < 6; i++) 
        {
            ctrlData.History[i] = false;
        }

        PlayHistoryDict[PlayMode.ControlRoom] = ctrlData;

        PlayHistoryDict[PlayMode.GovernorStationRoom] = new PlayHistory
        {
            Mode = PlayMode.GovernorStationRoom,
            History = new Dictionary<int, bool>()
            {
                { 0, false }
            }
        };
    }

    public PlayHistory GetHistory(PlayMode mode)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
            return history;
        else return null;
    }

    public bool IsAllClear(PlayMode mode)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
            return history.IsAllClear;
        else return false;
    }

    public void ClearStage(PlayMode mode, int stage = 0)
    {
        if (PlayHistoryDict.TryGetValue(mode, out PlayHistory history))
        {
            history.History[stage] = true;

            Debug.Log($"All Clear {history.IsAllClear}");
        }
    }
}

[System.Serializable]
public class PlayHistory
{
    public PlayMode Mode;
    public Dictionary<int, bool> History;
    public bool IsAllClear => History.Count > 0 && History.Values.All(v => v);
}

public enum PlayMode
{
    StudyRoom, 
    ControlRoom,
    GovernorStationRoom
}
