using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VavleHandle : MonoBehaviour, IMouseInteractable
{
    [Header("Object Position")]
    [SerializeField] private Vector3 currentPosition;

    [Header("Rotation Setting")]
    [SerializeField] private float rotateAngle = -360f;
    [SerializeField] private float rotateDuration = 1f;

    [Header("State")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private bool isLocked = false;

    [Header("Lock / Unlock Events")]
    [SerializeField] private UnityEvent onLockComplete;
    [SerializeField] private UnityEvent onUnlockComplete;

    private Coroutine rotateCoroutine;

    public Vector3 CurrentPosition => currentPosition;
    public bool IsLocked => isLocked;
    public bool IsRotating => isRotating;

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
        ToggleLock();
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
    /// 현재 상태에 따라 잠금 / 해제 토글
    /// </summary>
    public void ToggleLock()
    {
        if (isRotating)
            return;

        if (isLocked)
        {
            UnlockValve();
        }
        else
        {
            LockValve();
        }
    }

    /// <summary>
    /// 밸브 잠금
    /// </summary>
    public void LockValve()
    {
        if (isRotating)
            return;

        rotateCoroutine = StartCoroutine(RotateZ360Coroutine(1f, true));
    }

    /// <summary>
    /// 밸브 해제
    /// </summary>
    public void UnlockValve()
    {
        if (isRotating)
            return;

        rotateCoroutine = StartCoroutine(RotateZ360Coroutine(-1f, false));
    }

    private IEnumerator RotateZ360Coroutine(float direction, bool lockAfterRotate)
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

        transform.localEulerAngles = new Vector3(
            startEuler.x,
            startEuler.y,
            startZ + targetAngle
        );

        isLocked = lockAfterRotate;
        isRotating = false;
        rotateCoroutine = null;

        if (isLocked)
        {
            onLockComplete?.Invoke();
        }
        else
        {
            onUnlockComplete?.Invoke();
        }
    }

    /// <summary>
    /// 외부에서 강제로 잠금 상태 세팅할 때 사용
    /// 회전은 하지 않고 상태값만 바꿈
    /// </summary>
    public void SetLockedState(bool locked)
    {
        if (isRotating)
            return;

        isLocked = locked;
    }
}