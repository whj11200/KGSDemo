using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[Serializable]
public class SuitPart
{
    public string partName;
    public SkinnedMeshRenderer targetRenderer;
    public Mesh suitMesh;
    public Material suitMaterial;

    [HideInInspector] public Mesh originalMesh;
    [HideInInspector] public Material originalMaterial;
}
public class PPEOneSuit : MonoBehaviour
{
    [SerializeField] private SuitPart[] suitParts;
    [SerializeField] private RigBuilder rigBuilder; // 인스펙터에서 RigBuilder를 할당하세요.
    [SerializeField] GameObject OxygenTank; // 산소통 게임 오브젝트 참조
    [SerializeField] GameObject BackPack; // 백팩 게임 오브젝트 참조
    private bool isSuitEquipped = false; // 현재 방호복 착용 여부

    private void Awake()
    {
        //RefreshRigging();
        // 시작 시 현재 상태(평상복) 저장
        foreach (SuitPart part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.originalMesh = part.targetRenderer.sharedMesh;
                part.originalMaterial = part.targetRenderer.sharedMaterial;
            }
        }
    }

    // 호출할 때마다 상태를 반전시키는 토글 함수
    public void ToggleSuit()
    {
        if (isSuitEquipped)
        {
            RestoreOriginalSuit();
        }
        else
        {
            ChangeToLevelASuit();
        }

        // 상태값 반전 (true -> false, false -> true)
        isSuitEquipped = !isSuitEquipped;

    }

    private void ChangeToLevelASuit()
    {
        foreach (SuitPart part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.targetRenderer.sharedMesh = part.suitMesh;
                part.targetRenderer.sharedMaterial = part.suitMaterial;
            }
        }
        BackPack.SetActive(true); // 백팩 활성화
        OxygenTank.SetActive(true); // 산소통 활성화
        Debug.Log("방호복 착용 상태로 변경");
    }
    private void RefreshRigging()
    {
        if (rigBuilder != null)
        {
            // 리깅의 데이터 스트림을 초기화하고 다시 구축합니다.
            // 이 과정을 거쳐야 "TransformStreamHandle" 에러가 사라집니다.
            rigBuilder.Build();
            Debug.Log("리깅 캐시가 갱신되었습니다.");
        }
    }
    private void RestoreOriginalSuit()
    {
        foreach (SuitPart part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.targetRenderer.sharedMesh = part.originalMesh;
                part.targetRenderer.sharedMaterial = part.originalMaterial;
            }
        }
        BackPack.SetActive(false);
        OxygenTank.SetActive(false);
        Debug.Log("원래 의상으로 복구 완료");
    }


}
