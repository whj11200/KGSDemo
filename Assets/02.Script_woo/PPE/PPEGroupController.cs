using UnityEngine;
using System.Collections.Generic;

public class PPEGroupController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private PPEGroup targetGroup;

    [Header("Parts to Manage")]
    [SerializeField] private List<PPEPartController> partsInGroup = new List<PPEPartController>();

    [Header("UI Settings")]
    [SerializeField] private GameObject returnButton; // "반납하기" 버튼 오브젝트


    [SerializeField] EnvironmentManager manager;

    bool isClear = false;

    private void Start()
    {
        // 시작할 때는 버튼을 숨깁니다.
        if (returnButton != null)
            returnButton.SetActive(false);
    }

    private void Update()
    {
        if (returnButton == null) return;

        // 그룹 내의 장비들이 "모두" 장착되었는지 확인합니다.
        bool allEquipped = CheckAllEquipped();

        // 모두 입었다면 버튼을 활성화, 아니면 비활성화
        if (allEquipped && !returnButton.activeSelf)
        {
            returnButton.SetActive(true);
            if (!isClear && manager != null)
            {
                isClear = true; // "나 보고했다"고 잠금
                manager.CompleteMission(KGS_EnvEventType.PPE_Clear); // PPE 전용 Enum이 있다면 사용
                Debug.Log("[PPE] 모든 장비 장착 완료! 매니저에게 보고함.");
            }
        }
        else if (!allEquipped && returnButton.activeSelf)
        {
            returnButton.SetActive(false);
        }
    }

    private bool CheckAllEquipped()
    {
        // 리스트가 비어있으면 false
        if (partsInGroup.Count == 0) return false;

        foreach (var part in partsInGroup)
        {
            // 하나라도 안 입었으면 false 리턴
            if (part != null && !part.IsEquipped)
            {
                return false;
            }
        }
        // 반복문을 다 돌면 모두 입었다는 뜻!
        return true;
    }

    public void UnequipAllInGroup()
    {
        foreach (var part in partsInGroup)
        {
            if (part != null && part.IsEquipped)
            {
                part.Unequip();
            }
        }
        Debug.Log($"{targetGroup} 전체 반납 완료");
    }
}