using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using TMPro;

public class VideoProgressUI : MonoBehaviour, IMouseInteractable, IDragInteractable
{
    [Header("Refs")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RectTransform sliderRect; // 슬라이더의 전체 영역(배경 등)

    [Header("UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI timeText;

    private bool isScrubbing = false;

    // --- [IMouseInteractable: 클릭 시 즉시 시킹] ---
    public void ClickEnter()
    {
        isScrubbing = true;
        UpdateValueFromMouse();
        EndScrubAndSeek();
    }

    // --- [IDragInteractable: 드래그 시 실시간 대응] ---
    public void DragStart()
    {
        isScrubbing = true;
    }

    public void Dragging()
    {
        UpdateValueFromMouse();
    }

    public void DragEnd()
    {
        EndScrubAndSeek();
    }

    // 마우스 위치를 슬라이더 값(0~1)으로 변환하는 핵심 로직
    private void UpdateValueFromMouse()
    {
        if (!sliderRect || !progressSlider) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect, mousePos, Camera.main, out Vector2 localPoint))
        {
            float width = sliderRect.rect.width;
            // 피벗(Pivot)값을 더해줘서 0~1 사이 값을 정확히 계산함
            float normalizedValue = Mathf.Clamp01((localPoint.x / width) + sliderRect.pivot.x);
            progressSlider.value = normalizedValue;
        }
    }

    // --- [기존 유틸리티 함수들] ---
    private void Update()
    {
        if (!videoPlayer || !videoPlayer.isPrepared || isScrubbing) return;

        float t = (float)(videoPlayer.time / videoPlayer.length);
        if (progressSlider) progressSlider.SetValueWithoutNotify(t);
        if (timeText) timeText.text = $"{FormatTime(videoPlayer.time)}/{FormatTime(videoPlayer.length)}";
    }

    public void EndScrubAndSeek()
    {
        isScrubbing = false;
        if (videoPlayer && videoPlayer.isPrepared)
        {
            videoPlayer.time = progressSlider.value * videoPlayer.length;
        }
    }

    private string FormatTime(double seconds)
    {
        int s = Mathf.FloorToInt((float)seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }

    // 인터페이스 미사용 메서드 (빈칸 유지)
    public void ClickExit() { }
    public void ClickCancle() { }
    public void HoverEnter() { }
    public void HoverExit() { }
}