using UnityEngine;
using UnityEngine.EventSystems;

public class SliderClickToSeek : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RectTransform targetRect; // 클릭 좌표를 계산할 영역
    [SerializeField] private VideoProgressUI progress; // 위 스크립트 참조

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!targetRect || !progress) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            return;

        // localPoint.x 는 targetRect 중심이 0, 왼쪽이 -width/2, 오른쪽이 +width/2
        float width = targetRect.rect.width;
        if (width <= 0.0001f) return;

        float normalized = (localPoint.x / width) + 0.5f; // 0~1
        normalized = Mathf.Clamp01(normalized);

        //  클릭한 곳으로 바로 점프 시킹
        progress.SeekByNormalized(normalized);
    }
}