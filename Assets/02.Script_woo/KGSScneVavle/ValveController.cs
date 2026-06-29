using UnityEngine;

public class ValveController : MonoBehaviour
{
    private Animator anim;

    // 파티클과 웅덩이 컨트롤러 연결 (필요 시)
    public ParticleSystem waterParticle;
    public PuddleController puddleController;
    [SerializeField] EnvironmentManager manager;
    [SerializeField] GameObject vavleZone;
    [SerializeField]public bool isLeaking = false;

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
        //anim.SetInteger("State", 1); // Leak 상태
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
        manager.CompleteMission(KGS_EnvEventType.VavleCloseClear); // 미션 성공 처리
        //anim.SetInteger("State", 2); // Close 상태 (여기서 고정!)
        Debug.Log("밸브가 닫혔습니다.");
    }

    // 3. 트리거 존(Player 퇴장 등)에서 호출할 리셋 함수
    public void ResetValve()
    {
        // 1. 기존에 소비되지 않고 남아있을 수 있는 트리거들을 모두 청소합니다. (매우 중요)
        anim.ResetTrigger("Leak");
        anim.ResetTrigger("Close");
        anim.ResetTrigger("Reset");

        // 2. 상태값 초기화
        isLeaking = false;

        // 3. 파티클 정지
        if (waterParticle != null) waterParticle.Stop();

        // 4. 애니메이터 리셋 트리거 발동
        anim.SetTrigger("Reset");

        if (vavleZone != null) vavleZone.SetActive(true);

        Debug.Log("밸브 상태가 완전히 초기화되었습니다 (트리거 청소 완료).");
    }
}