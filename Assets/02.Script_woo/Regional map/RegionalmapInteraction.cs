using UnityEngine;

public class RegionalmapInteraction : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string boolParameterName = "IsOpen"; // Animator에 생성한 Bool 파라미터 이름

    private bool isMapActive = false; // 현재 맵이 올라와 있는지 상태 저장

    /// <summary>
    /// 호출할 때마다 맵을 올리거나 내립니다.
    /// </summary>
    public void ToggleMap()
    {
        isMapActive = !isMapActive; // 상태 반전

        if (animator != null)
        {
            // Animator의 Bool 값을 변경하여 Transition을 제어합니다.
            animator.SetBool(boolParameterName, isMapActive);
        }

        Debug.Log($"Map State: {(isMapActive ? "Opened" : "Closed")}");
    }

    // 필요하다면 명시적으로 끄거나 켜는 함수도 유지할 수 있습니다.
    public void SetMapState(bool isActive)
    {
        isMapActive = isActive;
        animator.SetBool(boolParameterName, isMapActive);
    }
}