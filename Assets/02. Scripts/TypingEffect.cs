using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    private Tween currentTween;
    private Tween delayedCall;
    private Action pendingOnComplete;

    public void Apply(
        TMP_Text tmp,
        string text,
        float typingDuration,
        float clipLength,
        Action onStart = null,
        Action onComplete = null)
    {
        tmp.enabled = false;

        currentTween?.Kill();
        delayedCall?.Kill();

        tmp.text = "";
        tmp.enabled = true;

        onStart?.Invoke();

        float totalCompleteTime = Mathf.Max(typingDuration, clipLength);
        float delay = Mathf.Max(0f, totalCompleteTime - typingDuration + 0.5f);

        pendingOnComplete = onComplete;

        currentTween = tmp.DOText(text, typingDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                currentTween = null;

                delayedCall = DOVirtual.DelayedCall(delay, () =>
                {
                    delayedCall = null;

                    var callback = pendingOnComplete;
                    pendingOnComplete = null;

                    callback?.Invoke();
                });
            });
    }

    public void Complete()
    {
        if (currentTween != null && currentTween.IsActive() && currentTween.IsPlaying())
        {
            currentTween.Complete();
        }

        if (delayedCall != null && delayedCall.IsActive())
        {
            delayedCall.Kill();
            delayedCall = null;
        }

        var callback = pendingOnComplete;
        pendingOnComplete = null;

        callback?.Invoke();
    }

    public bool IsTyping =>
        currentTween != null &&
        currentTween.IsActive() &&
        currentTween.IsPlaying();
}