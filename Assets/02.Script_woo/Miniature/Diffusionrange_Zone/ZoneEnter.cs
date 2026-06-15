using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ZoneEnter : MonoBehaviour, IMouseInteractable
{
    [Header("Material")]
    [SerializeField] private Material my_M;

    [Header("Alpha Setting")]
    [SerializeField] private string globalAlphaProperty = "_Alpha";
    [SerializeField] private float defaultAlpha = 0.5f;
    [SerializeField] private float hoverAlpha = 1f;
    [SerializeField] private float blinkDuration = 1f;

    [Header("UI Image Setting")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color hoverImageColor = new Color(1f, 0.9f, 0.35f, 1f); // 연한 노란색

    private Color originImageColor;

    private Coroutine blinkCoroutine;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            // renderer.material은 이 오브젝트 전용 Material Instance를 만듦
            my_M = meshRenderer.material;
            SetGlobalAlpha(defaultAlpha);
        }

        if (targetImage != null)
        {
            originImageColor = targetImage.color;
            targetImage.color = originImageColor;
        }
    }

    public void ClickCancle()
    {
    }

    public void ClickEnter()
    {
    }

    public void ClickExit()
    {
    }

    public void HoverEnter()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(BlinkAlphaAndImage());
    }

    public void HoverExit()
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
        while (true)
        {
            // 0.5 -> 1
            float timer = 0f;

            while (timer < blinkDuration)
            {
                timer += Time.deltaTime;

                float t = timer / blinkDuration;

                float alpha = Mathf.Lerp(defaultAlpha, hoverAlpha, t);
                SetGlobalAlpha(alpha);

                if (targetImage != null)
                {
                    targetImage.color = Color.Lerp(originImageColor, hoverImageColor, t);
                }

                yield return null;
            }

            SetGlobalAlpha(hoverAlpha);

            if (targetImage != null)
            {
                targetImage.color = hoverImageColor;
            }

            // 1 -> 0.5
            timer = 0f;

            while (timer < blinkDuration)
            {
                timer += Time.deltaTime;

                float t = timer / blinkDuration;

                float alpha = Mathf.Lerp(hoverAlpha, defaultAlpha, t);
                SetGlobalAlpha(alpha);

                if (targetImage != null)
                {
                    targetImage.color = Color.Lerp(hoverImageColor, originImageColor, t);
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
            Debug.LogWarning($"Material에 {globalAlphaProperty} 프로퍼티가 없습니다. Shader의 실제 변수명을 확인하세요.");
        }
    }
}