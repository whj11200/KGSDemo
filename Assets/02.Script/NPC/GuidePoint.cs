using System;
using UnityEngine;

[Serializable]
public class GuidePoint
{
    public string id;                 // "A", "B", "C"
    public Transform target;
    public float arriveDistance = 0.4f;
    public float explainPlayerDistance = 2f;

    public bool facePlayerOnArrive = true;
}