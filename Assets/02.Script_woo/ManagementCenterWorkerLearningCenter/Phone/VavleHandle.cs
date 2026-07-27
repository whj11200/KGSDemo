using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VavleHandle : MonoBehaviour, IMouseInteractable
{
    [Header("사운드")]
    [SerializeField] private AudioSource audioSource;
    [Header("Object Position")]
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private NavTriggerEnter Nav;

    [Header("Rotation Setting")]
    [SerializeField] private float rotateAngle = -360f;
    [SerializeField] private float rotateDuration = 1f;

    [Header("State")]
    [SerializeField] private bool isRotating = false;
    [SerializeField] private bool isLocked = false;

    [Header("Manager Count Register")]
    [SerializeField] private ValveQuestManager enviromentManager;
    [SerializeField] private bool reportToManager = true;

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

    public void LockValve()
    {
        if (isRotating)
            return;

        rotateCoroutine = StartCoroutine(RotateZ360Coroutine(1f, true));
        audioSource.Play();
    }

    public void UnlockValve()
    {
        if (isRotating)
            return;

        rotateCoroutine = StartCoroutine(RotateZ360Coroutine(-1f, false));
        audioSource.Play();
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
            NotifyManagerValveClosed();
            onLockComplete?.Invoke();
        }
        else
        {
            NotifyManagerValveOpened();
            onUnlockComplete?.Invoke();
        }

        Nav?.HideArrow();
    }

    private void NotifyManagerValveClosed()
    {
        if (!reportToManager)
            return;

        if (enviromentManager == null)
        {
            Debug.LogWarning($"{name}: Manager_EnviromentManager가 연결되지 않았습니다.");
            return;
        }

        enviromentManager.RegisterValveClosed(this);
    }

    private void NotifyManagerValveOpened()
    {
        if (!reportToManager)
            return;

        if (enviromentManager == null)
            return;

        enviromentManager.RegisterValveOpened(this);
    }

    public void SetLockedState(bool locked)
    {
        if (isRotating)
            return;

        isLocked = locked;
    }
}