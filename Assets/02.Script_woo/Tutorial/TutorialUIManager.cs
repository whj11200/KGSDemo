using UnityEngine;
using TMPro;

public class ManagerUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject guidePanel;    // 가이드 부모 판넬
    [SerializeField] private TextMeshProUGUI guideText; // 안내 문구

    private void Awake()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
    }

    public void OnCatchingObject()
    {
        Debug.Log("[TutorialUI] 잡기 가이드 활성화");
        ShowGuide("마우스 좌클릭으로 선택");
     
    }

    public void OnScroll()
    {
        Debug.Log("[TutorialUI] 줌인아웃 가이드 활성화");
        ShowGuide("마우스 스크롤로 줌인/줌아웃");
    }

    public void OnClear()
    {
        Debug.Log("[TutorialUI] 최종 가이드 활성화");
        ShowGuide("문으로 가서 이동하기");
    }

    public void ShowGuide(string message)
    {
        if (guidePanel == null) return;
        guideText.text = message;
        guidePanel.SetActive(true);
    }

    public void HideGuide()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
    }
}