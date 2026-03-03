using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoSetting : MonoBehaviour
{
    [SerializeField] Button play_btn;
    [SerializeField] Button pause_btn;      // RawImage + Button
    [SerializeField] GameObject stopImage;  // 멈춤 시 표시
    [SerializeField] VideoPlayer video_player;

    bool isPlaying = false;

    private void Start()
    {
        play_btn.gameObject.SetActive(true);
        pause_btn.gameObject.SetActive(false);
        stopImage.SetActive(false);

        pause_btn.onClick.AddListener(TogglePause);

        //  영상 종료 이벤트
        video_player.loopPointReached += OnVideoEnd;
    }

    public void VideoPlay()
    {
        play_btn.gameObject.SetActive(false);
        pause_btn.gameObject.SetActive(true);

        stopImage.SetActive(false);
        video_player.Play();
        isPlaying = true;
    }

    void TogglePause()
    {
        if (isPlaying)
        {
            video_player.Pause();
            stopImage.SetActive(true);
            isPlaying = false;
        }
        else
        {
            stopImage.SetActive(false);
            video_player.Play();
            isPlaying = true;
        }
    }

    //  영상 끝났을 때 호출
    void OnVideoEnd(VideoPlayer vp)
    {
        isPlaying = false;

        vp.Stop(); // 상태 리셋 (중요)

        play_btn.gameObject.SetActive(true);
        pause_btn.gameObject.SetActive(false);
        stopImage.SetActive(false);
    }

    private void OnDestroy()
    {
        // 이벤트 해제 (안전)
        video_player.loopPointReached -= OnVideoEnd;
    }
}
