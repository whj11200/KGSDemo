using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class DialogueController : MonoBehaviour
{
    public enum State
    {
        Idle,
        Playing,
        Typing,
        WaitingChoice,
        WaitingContinue,
        Ending
    }

    [Header("Refs")]
    [SerializeField] MonoBehaviour viewProvider;   // IDialogueView
    [SerializeField] MonoBehaviour inputProvider;  // IDialogueInput
    [SerializeField] AudioSource voiceSource;      // 선택

    [Header("Typing")]
    [SerializeField] float charsPerSecond = 40f;

    public State CurrentState => _state;
    public bool IsPlaying => _state != State.Idle;

    IDialogueView View => (IDialogueView)viewProvider;
    IDialogueInput Input => (IDialogueInput)inputProvider;

    State _state = State.Idle;

    DialogueAsset _asset;
    Dictionary<string, DialogueNode> _nodeMap;
    DialogueNode _current;

    Coroutine _flowRoutine;
    Coroutine _typingRoutine;

    bool _requestNext;
    bool _requestSkip;

    Action _onFinished;

    // ===== 공개 API (이것만 쓰면 됨) =====

    private void Start()
    {
        if(viewProvider is IDialogueView v)
        {
            v.Show(false);
        }
        _state = State.Idle;
    }
   
    public void Play(DialogueAsset asset, string startNodeId = null, Action onFinished = null)
    {
        if (asset == null) throw new ArgumentNullException(nameof(asset));
        Stop();

        _asset = asset;
        _nodeMap = asset.nodes
            .Where(n => !string.IsNullOrWhiteSpace(n.nodeId))
            .GroupBy(n => n.nodeId)
            .ToDictionary(g => g.Key, g => g.First());

        _onFinished = onFinished;

        var firstId = string.IsNullOrWhiteSpace(startNodeId)
            ? asset.nodes.FirstOrDefault()?.nodeId
            : startNodeId;

        if (string.IsNullOrWhiteSpace(firstId) || !_nodeMap.TryGetValue(firstId, out _current))
        {
            Debug.LogError($"Dialogue start node not found. asset={asset.name}, node={firstId}"); 
            Finish();
            return;
        }

        View.Show(true);
        _flowRoutine = StartCoroutine(FlowLoop());
    }

    public void Stop()
    {
        if (_flowRoutine != null) StopCoroutine(_flowRoutine);
        if (_typingRoutine != null) StopCoroutine(_typingRoutine);

        _flowRoutine = null;
        _typingRoutine = null;

        _asset = null;
        _nodeMap = null;
        _current = null;

        _requestNext = false;
        _requestSkip = false;

        _state = State.Idle;

        if (viewProvider is IDialogueView v)
        {
            v.HideChoices();
            v.Show(false);
        }
    }

    // ===== Unity Loop =====

    void Update()
    {
        if (_state == State.Idle) return;

        if (Input != null)
        {
            if (Input.NextPressed()) _requestNext = true;
            if (Input.SkipPressed()) _requestSkip = true;
        }
        if (Input.SkipPressed())
        {
            Finish();  // 코루틴까지 끊고 UI 닫음 (Finish보다 안전)
            DialogueEventBus.Raise("DIALOGUE_SKIP");
            return;
        }
    }

    // ===== 내부 로직 =====

    IEnumerator FlowLoop()
    {
        _state = State.Playing;

        while (_current != null)
        {
            var node = _current;

            ApplyNode(node);

            yield return RunTyping(node.text);

            if (node.choices != null && node.choices.Count > 0)
            {
                string pickedNext = null;
                yield return WaitChoice(node, next => pickedNext = next);

                RaiseExitEvent(node);
                GoNext(pickedNext);
                continue;
            }

            if (node.autoAdvanceDelay > 0f)
            {
                yield return WaitAutoOrNext(node.autoAdvanceDelay);

                RaiseExitEvent(node);
                GoNext(node.nextNodeId);
                continue;
            }

            yield return WaitContinue();

            RaiseExitEvent(node);
            GoNext(node.nextNodeId);
        }

        Finish();
    }


    void ApplySpeaker(string speakerId)
    {
        View.SetSpeaker(speakerId);
    }

    void PlayVoice(AudioClip clip)
    {
        if (!voiceSource) return;
        voiceSource.Stop();
        if (clip) voiceSource.PlayOneShot(clip);
    }

    IEnumerator RunTyping(string fullText)
    {
        _state = State.Typing;

        View.HideChoices();
        View.SetContinueHintVisible(false);
        View.SetTypingVisible(true);
        View.SetBodyText(string.Empty);

        _requestSkip = false;
        _requestNext = false;

        float secPerChar = charsPerSecond <= 0 ? 0 : 1f / charsPerSecond;

        for (int i = 0; i < fullText.Length; i++)
        {
            //  타이핑 중에는 Next를 무시한다 (스킵 금지)
            // Skip만 허용하고 싶으면 아래 한 줄만 남기면 됨:
            if (_requestSkip)
            {
                // 스킵도 막고 싶으면 이 블록 자체를 지워라.
                View.SetBodyText(fullText);
                _requestSkip = false;
                break;
            }

            View.SetBodyText(fullText.Substring(0, i + 1));
            if (secPerChar > 0) yield return new WaitForSeconds(secPerChar);
            else yield return null;
        }

        View.SetTypingVisible(false);
        _state = State.Playing;
    }


    IEnumerator WaitChoice(DialogueNode node, Action<string> onPickedNext)
    {
        _state = State.WaitingChoice;

        _requestNext = false;
        _requestSkip = false;

        var vms = node.choices.Select(c => new ChoiceVM(c.text)).ToList();
        int pickedIndex = -1;

        View.ShowChoices(vms, idx => pickedIndex = idx);

        while (pickedIndex < 0)
            yield return null;

        View.HideChoices();

        onPickedNext?.Invoke(node.choices[pickedIndex].nextNodeId);

        _state = State.Playing;
    }

    IEnumerator WaitContinue()
    {
        _state = State.WaitingContinue;

        _requestNext = false;
        View.SetContinueHintVisible(true);

        while (!_requestNext)
            yield return null;

        _requestNext = false;
        View.SetContinueHintVisible(false);

        _state = State.Playing;
    }

    IEnumerator WaitAutoOrNext(float delay)
    {
        _state = State.WaitingContinue;

        _requestNext = false;
        View.SetContinueHintVisible(false);

        float t = 0f;
        while (t < delay && !_requestNext)
        {
            t += Time.deltaTime;
            yield return null;
        }

        _requestNext = false;
        _state = State.Playing;
    }

    void GoNext(string nextNodeId)
    {
        if (string.IsNullOrWhiteSpace(nextNodeId))
        {
            _current = null;
            return;
        }

        if (_nodeMap != null && _nodeMap.TryGetValue(nextNodeId, out var next))
        {
            _current = next;
        }
        else
        {
            Debug.LogWarning($"Next node not found: {nextNodeId}");
            _current = null;
        }
    }

    void Finish()
    {
        _state = State.Ending;
        View.HideChoices();
        View.Show(false);

        var cb = _onFinished;
        _onFinished = null;

        _asset = null;
        _nodeMap = null;
        _current = null;

        _state = State.Idle;

        cb?.Invoke();
    }
    void ApplyNode(DialogueNode node)
    {
        ApplySpeaker(node.speakerId);
        PlayVoice(node.voice);

        if (!string.IsNullOrEmpty(node.onEnterEvent))
        {
            Debug.Log($"[Dialogue] Raise: {node.onEnterEvent}");
            DialogueEventBus.Raise(node.onEnterEvent);
        }
    }
    void RaiseExitEvent(DialogueNode node)
    {
        if (node == null) return;

        if (!string.IsNullOrEmpty(node.onExitEvent))
        {
            Debug.Log($"[Dialogue] RaiseExit: {node.onExitEvent}");
            DialogueEventBus.Raise(node.onExitEvent);
        }
    }
}
