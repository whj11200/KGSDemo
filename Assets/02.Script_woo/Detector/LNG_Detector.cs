using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LNG_Detector : MonoBehaviour
{
    [Header("UI & Target")]
    public GameObject detectorCanvas;
    public TextMeshProUGUI valueText;

    [Header("References")]
    public GameObject waterLeak; // 여기에 밸브(ValveController 붙은 객체)를 연결
    [SerializeField] GameObject returnText; // 반납텍스트
    [Header("Settings")]
    public float detectionRadius = 10f; // r감지거리
    public Transform handPos; // 손 위치를 나타내는 Transform (예: Player의 자식 Handpos)
    public float dropSpeed = 0.5f; // 가스가 사라지는 속도
    private Vector3 originPosition; // 원래 위치 저장
    private Quaternion originRotation; // 원래 회전 저장
    private Vector3 originScaleMode; // 원래 스케일 모드 저장 (필요 시)
    private Transform originParent; // 원래 부모 저장
    private bool isEquipped = false; // 착용 여부
    private float currentMeasuredValue = 0f; // 현재 측정된 수치 (0.1 ~ 1.0)
    [SerializeField] ValveController valve; // 밸브 컨트롤러 참조
    void Awake()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
        originScaleMode = transform.localScale; // 필요 시 스케일 모드 저장
        originParent = transform.parent;

        detectorCanvas.SetActive(false);
        if(returnText != null )returnText.SetActive(false);
    }
    void Update()
    {
        if (isEquipped)
        {
            UpdateDetection();
        }
    }
    public void ToggleEquip()
    {
        if (isEquipped) DropDetector();
        else TurnOnAndEquip();
    }
    private void TurnOnAndEquip()
    {
        isEquipped = true;

        // 1. 손으로 이동 및 부모 변경
        transform.SetParent(handPos);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(1f, 1f, 1f); // 필요 시 스케일 조정

        // 2. UI 및 텍스트 처리
        detectorCanvas.SetActive(true);
        if (returnText != null) returnText.SetActive(true); // "반납하기" 표시
    }

    void UpdateDetection()
    {
        if (waterLeak == null || valve == null) return;

        float distance = Vector3.Distance(transform.position, waterLeak.transform.position);
        float targetValue = 0f;

        // 1. 목표 수치 설정
        if (valve.isLeaking)
        {
            // 밸브가 새고 있을 때 (거리 기반 0.1 ~ 1.0)
            targetValue = Mathf.Clamp(1 - (distance / detectionRadius), 0.1f, 1.0f);
        }
        else
        {
            // 밸브가 잠겨도 최솟값은 0.1로 고정
            targetValue = 0.1f;
        }

        // 2. 현재 수치를 목표 수치로 부드럽게 이동
        currentMeasuredValue = Mathf.MoveTowards(currentMeasuredValue, targetValue, dropSpeed * Time.deltaTime);

        // 3. 0.1 단위로 끊기
        float steppedValue = Mathf.Round(currentMeasuredValue * 10f) / 10f;

        // 4. 최솟값 강제 고정 (한 번 더 확실하게)
        if (steppedValue < 0.1f) steppedValue = 0.1f;

        // 5. UI 업데이트
        valueText.text = $"{steppedValue}";

        // 6. 상태별 색상 처리
        if (steppedValue >= 1.0f)
        {
            OnMaxDetection();
        }
        else
        {
            // 0.1 상태일 때는 위험하지 않으므로 흰색 유지
            valueText.color = Color.white;
        }
    }
    void OnMaxDetection()
    {
        valueText.color = Color.red;
    }

    private void DropDetector()
    {
        isEquipped = false;

        // 1. 원래 부모(Station) 밑으로 복귀 및 위치 초기화
        transform.SetParent(originParent);
        transform.position = originPosition;
        transform.rotation = originRotation;
        transform.localScale = originScaleMode; // 필요 시 스케일 초기화
        // 2. UI 및 텍스트 처리
        detectorCanvas.SetActive(false);
        if (returnText != null) returnText.SetActive(false); // "반납하기" 숨김
        valueText.color = Color.white;
    }
}