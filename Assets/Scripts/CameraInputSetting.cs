using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraInputSetting : MonoBehaviour
{
    public Slider rotateSlider;
    public Slider moveSlider;

    public TMP_InputField rotateInput;
    public TMP_InputField moveInput;

    [SerializeField] private float curRotVal;
    [SerializeField] private float curMoveVal;

    [SerializeField] private float moveSpeedMin = 1f;
    [SerializeField] private float moveSpeedMax = 10f;

    [SerializeField] private float rotateSpeedMin = 1f;
    [SerializeField] private float rotateSpeedMax = 100f;

    [SerializeField] CameraController controller;

    private void Awake()
    {
        // 슬라이더 범위 설정
        rotateSlider.minValue = rotateSpeedMin;
        rotateSlider.maxValue = rotateSpeedMax;
        moveSlider.minValue = moveSpeedMin;
        moveSlider.maxValue = moveSpeedMax;

        // UI 초기화
        LoadCurrentValues();
        BindEvents();
    }

    private void BindEvents()
    {
        // 슬라이더 → 입력 필드
        rotateSlider.onValueChanged.AddListener(val =>
        {
            curRotVal = Mathf.Clamp(val, rotateSpeedMin, rotateSpeedMax);
            rotateInput.text = curRotVal.ToString("F1");
        });

        moveSlider.onValueChanged.AddListener(val =>
        {
            curMoveVal = Mathf.Clamp(val, moveSpeedMin, moveSpeedMax);
            moveInput.text = curMoveVal.ToString("F1");
        });

        // 입력 필드 → 슬라이더 (필터링 포함)
        rotateInput.onValueChanged.AddListener(text =>
        {
            text = FilterNumericInput(text);
            rotateInput.text = text;

            if (float.TryParse(text, out float v))
            {
                v = Mathf.Clamp(v, rotateSpeedMin, rotateSpeedMax);
                curRotVal = v;
                rotateSlider.SetValueWithoutNotify(v);
            }
        });

        moveInput.onValueChanged.AddListener(text =>
        {
            text = FilterNumericInput(text);
            moveInput.text = text;

            if (float.TryParse(text, out float v))
            {
                v = Mathf.Clamp(v, moveSpeedMin, moveSpeedMax);
                curMoveVal = v;
                moveSlider.SetValueWithoutNotify(v);
            }
        });
    }

    // 숫자/소수점/부호 입력 필터링
    private string FilterNumericInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string filtered = "";
        bool hasDecimal = false;

        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                filtered += c;
            }
            else if (c == '.' && !hasDecimal)
            {
                filtered += c;
                hasDecimal = true;
            }
        }

        return filtered;
    }

    private void LoadCurrentValues()
    {
        float prefsRotSpeed = PlayerPrefs.HasKey("RotSpeed") ? PlayerPrefs.GetFloat("RotSpeed") : 2f;
        float prefsMoveSpeed = PlayerPrefs.HasKey("MoveSpeed") ? PlayerPrefs.GetFloat("MoveSpeed") : 3.5f;

        curRotVal = Mathf.Clamp(prefsRotSpeed, rotateSpeedMin, rotateSpeedMax);
        curMoveVal = Mathf.Clamp(prefsMoveSpeed, moveSpeedMin, moveSpeedMax);

        controller.SetInputParam(curRotVal, curMoveVal);

        rotateSlider.SetValueWithoutNotify(curRotVal);
        moveSlider.SetValueWithoutNotify(curMoveVal);

        rotateInput.SetTextWithoutNotify(curRotVal.ToString("F1"));
        moveInput.SetTextWithoutNotify(curMoveVal.ToString("F1"));
    }

    public void Apply()
    {
        PlayerPrefs.SetFloat("RotSpeed", curRotVal);
        PlayerPrefs.SetFloat("MoveSpeed", curMoveVal);
        PlayerPrefs.Save();

        controller.SetInputParam(curRotVal, curMoveVal);

        controller.isPopupOpened = false;
        gameObject.SetActive(false);
    }

    public void Cancel()
    {
        LoadCurrentValues();
        controller.isPopupOpened = false;
        gameObject.SetActive(false);
    }
}