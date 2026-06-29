using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VavleHandle : MonoBehaviour, IMouseInteractable
{
    [Header("Object Position")]
    [SerializeField] private Vector3 currentPosition;

    [Header("Rotation Setting")]
    [SerializeField] private float rotateAngle = 360f;
    [SerializeField] private float rotateDuration = 1f;

    [Header("State")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private bool isReverseNext = false;

    [Header("Other Manager Event")]
    [SerializeField] private bool useCompleteEvent = false;
    [SerializeField] private UnityEvent onRotateComplete;

    private Coroutine rotateCoroutine;

    public Vector3 CurrentPosition => currentPosition;

    private void Start()
    {
        UpdateCurrentPosition();
    }

    private void Update()
    {
        UpdateCurrentPosition();
    }

    private void UpdateCurrentPosition()
    {
        currentPosition = transform.position;
    }

    public void ClickCancle()
    {

    }

    public void ClickEnter()
    {
        ToggleRotate();
    }

    public void ClickExit()
    {

    }

    public void HoverEnter()
    {

    }

    public void HoverExit()
    {

    }

    /// <summary>
    /// 정방향 / 역방향 360도 회전 토글
    /// </summary>
    public void ToggleRotate()
    {
        if (isRotating)
            return;

        if (isReverseNext)
        {
            RotateReverse360();
        }
        else
        {
            RotateForward360();
        }

        isReverseNext = !isReverseNext;
    }

    /// <summary>
    /// Z축 기준 정방향 360도 회전
    /// </summary>
    public void RotateForward360()
    {
        StartRotate(1f);
    }

    /// <summary>
    /// Z축 기준 역방향 360도 회전
    /// </summary>
    public void RotateReverse360()
    {
        StartRotate(-1f);
    }

    private void StartRotate(float direction)
    {
        if (isRotating)
            return;

        rotateCoroutine = StartCoroutine(RotateZ360Coroutine(direction));
    }

    private IEnumerator RotateZ360Coroutine(float direction)
    {
        isRotating = true;

        Vector3 startEuler = transform.localEulerAngles;
        float startZ = startEuler.z;

        float elapsed = 0f;
        float targetAngle = rotateAngle * direction;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / rotateDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float currentZ = startZ + targetAngle * smoothT;

            transform.localEulerAngles = new Vector3(
                startEuler.x,
                startEuler.y,
                currentZ
            );

            yield return null;
        }

        // 마지막에 정확히 360도 위치로 보정
        transform.localEulerAngles = new Vector3(
            startEuler.x,
            startEuler.y,
            startZ + targetAngle
        );

        isRotating = false;
        rotateCoroutine = null;

        // 기본값 false라서 이벤트는 작동안함
        if (useCompleteEvent)
        {
            onRotateComplete?.Invoke();
        }
    }

    /// <summary>
    /// 다른 매니저 이벤트 실행 여부 토글
    /// </summary>
    public void ToggleCompleteEvent()
    {
        useCompleteEvent = !useCompleteEvent;
    }

    /// <summary>
    /// 외부에서 이벤트 실행 여부 직접 설정할 때 사용
    /// </summary>
    public void SetCompleteEvent(bool value)
    {
        useCompleteEvent = value;
    }
}