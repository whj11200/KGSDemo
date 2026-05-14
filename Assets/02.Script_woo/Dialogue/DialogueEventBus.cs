using System;
using System.Collections.Generic;

public static class DialogueEventBus
{
    private static readonly Dictionary<string, Action> events
        = new Dictionary<string, Action>();

    public static void Subscribe(string key, Action action)
    {
        if (string.IsNullOrEmpty(key) || action == null)
            return;

        if (!events.ContainsKey(key))
            events[key] = action;
        else
            events[key] += action;
    }

    public static void Unsubscribe(string key, Action action)
    {
        if (string.IsNullOrEmpty(key) || action == null)
            return;

        if (events.TryGetValue(key, out var current))
        {
            current -= action;
            if (current == null)
                events.Remove(key);
            else
                events[key] = current;
        }
    }

    public static void Raise(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        if (events.TryGetValue(key, out var action))
        {
            UnityEngine.Debug.Log($"<color=green>이벤트 실행 성공:</color> {key}");
            action?.Invoke();
        }
        else
        {
            UnityEngine.Debug.LogWarning($"<color=red>이벤트 실행 실패 (구독자 없음):</color> {key}");
        }
    }

    public static void Clear()
    {
        events.Clear();
    }
}
