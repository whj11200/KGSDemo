using UnityEngine;

public class RegionalmapInteraction : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float topY = 0.215f;    // 초기 위치 (올라가 있을 때)
    [SerializeField] private float bottomY = -0.0584f; // 내려왔을 때 위치
    [SerializeField] private float smoothSpeed = 10f;  // 이동 속도 (높을수록 빠름)

    private bool isMapActive = false; // true면 내려온 상태(Active), false면 올라간 상태
    private Vector3 targetPosition;

    void Start()
    {
        // 시작 시 초기 위치(올라간 상태)로 설정
        transform.localPosition = new Vector3(transform.localPosition.x, topY, transform.localPosition.z);
        targetPosition = transform.localPosition;
    }

    void Update()
    {
        // 매 프레임마다 목표 위치로 부드럽게 이동
        Vector3 currentPos = transform.localPosition;
        transform.localPosition = Vector3.Lerp(currentPos, targetPosition, Time.deltaTime * smoothSpeed);
    }

    /// <summary>
    /// 호출할 때마다 맵을 올리거나 내립니다.
    /// </summary>
    public void ToggleMap()
    {
        isMapActive = !isMapActive; // 상태 반전

        // 목표 Y값 결정
        float targetY = isMapActive ? bottomY : topY;
        targetPosition = new Vector3(transform.localPosition.x, targetY, transform.localPosition.z);

        Debug.Log($"Map State: {(isMapActive ? "Down (Active)" : "Up (Inactive)")}");
    }

    // 명시적으로 끄거나 켜는 함수
    public void SetMapState(bool isActive)
    {
        isMapActive = isActive;
        float targetY = isMapActive ? bottomY : topY;
        targetPosition = new Vector3(transform.localPosition.x, targetY, transform.localPosition.z);
    }
}