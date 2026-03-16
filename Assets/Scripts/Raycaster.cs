using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Raycaster : MonoBehaviour
{
    public float rayDistance = 5f;
    [SerializeField] private IMouseInteractable prevHover;
    [SerializeField] private IMouseInteractable prevClick;
    [SerializeField] private IDragInteractable dragTarget;
    [SerializeField] private LayerMask raycastMask;
    public bool isDragging = false;
    private Mouse mouse;

    // UI 체크를 위한 리스트 및 데이터
    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();

    private void OnEnable() => mouse = Mouse.current;

    private void Update()
    {
        
        if (mouse == null || EventSystem.current == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        bool leftButtonDown = mouse.leftButton.wasPressedThisFrame;
        bool leftButtonUp = mouse.leftButton.wasReleasedThisFrame;

        // 1. 통합 타겟 찾기 (UI + Physics)
        IMouseInteractable currentInteractable = GetTargetAtMouse<IMouseInteractable>(mousePos);
        IDragInteractable currentDraggable = GetTargetAtMouse<IDragInteractable>(mousePos);
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        // 이 레이의 시작점과 방향을 사용해 선을 그립니다.
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        // --- 클릭 로직 ---
        if (leftButtonDown)
        {
            if (currentInteractable != null)
            {
                prevClick = currentInteractable;
                currentInteractable.ClickEnter();
            }

            if (currentDraggable != null)
            {
                dragTarget = currentDraggable;
                dragTarget.DragStart();
                isDragging = true;
            }
        }

        // --- 호버 로직 ---
        if (currentInteractable != prevHover)
        {
            prevHover?.HoverExit();
            currentInteractable?.HoverEnter();
            prevHover = currentInteractable;
        }

        // --- 드래그 로직 ---
        if (isDragging) dragTarget?.Dragging();

        // --- 릴리즈 로직 ---
        if (leftButtonUp)
        {
            if (prevClick != null)
            {
                // 최종 위치 확인 (동일 타겟인지)
                var finalTarget = GetTargetAtMouse<IMouseInteractable>(mousePos);
                if (finalTarget == prevClick) prevClick.ClickExit();
                else prevClick.ClickCancle();

                prevClick = null;
            }

            if (dragTarget != null)
            {
                dragTarget.DragEnd();
                dragTarget = null;
            }
            isDragging = false;
        }
    }

    // UI와 물리 오브젝트를 모두 뒤져서 인터페이스를 찾아주는 함수
    private T GetTargetAtMouse<T>(Vector2 mousePos) where T : class
    {
        pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = mousePos;

        raycastResults.Clear();
        // UI와 Physics Raycaster가 잡는 모든 것을 가져옴
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.distance > rayDistance) continue;

            // 맞은 오브젝트 본인 혹은 부모에게서 인터페이스 찾기
            T target = result.gameObject.GetComponentInParent<T>();
            if (target != null) return target;
        }
        return null;
    }
}