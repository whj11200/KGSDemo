using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSetting : MonoBehaviour, IMouseInteractable 
{
    [SerializeField] Button play_btn;
    [SerializeField] Button pause_btn;      // 재생 중일 때 노출될 버튼 영역
    [SerializeField] GameObject stopImage;  // 일시정지 아이콘 또는 오버레이
    [SerializeField] VideoPlayer video_player;

    bool isPlaying = false;

    private void Start()
    {
        // 초기 상태 설정
        ResetUI();

        // 영상 종료 이벤트 등록
        video_player.loopPointReached += OnVideoEnd;
        play_btn.onClick.AddListener(ToggleVideo);
        pause_btn.onClick.AddListener(ToggleVideo);
    }

    // 재생/일시정지를 하나로 통합한 토글 함수
    public void ToggleVideo()
    {
        if (video_player == null) return;

        if (!isPlaying)
        {
            // 재생 시작
            video_player.Play();
            play_btn.gameObject.SetActive(false);
            pause_btn.gameObject.SetActive(true);
            stopImage.SetActive(false);
            isPlaying = true;
            print("영상 재생 시작");
        }
        else
        {
            // 일시정지
            video_player.Pause();
            stopImage.SetActive(true);
            // 버튼 상태는 상황에 따라 유지하거나 변경 (여기서는 pause_btn 상태 유지)
            isPlaying = false;
            print("영상 일시정지");
        }
    }

    // UI 상태를 초기화하는 헬퍼 함수
    private void ResetUI()
    {
        play_btn.gameObject.SetActive(true);
        pause_btn.gameObject.SetActive(false);
        stopImage.SetActive(false);
        isPlaying = false;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        vp.Stop(); 
        ResetUI();
        print("영상 종료");
    }

    private void OnDestroy()
    {
        if (video_player != null)
            video_player.loopPointReached -= OnVideoEnd;
    }

    // --- IMouseInteractable 구현 부 ---

    public void ClickEnter()
    {
        // 레이캐스트가 이 오브젝트(콜라이더)를 클릭했을 때 토글 실행
        ToggleVideo();
    }

    public void ClickExit() { /* 필요 시 작성 */ }
    public void ClickCancle() { /* 필요 시 작성 */ }
    public void HoverEnter() { /* 필요 시 작성 */ }
    public void HoverExit() { /* 필요 시 작성 */ }
}