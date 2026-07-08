using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public static class TypingEffect
{
    private static Tween currentTween;
    private static Tween delayedCall;
    private static Action pendingOnComplete;

    public static void Apply(TMP_Text tmp, string text, float typingDuration, float clipLength, Action onStart = null, Action onComplete = null)
    {
        tmp.enabled = false;

        currentTween?.Kill();
        delayedCall?.Kill();

        tmp.text = "";
        tmp.enabled = true;

        onStart?.Invoke();

        float totalCompleteTime = Mathf.Max(typingDuration, clipLength);
        float delay = Mathf.Max(0f, totalCompleteTime - typingDuration + 0.5f);

        // onComplete를 저장
        pendingOnComplete = onComplete;

        currentTween = tmp.DOText(text, typingDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                currentTween = null;
                delayedCall = DOVirtual.DelayedCall(delay, () =>
                {
                    delayedCall = null;
                    pendingOnComplete?.Invoke();
                    pendingOnComplete = null;
                });
            });
    }

    public static void Complete()
    {
        if (currentTween != null && currentTween.IsActive() && currentTween.IsPlaying())
        {
            currentTween.Complete(); // 즉시 텍스트 완료
        }

        if (delayedCall != null && delayedCall.IsActive())
        {
            delayedCall.Kill();
            delayedCall = null;
        }

        // 예약 없이 바로 실행 (중복 방지 위해 null 체크)
        if (pendingOnComplete != null)
        {
            pendingOnComplete.Invoke();
            pendingOnComplete = null;
        }
    }

    public static bool IsTyping =>
        currentTween != null && currentTween.IsActive() && currentTween.IsPlaying();
}
