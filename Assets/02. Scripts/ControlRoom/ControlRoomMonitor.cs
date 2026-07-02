using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlRoomMonitor : MonoBehaviour
{
    [SerializeField] GameObject BlinkButton;
    [SerializeField] Image BlinkIconImage;

    [SerializeField] GameObject PopupPanel;
    [SerializeField] Image BackImage;
    [SerializeField] Image ScreenImage;
    [SerializeField] TextMeshProUGUI PopupText;

    [SerializeField] List<Image> WarnigPoints = new();

    [SerializeField] GameObject infoPanel;
    [SerializeField] TextMeshProUGUI info_Loc;
    int WPIndex = 0;

    private void Awake()
    {
        BackImage.enabled = false;
        ScreenImage.enabled = false;

        foreach(var item in WarnigPoints)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (ScreenImage != null)
        {
            ScreenImage.enabled = false;
        }

        AlarmBlinkRoutine = null;
        WPBlinkRoutine = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetScreenInfo(Sprite sprite, int wpIndex, string Loc)
    {
        WPIndex = wpIndex;

        BackImage.enabled = true;
        ScreenImage.sprite = sprite;
        ScreenImage.enabled = true;

        info_Loc.text = $"누출지점: <color=#FF0000>{Loc}</color>";
    }

    public void SetPopupText(string text)
    {
        PopupText.text = text;
    }

    public void OpenPopup()
    {
        PopupPanel.SetActive(true);
    }

    private Coroutine AlarmBlinkRoutine = null;
    public void BlinkIcon()
    {
        BlinkButton.SetActive(true);
        BlinkIconImage.enabled = true;

        if (AlarmBlinkRoutine != null)
        {
            StopCoroutine(AlarmBlinkRoutine);
            AlarmBlinkRoutine = null;
        }

        StartCoroutine(Blink(BlinkIconImage));    
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
    public void ShowWaringPoint()
    {
        var WP = WarnigPoints[WPIndex];

        WP.gameObject.SetActive(true);
        WP.enabled = true;

        if (WPBlinkRoutine != null)
        {
            StopCoroutine(WPBlinkRoutine);
            WPBlinkRoutine = null;
        }

        WPBlinkRoutine = StartCoroutine(Blink(WP));
    }
}
