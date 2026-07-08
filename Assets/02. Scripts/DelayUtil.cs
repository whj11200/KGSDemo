using System;
using System.Collections;
using UnityEngine;

public class DelayUtil : MonoBehaviour
{
    private static DelayUtil _instance;

    public static void Call(float delay, Action callback)
    {
        if (_instance == null)
        {
            var go = new GameObject("DelayUtil");
            _instance = go.AddComponent<DelayUtil>();
        }

        _instance.StartCoroutine(_instance.DelayRoutine(delay, callback));
    }

    private IEnumerator DelayRoutine(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
