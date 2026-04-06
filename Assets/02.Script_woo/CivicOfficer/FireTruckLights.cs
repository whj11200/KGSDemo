using DG.Tweening; // DOTween 필수
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTruckLights : MonoBehaviour
{
    [Header("Headlights (L/R Alternative)")]
    [SerializeField] private Light leftHeadLight;
    [SerializeField] private Light rightHeadLight;
    [SerializeField] private float headLightInterval = 0.3f;

    [Header("Warning Lamps (Simultaneous)")]
    [SerializeField] private List<Light> warningLamps = new List<Light>();
    [SerializeField] private float warningInterval = 0.2f;
    [SerializeField] private float warningIntensity = 5f;



    // 1. 헤드라이트: 왼쪽/오른쪽 번갈아가며 깜빡
    public void StartHeadLightSequence()
    {
        // 처음 상태 설정
        leftHeadLight.intensity = 0;
        rightHeadLight.intensity = 0;

        // 시퀀스 생성
        Sequence headSeq = DOTween.Sequence();

        // 왼쪽 켜짐 -> 오른쪽 꺼짐
        headSeq.AppendCallback(() => {
            leftHeadLight.intensity = 5f; // 밝기는 조절 가능
            rightHeadLight.intensity = 0f;
        });
        headSeq.AppendInterval(headLightInterval);

        // 왼쪽 꺼짐 -> 오른쪽 켜짐
        headSeq.AppendCallback(() => {
            leftHeadLight.intensity = 0f;
            rightHeadLight.intensity = 5f;
        });
        headSeq.AppendInterval(headLightInterval);

        // 무한 반복
        headSeq.SetLoops(-1);
    }

    // 2. 경광등: 전체가 동시에 깜빡
    public IEnumerator WarningLampRoutine()
    {
        while (true)
        {
            // 전체 ON
            SetWarningLampsIntensity(warningIntensity);
            yield return new WaitForSeconds(warningInterval);

            // 전체 OFF
            SetWarningLampsIntensity(0f);
            yield return new WaitForSeconds(warningInterval);
        }
    }
    // 경광등 리스트의 밝기를 조절하는 보조 함수
    private void SetWarningLampsIntensity(float intensity)
    {
        foreach (var lamp in warningLamps)
        {
            if (lamp != null) lamp.intensity = intensity;
        }
    }
    // 필요시 라이트를 끄는 기능
    public void StopLights()
    {
        leftHeadLight.DOKill();
        rightHeadLight.DOKill();
        foreach (var lamp in warningLamps) lamp.DOKill();
    }
}