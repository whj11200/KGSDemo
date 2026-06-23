using UnityEngine;
using TMPro;

public class CanvasClockTMP : MonoBehaviour
{
    [Header("TMP UI")]
    [SerializeField] private TextMeshProUGUI clockText;

    [Header("Start Time")]
    [SerializeField, Range(0, 23)] private int startHour = 13;
    [SerializeField, Range(0, 59)] private int startMinute = 0;

    [Header("Display")]
    [SerializeField] private bool showSeconds = false;

    private double currentSeconds;

    private void Awake()
    {
        if (clockText == null)
            clockText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        currentSeconds = startHour * 3600 + startMinute * 60;
        UpdateClockText();
    }

    private void Update()
    {
        // 실제 시간처럼 1초씩 흐름
        currentSeconds += Time.unscaledDeltaTime;

        // 24시간 지나면 00:00으로 순환
        currentSeconds %= 24 * 3600;

        UpdateClockText();
    }

    private void UpdateClockText()
    {
        int totalSeconds = Mathf.FloorToInt((float)currentSeconds);

        int hour = totalSeconds / 3600;
        int minute = (totalSeconds % 3600) / 60;
        int second = totalSeconds % 60;

        if (showSeconds)
        {
            clockText.text = $"{hour:00}:{minute:00}:{second:00}";
        }
        else
        {
            clockText.text = $"{hour:00}:{minute:00}";
        }
    }
}