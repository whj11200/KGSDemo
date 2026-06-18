using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Image_Hover : MonoBehaviour, IMouseInteractable
{
    [Header("UI Image Setting")]
    [SerializeField] private Image targetImage;
    [SerializeField] private Color hoverImageColor = new Color(1f, 0.9f, 0.35f, 1f);

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
            // targetRenderer가 가진 머티리얼을 이 오브젝트 전용 인스턴스로 가져옴
            my_M = targetRenderer.material;
            SetGlobalAlpha(defaultAlpha);
        }
        else
        {
            Debug.LogWarning($"{name} : targetRenderer가 비어 있습니다. 알파를 조절할 3D 오브젝트의 MeshRenderer를 넣어주세요.");
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