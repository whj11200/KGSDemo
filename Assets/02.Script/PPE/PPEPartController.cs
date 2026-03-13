using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using System.Collections;

public enum PPEPartMode
{
    MeshMaterialSwap,
    GameObjectOnly,
    Mixed
}

public enum PPEGroup
{
    None,
    Type1,   // 왼쪽
    Type34   // 오른쪽
}

public class PPEPartController : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private string partName;
    [SerializeField] private PPEPartMode mode;
    [SerializeField] private PPEGroup group;

    [Header("Renderer Swap")]
    [SerializeField] private SkinnedMeshRenderer targetRenderer;
    [SerializeField] private Mesh equippedMesh;
    [SerializeField] private Material equippedMaterial;

    [Header("GameObject Toggle")]
    [SerializeField] private GameObject targetObject;

    [Header("Optional")]
    [SerializeField] private RigBuilder rigBuilder;

    [Header("Debug")]
    [SerializeField] private MeshRenderer p_meshRenderer;

    [Header("상태창 UI")]
    [SerializeField] private GameObject statusWindow;
    [SerializeField] private Image statusImage;
    [SerializeField] private float statusShowTime = 2f;
    [SerializeField] private float statusFadeTime = 0.4f;

    private Mesh originalMesh;
    private Material originalMaterial;
    private bool isEquipped;
    private Coroutine statusRoutine;

    public PPEGroup Group => group;
    public bool IsEquipped => isEquipped;

    private void Awake()
    {
        if (targetRenderer != null)
        {
            originalMesh = targetRenderer.sharedMesh;
            originalMaterial = targetRenderer.sharedMaterial;
        }

        if (p_meshRenderer == null)
            p_meshRenderer = GetComponent<MeshRenderer>();

        if (statusWindow != null)
            statusWindow.SetActive(false);

        if (statusImage == null && statusWindow != null)
            statusImage = statusWindow.GetComponent<Image>();
    }

    public void Equip()
    {
        // 반대 그룹이 입혀져 있으면 장착 금지
        if (!PPEGroupManager.CanEquip(group))
        {
            Debug.Log($"{partName} 장착 불가: 반대 그룹 장비를 먼저 모두 벗어야 함");
            ShowBlockedStatusWindow();
            return;
        }

        if (isEquipped)
            return;

        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                ApplyRenderer();
                break;

            case PPEPartMode.GameObjectOnly:
                SetObjectActive(true);
                break;

            case PPEPartMode.Mixed:
                ApplyRenderer();
                SetObjectActive(true);
                break;
        }

        isEquipped = true;
        PPEGroupManager.Register(group);
        RefreshRig();
        Debug.Log($"{partName} 착용");

        if (p_meshRenderer != null)
            p_meshRenderer.enabled = false;
    }

    public void Unequip()
    {
        if (!isEquipped)
            return;

        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                RestoreRenderer();
                break;

            case PPEPartMode.GameObjectOnly:
                SetObjectActive(false);
                break;

            case PPEPartMode.Mixed:
                RestoreRenderer();
                SetObjectActive(false);
                break;
        }

        isEquipped = false;
        PPEGroupManager.Unregister(group);
        RefreshRig();
        Debug.Log($"{partName} 해제");

        if (p_meshRenderer != null)
            p_meshRenderer.enabled = true;
    }

    public void TogglePart()
    {
        if (isEquipped) Unequip();
        else Equip();
    }

    private void ApplyRenderer()
    {
        if (targetRenderer == null) return;

        if (equippedMesh != null)
            targetRenderer.sharedMesh = equippedMesh;

        if (equippedMaterial != null)
            targetRenderer.sharedMaterial = equippedMaterial;
    }

    private void RestoreRenderer()
    {
        if (targetRenderer == null) return;

        targetRenderer.sharedMesh = originalMesh;
        targetRenderer.sharedMaterial = originalMaterial;
    }

    private void SetObjectActive(bool active)
    {
        if (targetObject != null)
            targetObject.SetActive(active);
    }

    private void RefreshRig()
    {
        if (rigBuilder != null)
            rigBuilder.Build();
    }

    private void ShowBlockedStatusWindow()
    {
        if (statusWindow == null || statusImage == null)
            return;

        if (statusRoutine != null)
            StopCoroutine(statusRoutine);

        statusRoutine = StartCoroutine(CoShowStatusWindow());
    }

    private IEnumerator CoShowStatusWindow()
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
        statusRoutine = null;
    }
}