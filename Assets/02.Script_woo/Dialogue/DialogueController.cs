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
    [SerializeField] EnvironmentManager environmentManager; // 환경 이벤트 처리를 위한 매니저 참조
    [SerializeField] TutorialManager tutorialManager;

    [Header("Typing")]
    [SerializeField] float charsPerSecond = 40f;

    public State CurrentState => _state;
    public DialogueAsset CurrentAsset => _asset;
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
    public bool _requestSkip;

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
    public void JumpToNode(string nodeId)
    {
        // 이미 Play를 통해 _asset이 할당된 상태라면, 그 에셋을 그대로 씁니다.
        if (_asset == null)
        {
            Debug.LogError("재생 중인 에셋이 없어서 점프할 수 없습니다!");
            return;
        }

        // 에셋은 그대로 두고 노드 ID만 바꿔서 다시 Play 호출
        Play(_asset, nodeId);
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
            if(environmentManager != null)
            {
                environmentManager.AllClear(); // 대화 종료 시 환경 이벤트 초기화
            }
            if(tutorialManager != null)
            {
                tutorialManager.AllClear_T();
            }
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
        //if(environmentManager != null)
        //{
        //    environmentManager.AllClear(); // 대화 종료 시 환경 이벤트 초기화
        //}
        cb?.Invoke();
    }
    // [수정 완료] 노드 진입 시 호출 (화자 설정, 음성 재생, NPC 행동, 환경 이벤트)
    void ApplyNode(DialogueNode node)
    {
        if(environmentManager != null)
        {
            environmentManager.HandleDialogueStart(node.nodeId);
        }
        if( tutorialManager != null)
        {
            tutorialManager.HandleDialogueStart(node.nodeId);
        }
        // 1. 화자 이름 및 보이스 설정 (이 부분이 빠졌었네요!)
        ApplySpeaker(node.speakerId);
        PlayVoice(node.voice);

        // 2. NPC 행동 이벤트 실행 (Enum -> String 변환 후 Raise)
        if (node.npcEnterAction != NPCActionType.None)
        {
            string npcEvt = node.npcEnterAction.ToString();
            Debug.Log($"[Dialogue] NPC Enter Action: {npcEvt}");
            DialogueEventBus.Raise(npcEvt);
        }

        // 3. 환경/오브젝트 이벤트 실행 (Enum -> String 변환 후 Raise)
        if (node.envEnterEvent != KGS_EnvEventType.None)
        {
            string envEvt = node.envEnterEvent.ToString();
            Debug.Log($"[Dialogue] Env Enter Event: {envEvt}");
            DialogueEventBus.Raise(envEvt);
        }
    }

    // [수정 완료] 노드 탈출 시 호출
    void RaiseExitEvent(DialogueNode node)
    {
        if (node == null) return;

        // 1. NPC 퇴장 행동 실행
        if (node.npcExitAction != NPCActionType.None)
        {
            string npcEvt = node.npcExitAction.ToString();
            Debug.Log($"[Dialogue] NPC Exit Action: {npcEvt}");
            DialogueEventBus.Raise(npcEvt);
        }

        // 2. 환경 퇴장 이벤트 실행
        if (node.envExitEvent != KGS_EnvEventType.None)
        {
            string envEvt = node.envExitEvent.ToString();
            Debug.Log($"[Dialogue] Env Exit Event: {envEvt}");
            DialogueEventBus.Raise(envEvt);
        }
    }


}
