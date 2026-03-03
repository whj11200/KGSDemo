using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoProgressUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI timeText;

    private double videoLength = 0;
    private bool isScrubbing = false; // 사용자가 조작 중

    private void Awake()
    {
        if (progressSlider)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.SetValueWithoutNotify(0f);

            // 드래그로 값이 바뀌는 동안 "미리보기"처럼 시간/슬라이더 갱신은 가능
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        if (timeText) timeText.text = "00:00/00:00";
    }

    private void OnDestroy()
    {
        if (progressSlider)
            progressSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnEnable()
    {
        if (!videoPlayer) return;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnDisable()
    {
        if (!videoPlayer) return;
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnVideoEnd;
    }

    public void Prepare()
    {
        if (!videoPlayer) return;

        if (videoPlayer.isPrepared)
        {
            CacheLength();
            UpdateUI(0);
            return;
        }

        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        CacheLength();
        UpdateUI(vp.time);
    }

    private void CacheLength()
    {
        videoLength = videoPlayer.length;
        if (videoLength <= 0) videoLength = 0;
    }

    private void Update()
    {
        if (!videoPlayer || !videoPlayer.isPrepared) return;
        if (videoLength <= 0) return;

        // 사용자가 드래그(스크러빙) 중엔 자동으로 슬라이더를 덮어쓰지 않음
        if (isScrubbing) return;

        float t = Mathf.Clamp01((float)(videoPlayer.time / videoLength));

        if (progressSlider) progressSlider.SetValueWithoutNotify(t);

        if (timeText)
            timeText.text = $"{FormatTime(videoPlayer.time)}/{FormatTime(videoLength)}";
    }

    private void UpdateUI(double currentTime)
    {
        if (videoLength <= 0) return;

        float t = Mathf.Clamp01((float)(currentTime / videoLength));

        if (progressSlider) progressSlider.SetValueWithoutNotify(t);

        if (timeText)
            timeText.text = $"{FormatTime(currentTime)}/{FormatTime(videoLength)}";
    }

    private void OnSliderValueChanged(float v01)
    {
        if (!videoPlayer || !videoPlayer.isPrepared) return;
        if (videoLength <= 0) return;

        // 스크러빙 중이면 텍스트를 "가려는 위치"로 미리 보여주기
        if (isScrubbing && timeText)
        {
            double previewTime = v01 * videoLength;
            timeText.text = $"{FormatTime(previewTime)}/{FormatTime(videoLength)}";
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (progressSlider) progressSlider.SetValueWithoutNotify(0f);
        if (timeText) timeText.text = $"00:00/{FormatTime(videoLength)}";
    }

    //  외부(이벤트 트리거/클릭 스크립트)가 호출할 API
    public void BeginScrub() => isScrubbing = true;

    public void EndScrubAndSeek()
    {
        if (!videoPlayer || !videoPlayer.isPrepared) { isScrubbing = false; return; }
        if (videoLength <= 0) { isScrubbing = false; return; }

        float v01 = progressSlider ? progressSlider.value : 0f;

        isScrubbing = false;

        videoPlayer.time = v01 * videoLength;

        // WebGL/일부 플랫폼에서 seek 반영을 빠르게 하려면 Play/Pause 상태에 따라 아래가 도움될 때가 있음
        // if (!videoPlayer.isPlaying) videoPlayer.Play(); videoPlayer.Pause();

        UpdateUI(videoPlayer.time);
    }

    // 클릭 점프 시킹용(다른 스크립트가 호출)
    public void SeekByNormalized(float v01)
    {
        if (!videoPlayer || !videoPlayer.isPrepared) return;
        if (videoLength <= 0) return;

        v01 = Mathf.Clamp01(v01);

        if (progressSlider) progressSlider.SetValueWithoutNotify(v01);

        videoPlayer.time = v01 * videoLength;
        UpdateUI(videoPlayer.time);
    }

    private string FormatTime(double seconds)
    {
        if (seconds < 0) seconds = 0;
        int s = Mathf.FloorToInt((float)seconds);
        int m = s / 60;
        int r = s % 60;
        return $"{m:00}:{r:00}";
    }
}