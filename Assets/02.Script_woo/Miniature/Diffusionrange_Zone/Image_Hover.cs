using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Image_Hover : MonoBehaviour,
    IMouseInteractable,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI Image Setting")]
    [SerializeField] private Image targetImage;
    [SerializeField]
    private Color hoverImageColor =
        new Color(1f, 0.9f, 0.35f, 1f);

    private Color originImageColor;

    [Header("Target Renderer Material")]
    [SerializeField] private MeshRenderer targetRenderer;

    [Header("Alpha Setting")]
    [SerializeField] private string globalAlphaProperty = "_Alpha";
    [SerializeField] private float defaultAlpha = 0.5f;
    [SerializeField] private float hoverAlpha = 1f;
    [SerializeField] private float blinkDuration = 1f;

    private Material my_M;
    private Coroutine blinkCoroutine;

    // 왼손/오른손/마우스처럼 여러 포인터가 동시에 들어오는 경우 대응
    private readonly HashSet<int> activePointerIds = new();

    // 기존 IMouseInteractable 시스템에서 들어오는 호버 상태
    private bool legacyMouseHover;

    // 실제 호버 효과가 실행 중인지 여부
    private bool isHoverEffectActive;

    private void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null)
        {
            originImageColor = targetImage.color;
        }

        if (targetRenderer != null)
        {
            // 이 Renderer 전용 Material 인스턴스 생성
            my_M = targetRenderer.material;
            SetGlobalAlpha(defaultAlpha);
        }
        else
        {
            Debug.LogWarning(
                $"{name} : targetRenderer가 비어 있습니다. " +
                "알파를 조절할 3D 오브젝트의 MeshRenderer를 넣어주세요.");
        }
    }

    #region Unity UI Pointer Event

    // PC 마우스 또는 VR 레이가 Image에 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        activePointerIds.Add(eventData.pointerId);
        RefreshHoverState();
    }

    // PC 마우스 또는 VR 레이가 Image에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        activePointerIds.Remove(eventData.pointerId);
        RefreshHoverState();
    }

    #endregion

    #region IMouseInteractable

    public void ClickCancle()
    {
    }

    public void ClickEnter()
    {
    }

    public void ClickExit()
    {
    }

    // 기존 PC용 커스텀 마우스 시스템에서도 사용 가능
    public void HoverEnter()
    {
        legacyMouseHover = true;
        RefreshHoverState();
    }

    public void HoverExit()
    {
        legacyMouseHover = false;
        RefreshHoverState();
    }

    #endregion

    private void RefreshHoverState()
    {
        bool shouldHover =
            legacyMouseHover ||
            activePointerIds.Count > 0;

        if (shouldHover == isHoverEffectActive)
            return;

        isHoverEffectActive = shouldHover;

        if (isHoverEffectActive)
        {
            StartHoverEffect();
        }
        else
        {
            StopHoverEffect();
        }
    }

    private void StartHoverEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(BlinkAlphaAndImage());
    }

    private void StopHoverEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        SetGlobalAlpha(defaultAlpha);

        if (targetImage != null)
        {
            targetImage.color = originImageColor;
        }
    }

    private IEnumerator BlinkAlphaAndImage()
    {
        float duration = Mathf.Max(0.01f, blinkDuration);

        while (true)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);

                SetGlobalAlpha(
                    Mathf.Lerp(defaultAlpha, hoverAlpha, t));

                if (targetImage != null)
                {
                    targetImage.color = Color.Lerp(
                        originImageColor,
                        hoverImageColor,
                        t);
                }

                yield return null;
            }

            SetGlobalAlpha(hoverAlpha);

            if (targetImage != null)
            {
                targetImage.color = hoverImageColor;
            }

            timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);

                SetGlobalAlpha(
                    Mathf.Lerp(hoverAlpha, defaultAlpha, t));

                if (targetImage != null)
                {
                    targetImage.color = Color.Lerp(
                        hoverImageColor,
                        originImageColor,
                        t);
                }

                yield return null;
            }

            SetGlobalAlpha(defaultAlpha);

            if (targetImage != null)
            {
                targetImage.color = originImageColor;
            }
        }
    }

    private void SetGlobalAlpha(float alpha)
    {
        if (my_M == null)
            return;

        if (my_M.HasProperty(globalAlphaProperty))
        {
            my_M.SetFloat(globalAlphaProperty, alpha);
        }
        else
        {
            Debug.LogWarning(
                $"{name} : Material에 {globalAlphaProperty} 프로퍼티가 없습니다. " +
                "Shader의 실제 프로퍼티명을 확인하세요.");
        }
    }

    private void OnDisable()
    {
        activePointerIds.Clear();
        legacyMouseHover = false;
        isHoverEffectActive = false;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        SetGlobalAlpha(defaultAlpha);

        if (targetImage != null)
        {
            targetImage.color = originImageColor;
        }
    }

    private void OnDestroy()
    {
        // targetRenderer.material로 만들어진 인스턴스 정리
        if (my_M != null)
        {
            Destroy(my_M);
        }
    }
}