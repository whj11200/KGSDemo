using System.Collections;
using UnityEngine;

public class PipeInterestion : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Coroutine rippleEffectCoroutine;
    private Coroutine fillUpdateCoroutine;

    // 쉐이더 프로퍼티 이름
    private readonly string fillProp = "_Fill";
    private readonly string rippleColorProp = "_RippleColor";

    public bool isActive = false; // 통합 상태 관리 플래그
    private Color originalRippleColor;

    // 설정 값 (필요에 따라 인스펙터에서 수정 가능하도록 시리얼라이즈 가능)
    private readonly float minFill = 0.3f;
    private readonly float maxFill = 0.627f;
    private readonly float fillDuration = 2.0f;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // 초기 값 저장 및 설정
        if (meshRenderer.material.HasProperty(fillProp))
            meshRenderer.material.SetFloat(fillProp, minFill);

        if (meshRenderer.material.HasProperty(rippleColorProp))
            originalRippleColor = meshRenderer.material.GetColor(rippleColorProp);
    }

    /// <summary>
    /// 외부에서 이 함수 하나만 호출하면 켜고 끄기가 토글됩니다.
    /// </summary>
    public void TogglePipeState()
    {
        isActive = !isActive;

        if (isActive)
        {
            Debug.Log("파이프 시뮬레이션 활성화 (Fill Up + Ripple Start)");
            // 1. Fill 증가 (현재 값에서 maxFill까지)
            StartFillUpdate(maxFill);
            // 2. 리플 효과 시작
            StartRipple();
        }
        else
        {
            Debug.Log("파이프 시뮬레이션 비활성화 (Fill Down + Ripple Stop)");
            // 1. Fill 감소 (현재 값에서 minFill까지)
            StartFillUpdate(minFill);
            // 2. 리플 효과 정지 및 색상 복구
            StopRipple();
        }
    }

    // --- Fill 제어 로직 ---
    public void StartFillUpdate(float targetValue)
    {
        if (fillUpdateCoroutine != null) StopCoroutine(fillUpdateCoroutine);
        float currentVal = meshRenderer.material.GetFloat(fillProp);
        fillUpdateCoroutine = StartCoroutine(CoFillUpdate(currentVal, targetValue, fillDuration));
    }

    private IEnumerator CoFillUpdate(float start, float end, float duration)
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
        fillUpdateCoroutine = null;
    }

    // --- 리플 제어 로직 ---
    private void StartRipple()
    {
        if (rippleEffectCoroutine != null) StopCoroutine(rippleEffectCoroutine);
        rippleEffectCoroutine = StartCoroutine(CoRippleColorFlash());
    }

    public void StopRipple()
    {
        if (rippleEffectCoroutine != null) StopCoroutine(rippleEffectCoroutine);
        meshRenderer.material.SetColor(rippleColorProp, originalRippleColor);
        rippleEffectCoroutine = null;
    }

    private IEnumerator CoRippleColorFlash()
    {
        float fadeDuration = 1.0f;
        float stayDuration = 1.5f;
        Color targetColor = Color.orange * 50f;

        while (true)
        {
            // 밝아지기
            yield return StartCoroutine(CoLerpColor(originalRippleColor, targetColor, fadeDuration));
            yield return new WaitForSeconds(stayDuration);
            // 어두워지기
            yield return StartCoroutine(CoLerpColor(targetColor, originalRippleColor, fadeDuration));
            yield return new WaitForSeconds(stayDuration);
        }
    }

    private IEnumerator CoLerpColor(Color start, Color end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            meshRenderer.material.SetColor(rippleColorProp, Color.Lerp(start, end, elapsed / duration));
            yield return null;
        }
        meshRenderer.material.SetColor(rippleColorProp, end);
    }
}