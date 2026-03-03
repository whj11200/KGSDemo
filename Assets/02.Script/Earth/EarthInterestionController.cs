using UnityEngine;

public class EarthInterestionController : MonoBehaviour
{
    public enum Step
    {
        Intro = 0,              // Normal()
        EarthOn = 1,            // 지구확대
        RegionalSelection = 2,  // 지역선택
    }

    [Header("Refs")]
    [SerializeField] private EarthInterestion view;

    [Header("Start Step")]
    [SerializeField] private Step startStep = Step.Intro;

    public Step CurrentStep => _current;
    private Step _current;

    [SerializeField] bool _isBacking;
    [SerializeField] Step _pendingPrevStep;

    private void Awake()
    {
        if (!view) view = GetComponent<EarthInterestion>();
    }

    private void Start()
    {
        GoTo(startStep, applyEnterActions: true);
    }
    // 시작 버튼 할당
    public void Next()
    {
        if (_isBacking) return;

        var next = (Step)Mathf.Min((int)_current + 1, (int)Step.RegionalSelection);
        if (next == _current) return;

        GoTo(next, applyEnterActions: true);
    }
    // 되돌리기 버튼 할당
    public void Prev()
    {
        if (_isBacking) return;
        if (_current == Step.Intro) return;

        _pendingPrevStep = (Step)Mathf.Max((int)_current - 1, (int)Step.Intro);
        Debug.Log($"현재 스테이지: {_pendingPrevStep}");
        _isBacking = true;
        view.BackAnimation();
    }

    // 애니메이션 이벤트로 뒤돌아가기 완료 시점 맞춰 호출
    public void OnBackAnimationComplete()
    {
        _isBacking = false;
        GoTo(_pendingPrevStep, applyEnterActions: true);
    }

    public void GoTo(Step to, bool applyEnterActions)
    {
        if (_isBacking) return;
        var from = _current;
        if (from == to && applyEnterActions == false) return;
        _current = to;

        if (applyEnterActions)
            EnterStep(to, from);
    }

    private void EnterStep(Step step, Step from)
    {
        switch (step)
        {
            case Step.Intro:
                view.Normal();
                Debug.Log("초기상태");
                break;

            case Step.EarthOn:
                view.StepOne();
                Debug.Log("지구내려온 상태");
                break;

            case Step.RegionalSelection:
                Debug.Log("지역선택 상태");
                break;

            
        }
    }


}