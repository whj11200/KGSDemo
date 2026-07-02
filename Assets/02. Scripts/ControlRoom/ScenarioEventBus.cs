using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ScenarioEventBus<T>
{
    Dictionary<T, Action<ScenarioEvent>> events = new Dictionary<T, Action<ScenarioEvent>>();

    public ScenarioEventBus()
    {

    }

    public void Subscribe(T eventType, Action<ScenarioEvent> callback)
    {
        if (!events.ContainsKey(eventType))
        {
            events[eventType] = callback;
        }
        else
        {
            events[eventType] += callback;
        }
    }

    public void Unsubscribe(T eventType)
    {
        if (events.ContainsKey(eventType))
        {
            events.Remove(eventType);
        }
    }

    public void Publish(T eventType, ScenarioEvent scenarioEvent)
    {
        if (events.ContainsKey(eventType))
        {
            events[eventType]?.Invoke(scenarioEvent);
        }
    }
}
