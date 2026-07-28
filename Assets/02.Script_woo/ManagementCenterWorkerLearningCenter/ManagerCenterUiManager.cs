using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ManagerCenterUiManager : MonoBehaviour
{
    [SerializeField] HazeControl hazeControl;
    [Header("UI Components")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private TextMeshProUGUI guideText;

    private string currentGuideMessage = "";
    private bool hasGuideMessage = false;

    [Header("Valeve Quest Text Slot")]
    [SerializeField] private List<TMP_Text> QuestText = new();
    private Dictionary<string, TMP_Text> ValveInfoText = new();

    private Coroutine temporaryGuideCoroutine;

    [Header("어두운 페이드 인 아웃")]
    [SerializeField] private Image backGroundimage;
    [SerializeField] private TextMeshProUGUI messageTextMeshPro;

    [Header("Fade Setting")]
    [SerializeField] private float fadeStartDelay = 3f;      // 몇 초 뒤 페이드 시작
    [SerializeField] private float fadeInDuration = 3f;      // 어두워지는 시간
    [SerializeField] private float textShowDelay = 2f;       // 페이드 시작 후 텍스트 뜨는 시간
    [SerializeField] private float darkHoldTime = 2f;        // 완전히 어두운 상태 유지 시간
    [SerializeField] private float fadeOutDuration = 3f;     // 다시 밝아지는 시간

    [Header("Fade Complete Event")]
    [SerializeField] private UnityEvent onFadeComplete;
    [Header("Background Dark Only Event")]
    [SerializeField] private UnityEvent endEvnet;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (guidePanel != null)
            guidePanel.SetActive(false);

        InitFadeUI();

        ValveInfoText.Clear();
        foreach (var item in QuestText)
        {
            ValveInfoText[item.name] = item;
        }

        InitGuide();
    }

    private void InitFadeUI()
    {
        if (backGroundimage != null)
        {
            Color color = backGroundimage.color;
            color.a = 0f;
            backGroundimage.color = color;
            backGroundimage.gameObject.SetActive(false);
        }

        if (messageTextMeshPro != null)
        {
            messageTextMeshPro.text = "";
            messageTextMeshPro.gameObject.SetActive(false);
        }
    }

    public void OnCatchingObject()
    {
        Debug.Log("[TutorialUI] 잡기 가이드 활성화");
        ShowGuide("마우스 좌클릭으로 선택");
    }

    public void OnScroll()
    {
        Debug.Log("[TutorialUI] 줌인아웃 가이드 활성화");
        ShowGuide("마우스 스크롤로 줌인/줌아웃");
    }

    public void OnClear()
    {
        Debug.Log("[TutorialUI] 최종 가이드 활성화");
        ShowGuide("문으로 가서 이동하기");
    }

    public void InitGuide()
    {
        foreach(var text in QuestText)
        {
            RevertValveGuideText(text, Color.white);
        }
    }

    public void ShowGuide(string message)
    {
        currentGuideMessage = message;
        hasGuideMessage = true;

        ApplyGuideText(message);
    }

    public void ShowTemporaryGuide(string valveName, float duration = 1f)
    {
        if (temporaryGuideCoroutine != null)
        {
            StopCoroutine(temporaryGuideCoroutine);
            temporaryGuideCoroutine = null;
        }

        temporaryGuideCoroutine = StartCoroutine(TemporaryGuideRoutine(valveName, duration));
    }

    private IEnumerator TemporaryGuideRoutine(string valveName, float duration)
    {
        if (ValveInfoText.TryGetValue(valveName, out var text))
        {
            text.fontStyle = FontStyles.Strikethrough;
            text.color = Color.green;
        }
        else yield break;

        yield return new WaitForSeconds(duration);

        temporaryGuideCoroutine = null;
    }

    public void RevertValveGuideText(string name, Color color)
    {
        if (ValveInfoText.TryGetValue(name, out var text))
        {
            RevertValveGuideText(text, color);
        }
    }

    private void RevertValveGuideText(TMP_Text text, Color color)
    {
        if (warnig != null)
        {
            StopCoroutine(warnig); 
            warnig = null;
        }

        text.color = color;
        text.fontStyle = FontStyles.Normal;

        if (color != Color.white)
        {
            warnig = StartCoroutine(ColorWarnigRoutine(text));
        }
    }

    private Coroutine warnig = null;
    private IEnumerator ColorWarnigRoutine(TMP_Text text)
    {
        yield return new WaitForSeconds(1);

        text.color = Color.white;
    }

    public void HideGuide()
    {
        if (temporaryGuideCoroutine != null)
        {
            StopCoroutine(temporaryGuideCoroutine);
            temporaryGuideCoroutine = null;
        }

        if (guidePanel != null)
            guidePanel.SetActive(false);
    }

    private void ApplyGuideText(string message)
    {
        if (guidePanel == null)
            return;

        if (guideText != null)
            guideText.text = message;

        guidePanel.SetActive(true);
    }

    // 외부에서 이 함수 호출하면 됨
    public void PlayDarkFade(string message)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        fadeCoroutine = StartCoroutine(DarkFadeRoutine(message));
    }

    private IEnumerator DarkFadeRoutine(string message)
    {
        if (backGroundimage == null)
            yield break;

        backGroundimage.gameObject.SetActive(true);

        if (messageTextMeshPro != null)
        {
            messageTextMeshPro.text = message;
            messageTextMeshPro.gameObject.SetActive(false);
        }

        SetImageAlpha(0f);

        // 1. 3초 뒤 시작
        yield return new WaitForSeconds(fadeStartDelay);

        float elapsed = 0f;
        //bool isTextShown = false;

        // 2. 화면 어두워짐
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            SetImageAlpha(t);

            // 3. 페이드 시작 후 2초쯤 텍스트 표시
            //if (!isTextShown && elapsed >= textShowDelay)
            //{
            //    ShowFadeText(message);
            //    isTextShown = true;
            //}

            yield return null;
        }

        SetImageAlpha(1f);
        ShowFadeText(message);

        // 혹시 textShowDelay가 fadeInDuration보다 크면 여기서라도 표시
        //if (!isTextShown)
        //{
        //    ShowFadeText(message);
        //}
        
        // 4. 어두운 상태 잠깐 유지
        yield return new WaitForSeconds(darkHoldTime);

        if (messageTextMeshPro != null)
            messageTextMeshPro.gameObject.SetActive(false);

        elapsed = 0f;
        hazeControl.StopHaze();
        // 5. 다시 밝아짐
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            SetImageAlpha(1f - t);

            yield return null;
        }

        // SetImageAlpha(0f);

        SetImageAlpha(0f);
        backGroundimage.gameObject.SetActive(false);
        fadeCoroutine = null;

        // 6. 완전히 밝아지면 다른 함수 실행
        onFadeComplete?.Invoke();
    }

    private void ShowFadeText(string message)
    {
        if (messageTextMeshPro == null)
            return;

        messageTextMeshPro.text = message;
        messageTextMeshPro.gameObject.SetActive(true);
    }

    private void SetImageAlpha(float alpha)
    {
        if (backGroundimage == null)
            return;

        Color color = backGroundimage.color;
        color.a = alpha;
        backGroundimage.color = color;
    }
    public void PlayBackgroundDarkOnly()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        fadeCoroutine = StartCoroutine(BackgroundDarkOnlyRoutine());
    }

    private IEnumerator BackgroundDarkOnlyRoutine()
    {
        if (backGroundimage == null)
            yield break;

        backGroundimage.gameObject.SetActive(true);

        if (messageTextMeshPro != null)
        {
            messageTextMeshPro.text = "";
            messageTextMeshPro.gameObject.SetActive(false);
        }

        SetImageAlpha(0f);

        yield return new WaitForSeconds(fadeStartDelay);

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            SetImageAlpha(t);

            yield return null;
        }

        SetImageAlpha(1f);

        fadeCoroutine = null;

        endEvnet?.Invoke();
    }
}