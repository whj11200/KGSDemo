using System.Collections;
using UnityEngine;

public class PipeInterestion : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Coroutine rippleEffectCoroutine;
    private Coroutine fillUpdateCoroutine;

    // 쉐이더 프로퍼티 이름 (Reference 확인 필수)
    private readonly string fillProp = "_Fill";
    private readonly string rippleColorProp = "_RippleColor";

    private bool isRippleActive = false;
    private Color originalRippleColor; // 원래 색상 저장용

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // 시작 시 Fill 값 초기화
        if (meshRenderer.material.HasProperty(fillProp))
        {
            meshRenderer.material.SetFloat(fillProp, 0.3f);
        }

        // 초기 RippleColor 저장
        if (meshRenderer.material.HasProperty(rippleColorProp))
        {
            originalRippleColor = meshRenderer.material.GetColor(rippleColorProp);
        }
    }

    /// <summary>
    /// Fill 값을 0.38에서 0.627까지 천천히 올리는 함수
    /// </summary>
    public void StartFillUp()
    {
        if (fillUpdateCoroutine != null) StopCoroutine(fillUpdateCoroutine);
        fillUpdateCoroutine = StartCoroutine(CoFillUp(0.38f, 0.627f, 2.0f));
    }

    private IEnumerator CoFillUp(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentFill = Mathf.Lerp(start, end, elapsed / duration);
            meshRenderer.material.SetFloat(fillProp, currentFill);
            yield return null;
        }
        meshRenderer.material.SetFloat(fillProp, end);
    }

    /// <summary>
    /// 리플 색상(오리지널 <-> 노란색)을 토글하는 함수
    /// </summary>
    public void ToggleRippleColor()
    {
        isRippleActive = !isRippleActive;

        if (isRippleActive)
        {
            if (rippleEffectCoroutine != null) StopCoroutine(rippleEffectCoroutine);
            rippleEffectCoroutine = StartCoroutine(CoRippleColorFlash());
        }
        else
        {
            if (rippleEffectCoroutine != null) StopCoroutine(rippleEffectCoroutine);
            // 원래 색상으로 즉시 복구
            meshRenderer.material.SetColor(rippleColorProp, originalRippleColor);
        }
    }

    private IEnumerator CoRippleColorFlash()
    {
        float fadeDuration = 1.0f;  // 색이 변하는 시간 (서서히)
        float stayDuration = 1.5f;  // 변한 상태로 유지되는 시간 (텀)

        // 진한 노란색 (HDR 강도 4배)
        Color targetColor = Color.orange * 50f;

        while (true)
        {
            // 1. 오리지널 -> 노란색 (서서히 변화)
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                meshRenderer.material.SetColor(rippleColorProp, Color.Lerp(originalRippleColor, targetColor, elapsed / fadeDuration));
                yield return null;
            }
            meshRenderer.material.SetColor(rippleColorProp, targetColor);

            // 2. 노란색 상태에서 잠시 대기 (원하시는 '텀')
            yield return new WaitForSeconds(stayDuration);

            // 3. 노란색 -> 오리지널 (서서히 복구)
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                meshRenderer.material.SetColor(rippleColorProp, Color.Lerp(targetColor, originalRippleColor, elapsed / fadeDuration));
                yield return null;
            }
            meshRenderer.material.SetColor(rippleColorProp, originalRippleColor);

            // 4. 오리지널 상태에서 잠시 대기 (다시 노란색으로 가기 전 텀)
            yield return new WaitForSeconds(stayDuration);
        }
    }
    public void ResetPipe()
    {
        // 모든 코루틴 정지
        if (fillUpdateCoroutine != null) StopCoroutine(fillUpdateCoroutine);
        if (rippleEffectCoroutine != null) StopCoroutine(rippleEffectCoroutine);

        isRippleActive = false;

        // 쉐이더 값 초기화
        meshRenderer.material.SetFloat(fillProp, 0.3f);
        meshRenderer.material.SetColor(rippleColorProp, originalRippleColor);

        Debug.Log("파이프 상태가 초기화되었습니다.");
    }
}