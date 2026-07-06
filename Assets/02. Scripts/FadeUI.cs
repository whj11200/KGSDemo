using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI FadeText;
    [SerializeField] private Image FadeImage;

    private Sequence sequence;

    public void SetText(string text)
    {
        FadeText.text = text;
    }

    public void HideText()
    {
        FadeText.text = "";
    }

    public void FadeInOut(float inDur,
                          float outDur,
                          float delay,
                          Action OnInStart = null,
                          Action OnInEnd = null,
                          Action OnOutStart = null,
                          Action OnOutEnd = null)
    {
        sequence?.Kill();

        Color color = FadeImage.color;
        color.a = 1f;
        FadeImage.color = color;

        sequence = DOTween.Sequence();

        // Fade In (1 -> 0)
        sequence.AppendCallback(() => OnInStart?.Invoke());
        sequence.Append(FadeImage.DOFade(0f, inDur));
        sequence.AppendCallback(() => OnInEnd?.Invoke());

        // 대기
        sequence.AppendInterval(delay);

        // Fade Out (0 -> 1)
        sequence.AppendCallback(() => OnOutStart?.Invoke());
        sequence.Append(FadeImage.DOFade(1f, outDur));
        sequence.AppendCallback(() => OnOutEnd?.Invoke());
    }

    public void FadeOutIn(float outDur,
                      float inDur,
                      float delay,
                      Action OnOutStart = null,
                      Action OnOutEnd = null,
                      Action OnInStart = null,
                      Action OnInEnd = null)
    {
        sequence?.Kill();

        Color color = FadeImage.color;
        color.a = 0f;
        FadeImage.color = color;

        sequence = DOTween.Sequence();

        // Fade Out (0 -> 1)
        sequence.AppendCallback(() => OnOutStart?.Invoke());
        sequence.Append(FadeImage.DOFade(1f, outDur));
        sequence.AppendCallback(() => OnOutEnd?.Invoke());

        // 대기
        sequence.AppendInterval(delay);

        // Fade In (1 -> 0)
        sequence.AppendCallback(() => OnInStart?.Invoke());
        sequence.Append(FadeImage.DOFade(0f, inDur));
        sequence.AppendCallback(() => OnInEnd?.Invoke());
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}