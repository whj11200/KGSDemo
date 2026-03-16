using UnityEngine;
using System.Collections;

public class PuddleController : MonoBehaviour
{
    public float growSpeed = 0.02f;
    public float shrinkSpeed = 0.5f;
    [SerializeField] Transform Puddle;
    private bool isShrinking = false; // 현재 사라지는 중인지 체크

    private Vector3 targetScale = new Vector3(3.0f, 1.25f, 0.1f);
    private Coroutine currentRoutine;

    // 자식(충돌체)으로부터 신호를 받으면 실행
    // 웅덩이 커지는 함수, 충돌이 계속되면 계속 커짐
    public void GrowPuddle()
    {
        if (isShrinking) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        // 커지는 건 즉각적인 반응을 위해 Update 느낌으로 살짝만 키움
        Vector3 s = Puddle.localScale;
        if (s.x < targetScale.x)
        {
            float nextX = Mathf.Min(s.x + growSpeed, targetScale.x);
            float nextY = Mathf.Min(s.y + (growSpeed * 0.416f), targetScale.y);
            Puddle.localScale = new Vector3(nextX, nextY, s.z);
        }
    }
    // 벨브를 잠궜을때 이벤트 
    public void StartShrinking()
    {
        isShrinking = true; // 이제부터는 충돌해도 안 커짐
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ShrinkRoutine());
    
    }

    IEnumerator ShrinkRoutine()
    {
        Debug.Log("웅덩이 수축 루틴 시작");
        while (Puddle.localScale.x > 0.01f)
        {
            Puddle.localScale = Vector3.MoveTowards(
                Puddle.localScale,
                new Vector3(0, 0, Puddle.localScale.z),
                shrinkSpeed * Time.deltaTime
            );
            yield return null;
        }
        Puddle.localScale = new Vector3(0, 0, Puddle.localScale.z);
        isShrinking = false;
    }

}