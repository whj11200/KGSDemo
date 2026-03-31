using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSceneTrigger : MonoBehaviour
{
    public enum SceneName
    {
        ManagementCenterWorkerLearningCenter, // 관리소 작업자 학습관
        CivicEvacuationExperienceOfficer
    }
    [Header("이동 설정")]
    [SerializeField] private SceneName targetScene; // 인스펙터에서 드롭다운으로 선택

    [Header("전환 방식")]
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single; // 단일 씬 혹은 합치기(Additive)

    [Header("효과")]
    [SerializeField] private bool useFade = true; // 페이드 효과 사용 여부 (FadeUI가 있을 경우)
    [SerializeField] private FadeUi fadeUi; // 필요시 페이드 UI 참조

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌했는지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{targetScene}으로 이동을 시작합니다.");

            if (useFade && fadeUi != null)
            {
                // 페이드 아웃 후 씬 전환 (코루틴 방식)
                StartCoroutine(TransitionWithFade());
            }
            else
            {
                // 즉시 전환
                LoadTargetScene();
            }
        }
    }

    private void LoadTargetScene()
    {
        // Enum 이름을 문자열로 변환하여 씬 로드
        SceneManager.LoadScene(targetScene.ToString(), loadMode);
    }

    private System.Collections.IEnumerator TransitionWithFade()
    {
        // 1. 페이드 아웃 시작
        // fadeUi.FadeOut(); // 기존 프로젝트의 FadeUI 함수명에 맞춰 수정하세요.

        yield return new WaitForSeconds(1f); // 페이드 연출 시간 대기

        // 2. 씬 로드
        LoadTargetScene();
    }
}
