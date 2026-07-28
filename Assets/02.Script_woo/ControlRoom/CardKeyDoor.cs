using System.Collections;
using UnityEngine;

public class CardKeyDoor : SupportXRInteractable
{
    [Header("Door Objects")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Door Setting")]
    [SerializeField] private float openDistance = 1.2f;
    [SerializeField] private float moveDuration = 2.0f;
    [SerializeField] private float AutoCloseDelay = 5f;

    [Header("Sound")]
    [SerializeField] private AudioSource DoorSource;
    [SerializeField] private AudioSource KeySource;
    [SerializeField] private AudioClip OpenClip;
    [SerializeField] private AudioClip CloseClip;
    [SerializeField] private AudioClip KeyClip;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private Coroutine moveCoroutine;
    private bool isOpen;

    private void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogWarning("문 오브젝트가 할당되지 않았습니다.");
            return;
        }

        // 초기 위치 저장
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        // local X축 기준으로 문 열림 위치 계산
        leftOpenPos = leftClosedPos + Vector3.left * openDistance;
        rightOpenPos = rightClosedPos + Vector3.right * openDistance;
    }

    public override void ClickExit()
    {
        ToggleDoor();
    }

    public void OpenDoor()
    {
        if (leftDoor == null || rightDoor == null) return;
        if (isOpen) return;

        isOpen = true;

        PlaySound(DoorSource, OpenClip);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveDoor(leftOpenPos, rightOpenPos));

        if (CloseRoutine != null)
        {
            StopCoroutine(CloseRoutine);
            CloseRoutine = null;
        }

        CloseRoutine = StartCoroutine(AutoClose());
    }

    private Coroutine CloseRoutine;
    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(AutoCloseDelay);
        CloseDoor();
    }

    public void CloseDoor()
    {
        if (leftDoor == null || rightDoor == null) return;
        if (!isOpen) return;

        isOpen = false;

        PlaySound(DoorSource, CloseClip);

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        if (CloseRoutine != null)
        {
            StopCoroutine(CloseRoutine);
            CloseRoutine = null;
        }

        moveCoroutine = StartCoroutine(MoveDoor(leftClosedPos, rightClosedPos));
    }

    public void ToggleDoor()
    {
        PlaySound(KeySource, KeyClip);

        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    private IEnumerator MoveDoor(Vector3 targetLeftPos, Vector3 targetRightPos)
    {
        Vector3 startLeftPos = leftDoor.localPosition;
        Vector3 startRightPos = rightDoor.localPosition;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;

            // 부드러운 움직임
            t = Mathf.SmoothStep(0f, 1f, t);

            leftDoor.localPosition = Vector3.Lerp(startLeftPos, targetLeftPos, t);
            rightDoor.localPosition = Vector3.Lerp(startRightPos, targetRightPos, t);

            yield return null;
        }

        leftDoor.localPosition = targetLeftPos;
        rightDoor.localPosition = targetRightPos;

        moveCoroutine = null;
    }

    private void PlaySound(AudioSource source, AudioClip clip)
    {
        if (source == null  ||  clip == null)
            return;

        if (source.isPlaying)
            source.Stop();

        source.PlayOneShot(clip);
    }

    #region UnUse
    public override void ClickCancle()
    {

    }

    public override void ClickEnter()
    {

    }

    public override void HoverEnter()
    {

    }

    public override void HoverExit()
    {

    }
    #endregion
}