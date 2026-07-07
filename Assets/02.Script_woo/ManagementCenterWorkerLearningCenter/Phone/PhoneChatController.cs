using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhoneChatController : MonoBehaviour
{
    [Header("Chat UI")]
    [SerializeField] private GameObject chatRoot;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Bubble Prefabs")]
    [SerializeField] private PhoneChatBubbleView leftBubblePrefab;
    [SerializeField] private PhoneChatBubbleView rightBubblePrefab;
    [SerializeField] private PhoneChatBubbleView centerBubblePrefab;

    [Header("Click")]
    [SerializeField] private Button nextClickButton;

    private PhoneChatAsset currentAsset;
    private int currentIndex;
    private bool isPlaying;
    private Action onChatFinished;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (chatRoot != null)
            chatRoot.SetActive(false);

        if (nextClickButton != null)
            nextClickButton.onClick.AddListener(ShowNextLine);
    }

    private void OnDestroy()
    {
        if (nextClickButton != null)
            nextClickButton.onClick.RemoveListener(ShowNextLine);
    }

    public void Play(PhoneChatAsset asset, Action finishedCallback = null)
    {
        if (asset == null)
        {
            Debug.LogError("PhoneChatController: PhoneChatAsset이 없습니다.");
            return;
        }

        if (asset.lines == null || asset.lines.Count == 0)
        {
            Debug.LogError("PhoneChatController: 대화 데이터가 비어 있습니다.");
            return;
        }

        currentAsset = asset;
        currentIndex = 0;
        onChatFinished = finishedCallback;
        isPlaying = true;

        ClearChat();

        if (chatRoot != null)
            chatRoot.SetActive(true);

        // 시작하자마자 첫 대사 표시
        SpawnLine(currentAsset.lines[currentIndex]);
        currentIndex++;
    }

    public void ShowNextLine()
    {
        if (!isPlaying)
            return;

        if (currentAsset == null)
        {
            FinishChat();
            return;
        }

        if (currentIndex >= currentAsset.lines.Count)
        {
            FinishChat();
            return;
        }

        SpawnLine(currentAsset.lines[currentIndex]);
        currentIndex++;
    }

    private void SpawnLine(PhoneChatLine line)
    {
        if (line == null || contentRoot == null)
            return;

        PhoneChatBubbleView prefab = GetPrefab(line.side);

        if (prefab == null)
        {
            Debug.LogWarning($"PhoneChatController: {line.side} 프리팹이 없습니다.");
            return;
        }

        PhoneChatBubbleView bubble = Instantiate(prefab, contentRoot);
        bubble.SetData(line.speaker, line.message);

        StartCoroutine(ScrollToBottomNextFrame());
    }

    private PhoneChatBubbleView GetPrefab(PhoneChatSide side)
    {
        switch (side)
        {
            case PhoneChatSide.Left:
                return leftBubblePrefab;

            case PhoneChatSide.Right:
                return rightBubblePrefab;

            case PhoneChatSide.Center:
                return centerBubblePrefab;

            default:
                return leftBubblePrefab;
        }
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    private void ClearChat()
    {
        if (contentRoot == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }

    public void FinishChat()
    {
        isPlaying = false;

        if (chatRoot != null)
            chatRoot.SetActive(false);

        ClearChat();

        currentAsset = null;
        currentIndex = 0;

        Action callback = onChatFinished;
        onChatFinished = null;

        callback?.Invoke();
    }
}