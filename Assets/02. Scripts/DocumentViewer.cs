using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocumentViewer : MonoBehaviour
{
    [SerializeField] TMP_Text Title;
    [SerializeField] Image Tableholder;
    [SerializeField] Image DocImage;
    [SerializeField] List<TableEntry> TableEntries;
    [SerializeField] float Spacing = 25f;
    [SerializeField] float TableEntryWidth = 200f;
    [SerializeField] CameraController CameraController;

    private DocumentData CurrentData;
    private int currentPageIndex = 0;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetDocument(DocumentData data)
    {
        if (data == null) return;

        CurrentData = data;

        Title.text = data.Title;

        for (int i = 0; i < TableEntries.Count; i++) 
        {
            TableEntries[i].SetContent(data.Responsibilitys[i]);
        }

        bool isVersionC = data.DocumentVersion == 'C';
        var tableWidth = isVersionC ? TableEntryWidth * 2 + Spacing : TableEntryWidth * 3 + Spacing * 2;

        Tableholder.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tableWidth);
        TableEntries[^1].gameObject.SetActive(!isVersionC);

        if (data.Pages != null && data.Pages.Count > 0)
        {
            DocImage.sprite = data.Pages[0];
            currentPageIndex = 0;
        }
    }

    public void NextPage()
    {
        if (CurrentData == null || CurrentData.Pages == null || CurrentData.Pages.Count == 0) return;

        currentPageIndex++;
        if (currentPageIndex >= CurrentData.Pages.Count)
        {
            currentPageIndex = 0; // Loop back to the first page
        }

        DocImage.sprite = CurrentData.Pages[currentPageIndex];  
    }

    public void PrevPage()
    {
        if (CurrentData == null || CurrentData.Pages == null || CurrentData.Pages.Count == 0) return;

        currentPageIndex--;

        if (currentPageIndex < 0)
        {
            currentPageIndex = CurrentData.Pages.Count - 1; // Loop back to the last page
        }

        DocImage.sprite = CurrentData.Pages[currentPageIndex];
    }

    bool IsLockState = true;
    public void ToggleScreenMoniotrMode()
    {
        IsLockState = !IsLockState;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        CameraController.SetMoveLockState(false);
    }
}
