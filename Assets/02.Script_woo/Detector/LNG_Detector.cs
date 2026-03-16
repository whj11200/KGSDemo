using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LNG_Detector : MonoBehaviour
{
    [Header("UI & Target")]
    public GameObject detectorCanvas;
    public TextMeshProUGUI valueText;
    public GameObject waterLeak; // 여기에 밸브(ValveController 붙은 객체)를 연결

    [Header("Settings")]
    public float detectionRadius = 10f;
    public Transform handPos;
    public float dropSpeed = 0.5f; // 가스가 사라지는 속도

    [Header("Input Reference")]
    public InputActionReference dropActionReference;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private Transform originParent;
    private bool isEquipped = false;

    // 현재 측정되는 내부 수치 (0~1)
    private float currentMeasuredValue = 0f;
    [SerializeField] ValveController valve;
    [SerializeField] MessageUI messageUI;
    void Awake()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
        originParent = transform.parent;
        detectorCanvas.SetActive(false);
    }

    // ... [OnEnable, OnDisable, OnDropPerformed 코드는 동일함] ...
    void OnEnable()

    {

        // Reference가 할당되어 있다면 이벤트 연결 및 액션 활성화

        if (dropActionReference != null)

        {

            dropActionReference.action.Enable();

            dropActionReference.action.performed += OnDropPerformed;

        }

    }



    void OnDisable()

    {

        // 이벤트 연결 해제 및 액션 비활성화

        if (dropActionReference != null)

        {

            dropActionReference.action.performed -= OnDropPerformed;

            dropActionReference.action.Disable();

        }

    }



    // E키(또는 지정된 키)가 눌렸을 때 실행될 콜백

    private void OnDropPerformed(InputAction.CallbackContext context)

    {

        if (isEquipped)

        {

            DropDetector();

        }

    }
    void Update()
    {
        if (isEquipped)
        {
            UpdateDetection();
        }
    }

    public void TurnOnAndEquip()
    {
        isEquipped = true;
        transform.SetParent(handPos);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        detectorCanvas.SetActive(true);
        messageUI.ShowMessage("장비 습득 완료 (E키를 눌러 반납)");
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
        valueText.text = $"CH4: {steppedValue:F1}";

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

    void DropDetector()
    {
        isEquipped = false;
        detectorCanvas.SetActive(false);
        valueText.color = Color.white;
        transform.SetParent(originParent);
        transform.position = originPosition;
        transform.rotation = originRotation;
    }
}