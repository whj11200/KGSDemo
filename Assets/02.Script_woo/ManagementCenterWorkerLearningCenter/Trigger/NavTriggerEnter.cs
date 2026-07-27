using System.Collections;
using UnityEngine;

public class NavTriggerEnter : MonoBehaviour
{
    [SerializeField] private GameObject arrow;

    [Header("Arrow Move")]
    [SerializeField] private float moveDistance = 0.3f; // 내려갈 거리
    [SerializeField] private float moveSpeed = 1f;      // 이동 속도

    private Coroutine moveCoroutine;
    private Vector3 originLocalPosition;

    private void OnEnable()
    {
        //arrow = GetComponentInChildren<GameObject>();
        if (arrow == null)
        {
            Debug.LogWarning("arrow가 연결되지 않았습니다.");
            return;
        }

        originLocalPosition = arrow.transform.localPosition;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(ArrowMoveLoop());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HideArrow();
        }
    }

    private IEnumerator ArrowMoveLoop()
    {
        Vector3 downPosition = originLocalPosition + Vector3.back * moveDistance;

        while (true)
        {
            yield return MoveArrow(originLocalPosition, downPosition);
            yield return MoveArrow(downPosition, originLocalPosition);
        }
    }

    private IEnumerator MoveArrow(Vector3 start, Vector3 end)
    {
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;

            arrow.transform.localPosition = Vector3.Lerp(start, end, time);

            yield return null;
        }

        arrow.transform.localPosition = end;
    }

    public void HideArrow()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;


            if (arrow != null)
                arrow.transform.localPosition = originLocalPosition;

            this.gameObject.SetActive(false);
        }
    }
}