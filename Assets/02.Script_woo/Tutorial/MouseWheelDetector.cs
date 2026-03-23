using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWheelDetector : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField]  InputActionReference scrollActionRef; // InputActionReference 사용

    [Header("References")]
    [SerializeField] TutorialManager tutorialManager;

    private void OnEnable()
    {
        // 액션 활성화
        if (scrollActionRef != null) scrollActionRef.action.Enable();
    }

    void Update()
    {
        // 1. 현재 튜토리얼 매니저의 상태가 "S1"인지 확인 
        // (S1이 끝난 직후 혹은 S1 대화 중에 S2로 넘어가기 위한 조작인지 확인)
        if (tutorialManager.currentNodeID == "S1")
        {
            CheckScroll();
        }
    }

    private void CheckScroll()
    {
        // 2. 휠 값 읽기
        Vector2 scrollValue = scrollActionRef.action.ReadValue<Vector2>();

        // 3. 휠을 위/아래 어디로든 움직였다면
        if (Mathf.Abs(scrollValue.y) > 0.01f)
        {
            Debug.Log("S1 상태에서 휠 조작 감지 -> S2로 전환 시도");

            // 4. S2 미션 완료 전달 (그러면 매니저가 S2 노드를 플레이함)
            tutorialManager.CompleteMisson(TutorialEventType.ScrollzoominoutClear);

            // 중복 실행 방지를 위해 이 컴포넌트를 잠시 끔 (필요 시)
            // this.enabled = false;
        }
    }
}