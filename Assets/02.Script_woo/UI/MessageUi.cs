using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject visualChild;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float displayTime = 2.0f;
    [SerializeField] private float fadeOutTime = 0.5f;

    private Coroutine currentRoutine;
    private bool isDisplaying = false; // 현재 메시지가 출력 중인지 확인

    private void Awake()
    {
        if (visualChild != null)
        {
            SetUIAlpha(0f);
            visualChild.SetActive(false);
        }
    }

    public void ShowMessage(string content)
    {
        // 핵심: 이미 메시지가 떠 있다면 새로운 요청은 무시함 (중복 방지)
        if (isDisplaying) return;

        if (!gameObject.activeInHierarchy) return;

        currentRoutine = StartCoroutine(CoDisplaySequence(content));
    }

    private IEnumerator CoDisplaySequence(string content)
    {
        isDisplaying = true; // 출력 시작
        messageText.text = content;
        visualChild.SetActive(true);

        // 1. Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
            SetUIAlpha(alpha);
            yield return null;
        }
        SetUIAlpha(1f);

        // 2. Wait
        yield return new WaitForSeconds(displayTime);

        // 3. Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
            SetUIAlpha(alpha);
            yield return null;
        }

        // 4. 마무리
        SetUIAlpha(0f);
        visualChild.SetActive(false);
        isDisplaying = false; // 출력 종료 (이제 다음 메시지 수신 가능)
        currentRoutine = null;
    }

    private void SetUIAlpha(float alpha)
    {
        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = alpha;
            messageText.color = c;
        }

        if (backgroundImage != null)
        {
            Color c = backgroundImage.color;
            c.a = alpha * 0.5f;
            backgroundImage.color = c;
        }
    }
}