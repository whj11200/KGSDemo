using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using System.Collections;
using System;

// 1. 여러 부위를 묶어서 관리하기 위한 클래스
[Serializable]
public class SuitParts
{
    
    public SkinnedMeshRenderer targetRenderer;
    public Mesh suitMesh;
    public Material[] suitMaterials;

    [HideInInspector] public Mesh originalMesh;
    [HideInInspector] public Material[] originalMaterials;
}

public enum PPEPartMode
{
    MeshMaterialSwap, // 리스트에 등록된 메쉬/마테리얼 교체
    GameObjectOnly,   // 오브젝트 활성/비활성만
    Mixed             // 리스트 교체 + 오브젝트 활성/비활성
}

public enum PPEGroup
{
    None,
    Type1,   // 왼쪽
    Type34   // 오른쪽
}

public class PPEPartController : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private PPEPartMode mode;
    [SerializeField] private PPEGroup group;

    [Header("Suit Parts List")]
    // PPEOneSuit처럼 리스트로 관리
    [SerializeField] private SuitParts[] suitParts;

    [Header("GameObject Toggle")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private GameObject returnText;
    [SerializeField] private GameObject hair;

    [Header("Optional")]
    [SerializeField] private RigBuilder rigBuilder;

    [Header("Debug/UI")]
    [SerializeField] private MeshRenderer p_meshRenderer;
    [SerializeField] private MessageUI messageUI;

    private bool isEquipped = false;

    public PPEGroup Group => group;
    public bool IsEquipped => isEquipped;

    private void Awake()
    {
        // 원본 메쉬 및 마테리얼 배열 저장 (PPEOneSuit 방식)
        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.originalMesh = part.targetRenderer.sharedMesh;
                part.originalMaterials = part.targetRenderer.sharedMaterials;
            }
        }

        if (p_meshRenderer == null)
            p_meshRenderer = GetComponent<MeshRenderer>();

        if (returnText == null && transform.childCount > 0)
            returnText = transform.GetChild(0).gameObject;

        if (returnText != null) returnText.SetActive(false);
    }

    public void TogglePart()
    {
        if (isEquipped) Unequip();
        else Equip();
    }

    public void Equip()
    {
        if (isEquipped) return;
        if (group == PPEGroup.Type1 && hair != null)
        {
            hair.SetActive(false);
        }
        // 그룹 체크
        if (!PPEGroupManager.CanEquip(group))
        {
            if (messageUI != null)
                messageUI.ShowMessage("다른 형식의 보호구가 착용 중입니다. 먼저 모두 해제하세요.");
            return;
        }

        // 모드에 따른 실행
        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                ApplySuitParts();
                break;
            case PPEPartMode.GameObjectOnly:
                SetObjectActive(true);
                break;
            case PPEPartMode.Mixed:
                ApplySuitParts();
                SetObjectActive(true);
                break;
        }

        isEquipped = true;
        PPEGroupManager.Register(group);
        RefreshRig();

        if (p_meshRenderer != null) p_meshRenderer.enabled = false;
        if (returnText != null) returnText.SetActive(true);
    }

    public void Unequip()
    {
        if (!isEquipped) return;
        if (group == PPEGroup.Type1 && hair != null)
        {
            hair.SetActive(true);
        }
        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                RestoreSuitParts();
                break;
            case PPEPartMode.GameObjectOnly:
                SetObjectActive(false);
                break;
            case PPEPartMode.Mixed:
                RestoreSuitParts();
                SetObjectActive(false);
                break;
        }

        isEquipped = false;
        PPEGroupManager.Unregister(group);
        RefreshRig();

        if (p_meshRenderer != null) p_meshRenderer.enabled = true;
        if (returnText != null) returnText.SetActive(false);
    }

    // --- 내부 로직 (PPEOneSuit 로직 이식) ---

    private void ApplySuitParts()
    {
        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer == null) continue;

            if (part.suitMesh != null)
                part.targetRenderer.sharedMesh = part.suitMesh;

            if (part.suitMaterials != null && part.suitMaterials.Length > 0)
                part.targetRenderer.sharedMaterials = part.suitMaterials;
        }
    }

    private void RestoreSuitParts()
    {
        foreach (SuitParts part in suitParts)
        {
            if (part.targetRenderer == null) continue;

            part.targetRenderer.sharedMesh = part.originalMesh;
            part.targetRenderer.sharedMaterials = part.originalMaterials;
        }
    }

    private void SetObjectActive(bool active)
    {
        if (targetObject != null) targetObject.SetActive(active);
    }

    private void RefreshRig()
    {
        if (rigBuilder != null)
        {
            rigBuilder.Build();
        }
    }
}