using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDialogueViewUGUI : OverlayUI, IDialogueView
{
    [Header("Root")]
    [SerializeField] GameObject root;

    [Header("Texts")]
    [SerializeField] Image speakerBG;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] float charsPerSecond = 15f;

    [Header("Hints")]
    [SerializeField] GameObject typingIcon;
    [SerializeField] GameObject continueHint;
    [SerializeField] TMP_Text HintText;

    [Header("Choices")]
    [SerializeField] Transform choiceRoot;
    [SerializeField] Button choiceButtonPrefab;

    readonly List<Button> _spawned = new();

    private TypingEffect TypingEffect;

    protected override void Awake()
    {
        base.Awake();

        if (speakerBG == null)
            speakerBG = speakerText.GetComponentInParent<Image>();

        if (HintText == null)
        {
            HintText = continueHint.GetComponent<TMP_Text>();
        }

        var continueKey = PlayerDeviceManager.IsDesktop ? "Space" : "A";

        HintText.text = $"[ {continueKey} ]";
    }

    public void Show(bool visible) => root.SetActive(visible);
    
    public void SetSpeaker(string speakerId = "")
    {
        if (string.IsNullOrEmpty(speakerId))
        {
            speakerBG.enabled = false;
            speakerText.text = string.Empty;
            return;
        }

        if (speakerText)
        {
            speakerBG.enabled = true;
            speakerText.text = speakerId ?? "";
        }
        else
        {
            speakerBG.enabled = false;
            speakerText.text = string.Empty;
        }
    }

    public void SetBodyText(string text)
    {
        if (bodyText) bodyText.text = text ?? "";
    }

    public void SetBodyTextWithTyping(string text, ScenarioEvent eventArgs = null, Action OnEnd = null)
    {
        if (eventArgs == null) SetBodyText(text);
        else
        {
            var clipLength = eventArgs.FloatValue;

            if (TypingEffect == null)
            {
                var go = new GameObject("TypingEffect");
                TypingEffect = go.AddComponent<TypingEffect>();
                TypingEffect.charsPerSecond = charsPerSecond;
            }

            TypingEffect.Apply(bodyText, text, clipLength, null, OnEnd);
        }
    }

    public void Complete()
    {
        TypingEffect.Complete();
    }

    public void SetTypingVisible(bool visible)
    {
        if (typingIcon) typingIcon.SetActive(visible);
    }

    public void SetContinueHintVisible(bool visible)
    {
        if (continueHint) continueHint.SetActive(visible);
    }

    public void ShowChoices(IReadOnlyList<ChoiceVM> choices, Action<int> onPick)
    {
        HideChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            int idx = i;
            var btn = Instantiate(choiceButtonPrefab, choiceRoot);
            _spawned.Add(btn);

            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = choices[i].Text;

            btn.onClick.AddListener(() => onPick?.Invoke(idx));
            btn.gameObject.SetActive(true);
        }
    }

    public void HideChoices()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i]) Destroy(_spawned[i].gameObject);
        }
        _spawned.Clear();
    }
}
