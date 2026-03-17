using UnityEngine;
using System.Collections;

public class SrianSenser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float rotateSpeed = 50f;
    [SerializeField] AudioSource AlarmSource;
    [SerializeField] GameObject[] Lights;

    [Header("References")]
    [SerializeField] private ValveController valve; // 밸브 컨트롤러 연결

    private Coroutine rotateRoutine;
    private bool isAlarmActive = false; // 현재 알람이 돌고 있는지 체크용

    void Start()
    {
        LightsActive(false);

  
    }

    void Update()
    {
        if (valve == null) return;

        // 밸브 상태에 따른 알람 자동 제어
        if (valve.isLeaking && !isAlarmActive)
        {
            // 누출 중인데 알람이 꺼져있으면 켜기
            HandleStartAlarm();
        }
        else if (!valve.isLeaking && isAlarmActive)
        {
            // 누출 멈췄는데 알람이 켜져있으면 끄기
            HandleStopAlarm();
        }
    }

    IEnumerator RotateLoop()
    {
        while (true)
        {
            // X축 기준으로 회전
            transform.Rotate(rotateSpeed * Time.deltaTime, 0f, 0f, Space.Self);
            yield return null;
        }
    }

    // 내부에서 사용하는 시작 처리
    private void HandleStartAlarm()
    {
        isAlarmActive = true;
        if (AlarmSource != null) AlarmSource.Play();
        if (rotateRoutine != null) StopCoroutine(rotateRoutine); // 중복 방지
        rotateRoutine = StartCoroutine(RotateLoop());
        LightsActive(true);
        Debug.Log("사이렌 작동 시작");
    }

    // 내부에서 사용하는 정지 처리
    private void HandleStopAlarm()
    {
        isAlarmActive = false;
        if (rotateRoutine != null) StopCoroutine(rotateRoutine);
        if (AlarmSource != null) AlarmSource.Stop();
        LightsActive(false);
        Debug.Log("사이렌 작동 정지");
    }

    public void LightsActive(bool toggle)
    {
        foreach (var light in Lights)
        {
            if (light != null) light.SetActive(toggle);
        }
    }
}