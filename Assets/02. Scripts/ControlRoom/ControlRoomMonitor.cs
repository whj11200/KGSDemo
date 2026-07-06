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

    [SerializeField] GameObject infoPanel;
    [SerializeField] TextMeshProUGUI info_Loc;
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
        infoPanel.SetActive(false);
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

            var btn = item.GetComponentInChildren<Button>();
            btn.interactable = true;

            btn.GetComponent<Image>().raycastTarget = true;
        }

        AlarmNodeId = -1;
        WPNodeID = -1;
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
    private int WPNodeID = -1;
    public void ShowWaringPoint(int nodeID)
    {
        WPNodeID = nodeID; 
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

    public void OnClickWP()
    {
        scenarioPlayer.CheckStep(WPNodeID);
    }

    public void ShowValves()
    {
        StopCoroutine(WPBlinkRoutine);

        var WP = WarnigPoints[WPIndex];
        WP.enabled = true;

        var btn = WP.GetComponentInChildren<Button>();
        btn.interactable = false;
        btn.GetComponent<Image>().raycastTarget = false;
    }
}
