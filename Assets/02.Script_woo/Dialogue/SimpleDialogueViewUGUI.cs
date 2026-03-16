using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleDialogueViewUGUI : MonoBehaviour, IDialogueView
{
    [Header("Root")]
    [SerializeField] GameObject root;

    [Header("Texts")]
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;

    [Header("Hints")]
    [SerializeField] GameObject typingIcon;
    [SerializeField] GameObject continueHint;

    [Header("Choices")]
    [SerializeField] Transform choiceRoot;
    [SerializeField] Button choiceButtonPrefab;

    readonly List<Button> _spawned = new();

    public void Show(bool visible) => root.SetActive(visible);

    public void SetSpeaker(string speakerId)
    {
        if (speakerText) speakerText.text = speakerId ?? "";
    }

    public void SetBodyText(string text)
    {
        if (bodyText) bodyText.text = text ?? "";
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
