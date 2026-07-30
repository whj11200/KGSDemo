using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[Serializable]
public class SuitParts
{
    public SkinnedMeshRenderer targetRenderer;
    public Mesh suitMesh;
    public Material[] suitMaterials;

    [HideInInspector] public Mesh originalMesh;
    [HideInInspector] public Material[] originalMaterials;
}

[Serializable]
public class DevicePPESetup
{
    [Header("Device")]
    public EPlayDevice device;

    [Header("Mesh / Material Swap")]
    public SuitParts[] suitParts;

    [Header("Activated When Equipped")]
    public GameObject[] targetObjects;

    [Header("Displayed When Equipped")]
    public GameObject[] returnTexts;

    [Header("Hidden When Equipped")]
    public GameObject[] hairObjects;

    [Header("Rig")]
    public RigBuilder rigBuilder;
}

public enum PPEPartMode
{
    MeshMaterialSwap,
    GameObjectOnly,
    Mixed
}

public enum PPEGroup
{
    None,
    Type1,
    Type34
}

public class PPEPartController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource playerSound;
    [SerializeField] private AudioClip getClip;
    [SerializeField] private AudioClip putClip;

    [Header("Basic Settings")]
    [SerializeField] private PPEPartMode mode;
    [SerializeField] private PPEGroup group;

    [Header("Device Settings")]
    [SerializeField] private DevicePPESetup[] deviceSetups;

    [Tooltip("게임 시작 시 모든 기기의 착용 전용 오브젝트를 비활성화합니다.")]
    [SerializeField] private bool initializeTargetObjectsOff = true;

    [Header("Debug / UI")]
    [SerializeField] private MeshRenderer pickupMeshRenderer;
    [SerializeField] private MessageUI messageUI;

    private DevicePPESetup activeSetup;
    private bool isEquipped;

    public PPEGroup Group => group;
    public bool IsEquipped => isEquipped;
    public EPlayDevice ActiveDevice => PlayerDeviceManager.PlayDevice;

    private void Awake()
    {
        activeSetup = FindDeviceSetup(PlayerDeviceManager.PlayDevice);
            CacheReturnTextsFromChildren();
        if (activeSetup == null)
        {
            Debug.LogError(
                $"[{name}] {PlayerDeviceManager.PlayDevice}용 PPE 설정이 없습니다.",
                this
            );

            enabled = false;
            return;
        }

        CacheOriginalSuitParts(activeSetup.suitParts);

        if (pickupMeshRenderer == null)
            pickupMeshRenderer = GetComponent<MeshRenderer>();

        // 모든 기기의 착용 전용 오브젝트 초기화
        if (initializeTargetObjectsOff)
        {
            SetAllDeviceTargetObjects(false);
        }

        // Return Text는 현재 기기 설정만 초기 비활성화
        SetObjectsActive(activeSetup.returnTexts, false);

        Debug.Log(
            $"[{name}] PPE Device Setup: {PlayerDeviceManager.PlayDevice}",
            this
        );
    }

    public void TogglePart()
    {
        if (isEquipped)
            Unequip();
        else
            Equip();
    }

    public void Equip()
    {
        if (isEquipped || activeSetup == null)
            return;

        // 먼저 그룹 착용 가능 여부를 확인해야 한다.
        // 기존 코드는 Hair를 먼저 끈 뒤 검사해서,
        // 착용 실패 시 Hair가 꺼진 채 남을 수 있었다.
        if (!PPEGroupManager.CanEquip(group))
        {
            if (messageUI != null)
            {
                messageUI.ShowMessage(
                    "다른 형식의 보호구가 착용 중입니다. 먼저 모두 해제하세요."
                );
            }

            return;
        }

        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                ApplySuitParts();
                break;

            case PPEPartMode.GameObjectOnly:
                SetTargetObjectsActive(true);
                break;

            case PPEPartMode.Mixed:
                ApplySuitParts();
                SetTargetObjectsActive(true);
                break;
        }

        if (group == PPEGroup.Type1)
        {
            SetObjectsActive(activeSetup.hairObjects, false);
        }

        isEquipped = true;

        PPEGroupManager.Register(group);

        RefreshRig();
        PlaySound(getClip);

        if (pickupMeshRenderer != null)
            pickupMeshRenderer.enabled = false;

        SetObjectsActive(activeSetup.returnTexts, true);
    }

    public void Unequip()
    {
        if (!isEquipped || activeSetup == null)
            return;

        switch (mode)
        {
            case PPEPartMode.MeshMaterialSwap:
                RestoreSuitParts();
                break;

            case PPEPartMode.GameObjectOnly:
                SetTargetObjectsActive(false);
                break;

            case PPEPartMode.Mixed:
                RestoreSuitParts();
                SetTargetObjectsActive(false);
                break;
        }

        if (group == PPEGroup.Type1)
        {
            SetObjectsActive(activeSetup.hairObjects, true);
        }

        isEquipped = false;

        PPEGroupManager.Unregister(group);

        RefreshRig();
        PlaySound(putClip);

        if (pickupMeshRenderer != null)
            pickupMeshRenderer.enabled = true;

        SetObjectsActive(activeSetup.returnTexts, false);
    }

    private DevicePPESetup FindDeviceSetup(EPlayDevice device)
    {
        if (deviceSetups == null)
            return null;

        foreach (DevicePPESetup setup in deviceSetups)
        {
            if (setup != null && setup.device == device)
                return setup;
        }

        return null;
    }

    private void CacheOriginalSuitParts(SuitParts[] parts)
    {
        if (parts == null)
            return;

        foreach (SuitParts part in parts)
        {
            if (part == null || part.targetRenderer == null)
                continue;

            part.originalMesh = part.targetRenderer.sharedMesh;
            part.originalMaterials = part.targetRenderer.sharedMaterials;
        }
    }

    private void ApplySuitParts()
    {
        if (activeSetup.suitParts == null)
            return;

        foreach (SuitParts part in activeSetup.suitParts)
        {
            if (part == null || part.targetRenderer == null)
                continue;

            if (part.suitMesh != null)
                part.targetRenderer.sharedMesh = part.suitMesh;

            if (part.suitMaterials != null &&
                part.suitMaterials.Length > 0)
            {
                part.targetRenderer.sharedMaterials = part.suitMaterials;
            }
        }
    }

    private void RestoreSuitParts()
    {
        if (activeSetup.suitParts == null)
            return;

        foreach (SuitParts part in activeSetup.suitParts)
        {
            if (part == null || part.targetRenderer == null)
                continue;

            if (part.originalMesh != null)
                part.targetRenderer.sharedMesh = part.originalMesh;

            if (part.originalMaterials != null)
            {
                part.targetRenderer.sharedMaterials =
                    part.originalMaterials;
            }
        }
    }

    private void SetTargetObjectsActive(bool active)
    {
        SetObjectsActive(activeSetup.targetObjects, active);
    }

    private void SetAllDeviceTargetObjects(bool active)
    {
        if (deviceSetups == null)
            return;

        foreach (DevicePPESetup setup in deviceSetups)
        {
            if (setup == null)
                continue;

            SetObjectsActive(setup.targetObjects, active);
            SetObjectsActive(setup.returnTexts, false);
        }
    }

    private void SetObjectsActive(
        GameObject[] objects,
        bool active
    )
    {
        if (objects == null)
            return;

        foreach (GameObject target in objects)
        {
            if (target != null)
                target.SetActive(active);
        }
    }

    private void RefreshRig()
    {
        if (activeSetup.rigBuilder != null)
        {
            activeSetup.rigBuilder.Build();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (playerSound == null || clip == null)
            return;

        playerSound.PlayOneShot(clip);
    }
    private void CacheReturnTextsFromChildren()
    {
        int childCount = transform.childCount;

        activeSetup.returnTexts = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            activeSetup.returnTexts[i] =
                transform.GetChild(i).gameObject;
        }
    }
}