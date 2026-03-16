using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MessageUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image backgroundImage;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float displayTime = 2.0f;
    [SerializeField] private float fadeOutTime = 0.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        // 시작 시 오브젝트 자체를 꺼버림
        gameObject.SetActive(false);
    }

    // 외부에서 호출: messageUI.ShowMessage("장비를 해제하세요");
    public void ShowMessage(string content)
    {
        gameObject.SetActive(true);
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(CoDisplaySequence(content));
    }

    private IEnumerator CoDisplaySequence(string content)
    {
        // 1. 시작: 텍스트 설정 및 오브젝트 활성화
        messageText.text = content;
        SetUIAlpha(0f); // 나타나기 전 투명하게
       

        // 2. Fade In
        float elapsedTime = 0f;
        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.5f, elapsedTime / fadeInTime);
            SetUIAlpha(alpha);
            yield return null;
        }
        SetUIAlpha(1f);

        // 3. Wait (지정된 시간 동안 노출)
        yield return new WaitForSeconds(displayTime);

        // 4. Fade Out
        elapsedTime = 0f;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
            SetUIAlpha(alpha);
            yield return null;
        }

        // 5. 종료: 다시 투명하게 만들고 오브젝트 끄기
        SetUIAlpha(0f);
        gameObject.SetActive(false);
        currentRoutine = null;
    }

    // 텍스트와 이미지의 알파값을 직접 수정하는 함수
    private void SetUIAlpha(float alpha)
    {
        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = alpha*1;
            messageText.color = c;
        }

        if (backgroundImage != null)
        {
            Color c = backgroundImage.color;
            c.a = alpha*0.5f;
            backgroundImage.color = c;
        }
    }
}