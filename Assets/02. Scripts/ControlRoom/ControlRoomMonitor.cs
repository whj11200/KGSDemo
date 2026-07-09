using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlRoomMonitor : MonoBehaviour, IControlRoomMonitor
{
    [SerializeField] GameObject BlinkButton;
    [SerializeField] Image BlinkIconImage;

    [SerializeField] GameObject PopupPanel;
    [SerializeField] Image BackImage;
    [SerializeField] Image ScreenImage;
    [SerializeField] TextMeshProUGUI PopupText;

    [SerializeField] List<Image> WarnigPoints = new();

    [SerializeField] ControlScenarioPlayer scenarioPlayer;

    public event Action OnProcessBtn;
    int WPIndex = 0;

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Initialize()
    {
        BackImage.enabled = false;

        if (ScreenImage != null)
        {
            ScreenImage.enabled = false;
        }

        AlarmBlinkRoutine = null;
        WPBlinkRoutine = null;

        foreach (var item in WarnigPoints)
        {
            item.gameObject.SetActive(false);
        }

        AlarmNodeId = -1;
    }

    public void SetScreenInfo(Sprite sprite, int wpIndex, string Loc)
    {
        WPIndex = wpIndex;

        BackImage.enabled = true;
        ScreenImage.sprite = sprite;
        ScreenImage.enabled = true;

        PopupText.text = $"OO관리소입니다. {Loc}에서 가스 누출이 확인되었습니다.가스 검지기 측정 결과 23.2% LFL입니다.\r\n" +
                        $"대응 바랍니다.";
    }

    public void OpenPopup()
    {
        PopupPanel.SetActive(true);
    }

    private Coroutine AlarmBlinkRoutine = null;
    private int AlarmNodeId = -1;
    public void BlinkIcon(int idx)
    {
        AlarmNodeId = idx;
        BlinkButton.SetActive(true);
        BlinkIconImage.enabled = false;

        if (AlarmBlinkRoutine != null)
        {
            StopCoroutine(AlarmBlinkRoutine);
            AlarmBlinkRoutine = null;
        }

        StartCoroutine(Blink(BlinkIconImage));    
    }

    public void OnClickAlarm()
    {
        scenarioPlayer.CheckStep(AlarmNodeId);
    }

    IEnumerator Blink(Image image)
    {
        while (true)
        {
            image.enabled = !image.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Coroutine WPBlinkRoutine = null;
    public void ShowWaringPoint(int nodeID)
    {
        var WP = WarnigPoints[WPIndex];

        WP.gameObject.SetActive(true);
        WP.enabled = false;

        if (WPBlinkRoutine != null)
        {
            StopCoroutine(WPBlinkRoutine);
            WPBlinkRoutine = null;
        }

        WPBlinkRoutine = StartCoroutine(Blink(WP));
    }

    public void ShowValves()
    {
        StopCoroutine(WPBlinkRoutine);

        var WP = WarnigPoints[WPIndex];
        WP.enabled = true;
    }
}
