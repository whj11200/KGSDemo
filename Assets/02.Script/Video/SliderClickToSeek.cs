using UnityEngine;
using UnityEngine.InputSystem; // 마우스 좌표를 직접 가져오기 위해 필요

public class SliderClickToSeek : MonoBehaviour, IMouseInteractable
{
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private VideoProgressUI progress;

    // IMouseInteractable 인터페이스 구현 (Raycaster가 호출함)
    public void ClickEnter()
    {
        DoSeek();
    }

    private void DoSeek()
    {
        if (!targetRect || !progress) return;

        // 현재 마우스 위치 가져오기 (Input System 사용 시)
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 마우스 좌표를 슬라이더 내부의 로컬 좌표로 변환
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRect,
                mousePosition,
                Camera.main, // 월드 공간 UI이므로 메인 카메라 사용
                out Vector2 localPoint))
            return;

        float width = targetRect.rect.width;
        if (width <= 0.0001f) return;

        // 중심이 0이므로 비율 계산 (0~1 사이 값으로 변환)
        float normalized = (localPoint.x / width) + targetRect.pivot.x;
        normalized = Mathf.Clamp01(normalized);

        // 클릭한 위치로 영상 이동
        //progress.SeekByNormalized(normalized);
        Debug.Log($"슬라이더 클릭 시킹: {normalized * 100}% 지점");
    }

    // 인터페이스 의무 구현 (필요 없으면 비워둠)
    public void ClickExit() { }
    public void ClickCancle() { }
    public void HoverEnter() { }
    public void HoverExit() { }
}