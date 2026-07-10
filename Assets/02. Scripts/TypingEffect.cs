using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    private Coroutine typingCoroutine;
    private Action pendingOnComplete;

    private bool isTyping;
    public bool IsTyping => isTyping;

    public float charsPerSecond = 15f;

    private TMP_Text currentText;
    private string currentFullText;
    private bool typingFinished;

    private void Awake()
    {
        typingCoroutine = null;
        pendingOnComplete = null;
        isTyping = false;

        currentText = null;
        currentFullText = string.Empty;
        typingFinished = false;
    }

    public void Apply(
        TMP_Text tmp,
        string text,
        float clipLength,
        Action onStart = null,
        Action onComplete = null)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentText = tmp;
        currentFullText = text;
        typingFinished = false;

        pendingOnComplete = onComplete;

        typingCoroutine = StartCoroutine(
            TypingRoutine(tmp, text, clipLength, onStart)
        );
    }

    private IEnumerator TypingRoutine(
        TMP_Text tmp,
        string text,
        float clipLength,
        Action onStart)
    {
        tmp.enabled = false;

        tmp.text = "";
        tmp.enabled = true;

        onStart?.Invoke();

        isTyping = true;

        float secPerChar =
            charsPerSecond <= 0 ? 0f : 1f / charsPerSecond;


        for (int i = 0; i < text.Length; i++)
        {
            tmp.text = text.Substring(0, i + 1);

            if (secPerChar > 0)
                yield return new WaitForSeconds(secPerChar);
            else
                yield return null;
        }


        isTyping = false;
        typingFinished = true;

        float typingDuration = text.Length * secPerChar;
        float delay = Mathf.Max(1.5f, clipLength - typingDuration);

        if (delay > 0)
            yield return new WaitForSeconds(delay);

        typingCoroutine = null;

        //var callback = pendingOnComplete;
        //pendingOnComplete = null;

        //callback?.Invoke();
    }

    public void Complete()
    {
        // 첫 번째 입력: 글자만 완성
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            currentText.text = currentFullText;

            isTyping = false;
            typingFinished = true;

            return;
        }

        // 두 번째 입력: 다음 진행
        if (typingFinished)
        {
            typingFinished = false;

            var callback = pendingOnComplete;
            pendingOnComplete = null;

            callback?.Invoke();
        }
    }
}