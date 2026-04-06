using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FireTrackTrigger : MonoBehaviour
{
    [SerializeField] List<DOTweenPath> fireTrackPaths = new();
    [SerializeField] List<FireTruckLights> fireTruckLights = new();

    private void Start()
    {
        foreach (var path in fireTrackPaths)
        {
            path.DORewind();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
           foreach (var path in fireTrackPaths)
            {
                path.DOPlay();
            }
            foreach (var light in fireTruckLights)
            {
                light.StartHeadLightSequence();
                StartCoroutine(light.WarningLampRoutine());
            }
        }
    }
}
