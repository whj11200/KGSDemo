using System;
using System.Collections.Generic;

public interface IDialogueView
{
    void Show(bool visible);

    void SetSpeaker(string speakerId);
    void SetBodyText(string text);

    void SetTypingVisible(bool visible);
    void SetContinueHintVisible(bool visible);

    void ShowChoices(IReadOnlyList<ChoiceVM> choices, Action<int> onPick);
    void HideChoices();
}

public readonly struct ChoiceVM
{
    public readonly string Text;
    public ChoiceVM(string text) => Text = text;
}
