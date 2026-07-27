using UnityEngine;
using UnityEngine.UI;

public class PlumbingSpecifications : MonoBehaviour, IMouseInteractable
{
    [Header("Script")]
    [SerializeField] private Teleporter teleporter;

    [Header("Pump")]
    [SerializeField]PipeInterestion pipeInterestion;

    [Header("Button")]
    [SerializeField] private Button uiButton;
    private Image uiButtonImage;

    [Header("UI")]
    [SerializeField] private GameObject uiPanel;

    [Header("Color")]
    [SerializeField] private Color originColor;
    [SerializeField] private Color hoverColor = Color.green;

    [Header("State")]
    [SerializeField] private bool isOpen;

    public bool IsOpen => isOpen;
    public Button Button => uiButton;

    private void Awake()
    {
        if (uiButton != null)
        {
            uiButtonImage = uiButton.GetComponent<Image>();

            if (uiButtonImage != null)
                originColor = uiButtonImage.color;
        }

     
    }

    //public void ShowUI()
    //{
    //    isOpen = true;

    //    if (uiPanel != null)
    //        uiPanel.SetActive(true);
    //}

    //public void HideUI()
    //{
    //    isOpen = false;

    //    if (uiPanel != null)
    //        uiPanel.SetActive(false);
    //}

    //public void ToggleUI()
    //{
    //    if (isOpen)
    //        HideUI();
    //    else
    //        ShowUI();
    //}

    public void HoverEnter()
    {
        if (uiButtonImage != null)
            uiButtonImage.color = hoverColor;
    }

    public void HoverExit()
    {
        if (uiButtonImage != null)
            uiButtonImage.color = originColor;
    }

    public void ClickEnter()
    {
        teleporter.RequestPipeSpecification();
   
    }

    public void ClickExit()
    {
    }

    public void ClickCancle()
    {
    }
}