using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlRoomMonitor : MonoBehaviour
{
    [SerializeField] GameObject BlinkButton;
    [SerializeField] Image BlinkIconImage;

    [SerializeField] GameObject PopupPanel;
    [SerializeField] Image ScreenImage;
    [SerializeField] TextMeshProUGUI PopupText;

    private void OnEnable()
    {
        if (ScreenImage != null)
        {
            ScreenImage.enabled = false;
        }

        blinkRoutine = null;
    }

    private void OnDisable()
    {
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = null;
    }

    public void SetScreenImage(Sprite sprite)
    {
        ScreenImage.sprite = sprite;
        ScreenImage.enabled = true;
    }

    public void SetPopupText(string text)
    {
        PopupText.text = text;
    }

    public void OpenPopup()
    {
        PopupPanel.SetActive(true);
    }

    private Coroutine blinkRoutine = null;
    public void BlinkIcon()
    {
        BlinkButton.SetActive(true);
        BlinkIconImage.enabled = true;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        StartCoroutine(Blink());    
    }

    IEnumerator Blink()
    {
        while (true)
        {
            BlinkIconImage.enabled = !BlinkIconImage.enabled;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
