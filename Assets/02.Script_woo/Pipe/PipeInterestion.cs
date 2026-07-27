using System.Collections;
using UnityEngine;

public class PipeInterestion : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private MeshRenderer meshRenderer;

    private Material material;

    private Coroutine rippleEffectCoroutine;
    private Coroutine fillUpdateCoroutine;
    private Coroutine materialColorCoroutine;

    [Header("Shader Properties")]
    [SerializeField] private string fillProp = "_Fill";
    [SerializeField] private string rippleColorProp = "_RippleColor";
    [SerializeField] private string materialColorProp = "_BaseColor";

    [Header("Pipe State")]
    [SerializeField] private bool isActive = false;

    [Header("Fill Settings")]
    [SerializeField] private float minFill = 0.3f;
    [SerializeField] private float maxFill = 0.627f;
    [SerializeField] private float fillDuration = 2f;

    [Header("Material Color Settings")]
    [SerializeField] private Color targetMaterialColor = Color.yellow;
    [SerializeField] private float materialColorDuration = 2f;
    [SerializeField] private bool isYellowColor = false;

    private Color originalRippleColor;
    private Color originalMaterialColor;
    [Header("Material Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;

    private Coroutine materialBlinkCoroutine;
    [SerializeField] private bool isMaterialBlinking = false;
    private void Awake()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            Debug.LogError($"{name}: MeshRenderer가 없습니다.");
            enabled = false;
            return;
        }

        // 해당 오브젝트만 사용하는 머티리얼 인스턴스
        material = meshRenderer.material;

        if (material.HasProperty(fillProp))
            material.SetFloat(fillProp, minFill);

        if (material.HasProperty(rippleColorProp))
            originalRippleColor = material.GetColor(rippleColorProp);

        // URP Lit은 일반적으로 _BaseColor 사용
        // Built-in Standard Shader는 _Color 사용
        if (!material.HasProperty(materialColorProp))
        {
            if (material.HasProperty("_BaseColor"))
                materialColorProp = "_BaseColor";
            else if (material.HasProperty("_Color"))
                materialColorProp = "_Color";
        }

        if (material.HasProperty(materialColorProp))
        {
            originalMaterialColor = material.GetColor(materialColorProp);
        }
        else
        {
            Debug.LogWarning(
                $"{name}: 메테리얼에 {materialColorProp}, _BaseColor, _Color 프로퍼티가 없습니다."
            );
        }
    }

    /// <summary>
    /// 파이프 Fill과 Ripple 효과를 토글합니다.
    /// </summary>
    public void TogglePipeState()
    {
        isActive = !isActive;

        if (isActive)
        {
            Debug.Log("파이프 시뮬레이션 활성화");

            StartFillUpdate(maxFill);
            StartRipple();
        }
        else
        {
            Debug.Log("파이프 시뮬레이션 비활성화");

            StartFillUpdate(minFill);
            StopRipple();
        }
    }

    // =========================================================
    // Material Color
    // =========================================================

    /// <summary>
    /// 호출할 때마다 원래 색상과 노란색을 번갈아 전환합니다.
    /// </summary>
    public void ToggleMaterialColor()
    {
        if (material == null || !material.HasProperty(materialColorProp))
            return;

        // 깜박이는 중이라면 먼저 정지
        if (isMaterialBlinking)
            StopMaterialBlink();

        isYellowColor = !isYellowColor;

        Color targetColor = isYellowColor
            ? targetMaterialColor
            : originalMaterialColor;

        StartMaterialColorUpdate(targetColor);
    }

    /// <summary>
    /// 메테리얼을 노란색으로 변경합니다.
    /// </summary>
    public void SetYellowColor()
    {
        if (material == null || !material.HasProperty(materialColorProp))
            return;

        isYellowColor = true;
        StartMaterialColorUpdate(targetMaterialColor);
    }

    /// <summary>
    /// 메테리얼을 처음 색상으로 복구합니다.
    /// </summary>
    public void RestoreOriginalColor()
    {
        if (material == null || !material.HasProperty(materialColorProp))
            return;

        isYellowColor = false;
        StartMaterialColorUpdate(originalMaterialColor);
    }

    private void StartMaterialColorUpdate(Color targetColor)
    {
        if (materialColorCoroutine != null)
            StopCoroutine(materialColorCoroutine);

        Color currentColor = material.GetColor(materialColorProp);

        materialColorCoroutine = StartCoroutine(
            CoMaterialColorUpdate(
                currentColor,
                targetColor,
                materialColorDuration
            )
        );
    }

    private IEnumerator CoMaterialColorUpdate(
        Color startColor,
        Color endColor,
        float duration
    )
    {
        if (duration <= 0f)
        {
            material.SetColor(materialColorProp, endColor);
            materialColorCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsed / duration);
            Color currentColor = Color.Lerp(startColor, endColor, ratio);

            material.SetColor(materialColorProp, currentColor);

            yield return null;
        }

        material.SetColor(materialColorProp, endColor);
        materialColorCoroutine = null;
    }

    // =========================================================
    // Fill
    // =========================================================

    public void StartFillUpdate(float targetValue)
    {
        if (material == null || !material.HasProperty(fillProp))
            return;

        if (fillUpdateCoroutine != null)
            StopCoroutine(fillUpdateCoroutine);

        float currentValue = material.GetFloat(fillProp);

        fillUpdateCoroutine = StartCoroutine(
            CoFillUpdate(
                currentValue,
                targetValue,
                fillDuration
            )
        );
    }

    private IEnumerator CoFillUpdate(
        float start,
        float end,
        float duration
    )
    {
        if (duration <= 0f)
        {
            material.SetFloat(fillProp, end);
            fillUpdateCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsed / duration);
            float currentFill = Mathf.Lerp(start, end, ratio);

            material.SetFloat(fillProp, currentFill);

            yield return null;
        }

        material.SetFloat(fillProp, end);
        fillUpdateCoroutine = null;
    }

    // =========================================================
    // Ripple
    // =========================================================

    private void StartRipple()
    {
        if (material == null || !material.HasProperty(rippleColorProp))
            return;

        if (rippleEffectCoroutine != null)
            StopCoroutine(rippleEffectCoroutine);

        rippleEffectCoroutine = StartCoroutine(CoRippleColorFlash());
    }

    public void StopRipple()
    {
        if (rippleEffectCoroutine != null)
        {
            StopCoroutine(rippleEffectCoroutine);
            rippleEffectCoroutine = null;
        }

        if (material != null && material.HasProperty(rippleColorProp))
            material.SetColor(rippleColorProp, originalRippleColor);
    }

    private IEnumerator CoRippleColorFlash()
    {
        float fadeDuration = 1f;
        float stayDuration = 1.5f;

        Color targetColor = Color.orange * 50f;

        while (true)
        {
            yield return CoLerpRippleColor(
                originalRippleColor,
                targetColor,
                fadeDuration
            );

            yield return new WaitForSeconds(stayDuration);

            yield return CoLerpRippleColor(
                targetColor,
                originalRippleColor,
                fadeDuration
            );

            yield return new WaitForSeconds(stayDuration);
        }
    }

    private IEnumerator CoLerpRippleColor(
        Color start,
        Color end,
        float duration
    )
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float ratio = Mathf.Clamp01(elapsed / duration);
            material.SetColor(
                rippleColorProp,
                Color.Lerp(start, end, ratio)
            );

            yield return null;
        }

        material.SetColor(rippleColorProp, end);
    }

    private void OnDisable()
    {
        if (materialColorCoroutine != null)
            StopCoroutine(materialColorCoroutine);

        if (materialBlinkCoroutine != null)
            StopCoroutine(materialBlinkCoroutine);

        if (fillUpdateCoroutine != null)
            StopCoroutine(fillUpdateCoroutine);

        if (rippleEffectCoroutine != null)
            StopCoroutine(rippleEffectCoroutine);

        isMaterialBlinking = false;

        if (material != null && material.HasProperty(materialColorProp))
            material.SetColor(materialColorProp, originalMaterialColor);
    }
    public void ToggleMaterialBlink()
    {
        if (isMaterialBlinking)
        {
            StopMaterialBlink();
        }
        else
        {
            StartMaterialBlink();
        }
    }

    /// <summary>
    /// 노란색과 원래 색상 깜박임을 시작합니다.
    /// </summary>
    public void StartMaterialBlink()
    {
        if (material == null || !material.HasProperty(materialColorProp))
            return;

        // 일반 색상 변경 코루틴과 충돌 방지
        if (materialColorCoroutine != null)
        {
            StopCoroutine(materialColorCoroutine);
            materialColorCoroutine = null;
        }

        if (materialBlinkCoroutine != null)
            StopCoroutine(materialBlinkCoroutine);

        isMaterialBlinking = true;
        materialBlinkCoroutine = StartCoroutine(CoMaterialBlink());
    }

    /// <summary>
    /// 깜박임을 정지하고 원래 색상으로 복구합니다.
    /// </summary>
    public void StopMaterialBlink()
    {
        if (materialBlinkCoroutine != null)
        {
            StopCoroutine(materialBlinkCoroutine);
            materialBlinkCoroutine = null;
        }

        isMaterialBlinking = false;
        isYellowColor = false;

        if (material != null && material.HasProperty(materialColorProp))
            material.SetColor(materialColorProp, originalMaterialColor);
    }

    private IEnumerator CoMaterialBlink()
    {
        while (isMaterialBlinking)
        {
            // 노란색
            material.SetColor(materialColorProp, targetMaterialColor);
            isYellowColor = true;

            yield return new WaitForSeconds(blinkInterval);

            // 원래 색상
            material.SetColor(materialColorProp, originalMaterialColor);
            isYellowColor = false;

            yield return new WaitForSeconds(blinkInterval);
        }

        material.SetColor(materialColorProp, originalMaterialColor);
        materialBlinkCoroutine = null;
    }
}