using UnityEngine;

public class ValveController : MonoBehaviour
{
    private Animator anim;

    // 파티클과 웅덩이 컨트롤러 연결 (필요 시)
    public ParticleSystem waterParticle;
    public PuddleController puddleController;
    [SerializeField] EnvironmentManager manager;
    [SerializeField] GameObject vavleZone;
    public bool isLeaking = false;

    void Start()
    {
        waterParticle.Stop(); // 시작 시 파티클 비활성화
        anim = GetComponent<Animator>();
        
    }

    // 1. 외부(퀘스트, 다이얼로그 매니저 등)에서 호출할 누출 시작 함수
    public void StartLeak()
    {
        if (isLeaking) return;

        isLeaking = true;
        anim.SetTrigger("Leak"); // 애니메이터의 Leak 트리거 발동
        
        if (waterParticle != null) waterParticle.Play();
        Debug.Log("밸브 누출 시작: 외부에서 호출됨");
    }

    public void OnInteract()
    {
        if (isLeaking)
        {
            CloseValve();

        }
    }

    private void CloseValve()
    {
        isLeaking = false;
        anim.SetTrigger("Close"); // 애니메이터의 Close 트리거 발동
        manager.CompleteMission(EnvEventType.VavleCloseClear); // 미션 성공 처리
        Debug.Log("밸브가 닫혔습니다.");
    }

    // 3. 트리거 존(Player 퇴장 등)에서 호출할 리셋 함수
    public void ResetValve()
    {
        isLeaking = false;
        anim.SetTrigger("Reset"); // 애니메이터의 Reset 트리거 발동
        if (waterParticle != null) waterParticle.Stop();
        vavleZone.SetActive(true); // 밸브 존 활성화 (필요 시)
        Debug.Log("밸브 상태가 초기화되었습니다.");
    }
}