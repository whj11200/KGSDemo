using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

[Serializable]
public class Datas
{
    public string partName;
    public SkinnedMeshRenderer targetRenderer;
    public Mesh suitMesh;
    public Material[] suitMaterials;

    [HideInInspector] public Mesh originalMesh;
    [HideInInspector] public Material[] originalMaterials;
}

public class PPEOneSuit : MonoBehaviour
{
    [Header("Group")]
    [SerializeField] private PPEGroup group = PPEGroup.Type1;

    [Header("Suit Parts")]
    [SerializeField] private SuitParts[] suitParts;

    [Header("Optional")]
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private GameObject hair;

    [Header("Debug")]
    [SerializeField] private MeshRenderer p_meshRender;
    [Header("Script")]
    [SerializeField] private MessageUI messageUI;

    private bool isSuitEquipped = false;
    private Coroutine statusWindowRoutine;

    private void Awake()
    {
        if (p_meshRender == null)
            p_meshRender = GetComponentInChildren<MeshRenderer>();

        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.originalMesh = part.targetRenderer.sharedMesh;
                part.originalMaterials = part.targetRenderer.sharedMaterials; // 복수형으로 변경
            }
        }
    }

    public void ToggleSuit()
    {
        if (isSuitEquipped)
            UnequipSuit();
        else
            EquipSuit();
    }

    public void EquipSuit()
    {
        if (isSuitEquipped)
            return;

        // 반대 그룹이 입고 있으면 착용 막기
        if (!PPEGroupManager.CanEquip(group))
        {
            Debug.Log($"{name} 장착 불가: 반대 그룹 장비를 먼저 모두 벗어야 함");
            messageUI.ShowMessage("다른 형식의 보호구가 착용 중입니다. 먼저 모두 해제하세요.");
            return;
        }

        ChangeToLevelASuit();
        isSuitEquipped = true;
        PPEGroupManager.Register(group);

        if (p_meshRender != null)
            p_meshRender.enabled = false;

        Debug.Log($"[{name}] 방호복 착용 완료");
    }

    public void UnequipSuit()
    {
        if (!isSuitEquipped)
            return;

        RestoreOriginalSuit();
        isSuitEquipped = false;
        PPEGroupManager.Unregister(group);

        if (p_meshRender != null)
            p_meshRender.enabled = true;

        Debug.Log($"[{name}] 방호복 해제 완료");
    }

    private void ChangeToLevelASuit()
    {
        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                if (part.suitMesh != null)
                    part.targetRenderer.sharedMesh = part.suitMesh;

                // suitMaterials 배열이 비어있지 않은지 확인 후 적용
                if (part.suitMaterials != null && part.suitMaterials.Length > 0)
                    part.targetRenderer.sharedMaterials = part.suitMaterials;
            }
        }

        if (hair != null)
            hair.SetActive(false);

        RefreshRigging();
        Debug.Log("방호복 착용 상태로 변경");
    }

    private void RestoreOriginalSuit()
    {
        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.targetRenderer.sharedMesh = part.originalMesh;
                part.targetRenderer.sharedMaterials = part.originalMaterials; // 복수형으로 변경
            }
        }

        if (hair != null)
            hair.SetActive(true);

        RefreshRigging();
        Debug.Log("원래 의상으로 복구 완료");
    }

    private void RefreshRigging()
    {
        if (rigBuilder != null)
        {
            rigBuilder.Build();
            Debug.Log("리깅 캐시가 갱신되었습니다.");
        }
    }

    public bool IsSuitEquipped()
    {
        return isSuitEquipped;
    }

    public PPEGroup GetGroup()
    {
        return group;
    }




}