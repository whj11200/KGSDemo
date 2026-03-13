using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

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
    [Header("Group")]
    [SerializeField] private PPEGroup group = PPEGroup.Type1;

    [Header("Suit Parts")]
    [SerializeField] private SuitPart[] suitParts;

    [Header("Optional")]
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private GameObject hair;

    [Header("Debug")]
    [SerializeField] private MeshRenderer p_meshRender;

    [Header("상태창 UI")]
    [SerializeField] private GameObject statusWindow;
    [SerializeField] private Image statusImage;
    [SerializeField] private float statusShowTime = 2f;
    [SerializeField] private float statusFadeTime = 0.4f;

    private bool isSuitEquipped = false;
    private Coroutine statusWindowRoutine;

    private void Awake()
    {
        if (p_meshRender == null)
            p_meshRender = GetComponentInChildren<MeshRenderer>();

        foreach (SuitPart part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                part.originalMesh = part.targetRenderer.sharedMesh;
                part.originalMaterial = part.targetRenderer.sharedMaterial;
            }
        }

        if (statusImage == null && statusWindow != null)
            statusImage = statusWindow.GetComponent<Image>();

        if (statusWindow != null)
            statusWindow.SetActive(false);
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
            ShowBlockedStatusWindow();
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
        foreach (SuitPart part in suitParts)
        {
            if (part.targetRenderer != null)
            {
                if (part.suitMesh != null)
                    part.targetRenderer.sharedMesh = part.suitMesh;

                if (part.suitMaterial != null)
                    part.targetRenderer.sharedMaterial = part.suitMaterial;
            }
        }

        if (hair != null)
            hair.SetActive(false);

        RefreshRigging();
        Debug.Log("방호복 착용 상태로 변경");
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

    private void ShowBlockedStatusWindow()
    {
        if (statusWindow == null || statusImage == null)
            return;

        if (statusWindowRoutine != null)
            StopCoroutine(statusWindowRoutine);

        statusWindowRoutine = StartCoroutine(CoShowBlockedStatusWindow());
    }

    private IEnumerator CoShowBlockedStatusWindow()
    {
        statusWindow.SetActive(true);

        Color color = statusImage.color;
        color.a = 0.5f;
        statusImage.color = color;

        yield return new WaitForSeconds(statusShowTime);

        float t = 0f;

        while (t < statusFadeTime)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, t / statusFadeTime);
            statusImage.color = color;
            yield return null;
        }

        color.a = 0f;
        statusImage.color = color;

        statusWindow.SetActive(false);
        statusWindowRoutine = null;
    }
}