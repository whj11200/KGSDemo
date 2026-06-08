using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIBoxClipBinder : MonoBehaviour
{
    [Header("Clip Source")]
    [SerializeField] private BoxCollider clipBoxCollider;

    [Header("UI Targets")]
    [SerializeField] private Material boxClipMaterial;
    [SerializeField] private List<Graphic> targetGraphics = new();

    [Header("Option")]
    [SerializeField] private bool useBoxClip = true;

    private readonly List<Material> materialInstances = new();

    private void OnEnable()
    {
        CreateMaterialInstances();
    }

    private void OnDisable()
    {
        ClearMaterialInstances();
    }

    private void LateUpdate()
    {
        ApplyClipData();
    }

    [ContextMenu("Collect Child Graphics")]
    private void CollectChildGraphics()
    {
        targetGraphics.Clear();
        GetComponentsInChildren(true, targetGraphics);
    }

    private void CreateMaterialInstances()
    {
        ClearMaterialInstances();

        if (boxClipMaterial == null)
        {
            Debug.LogWarning($"{name}: boxClipMaterial이 비어있음");
            return;
        }

        foreach (Graphic graphic in targetGraphics)
        {
            if (graphic == null) continue;

            Material instance = new Material(boxClipMaterial);
            instance.name = $"{boxClipMaterial.name}_Instance_{graphic.name}";

            graphic.material = instance;
            materialInstances.Add(instance);
        }

        ApplyClipData();
    }

    private void ApplyClipData()
    {
        if (clipBoxCollider == null)
            return;

        Transform boxTransform = clipBoxCollider.transform;

        Matrix4x4 worldToLocal = boxTransform.worldToLocalMatrix;
        Vector3 center = clipBoxCollider.center;
        Vector3 halfSize = clipBoxCollider.size * 0.5f;

        foreach (Material mat in materialInstances)
        {
            if (mat == null) continue;

            mat.SetFloat("_UseBoxClip", useBoxClip ? 1f : 0f);
            mat.SetMatrix("_ClipBoxWorldToLocal", worldToLocal);
            mat.SetVector("_ClipBoxCenter", center);
            mat.SetVector("_ClipBoxHalfSize", halfSize);
        }
    }

    private void ClearMaterialInstances()
    {
        foreach (Material mat in materialInstances)
        {
            if (mat == null) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(mat);
            else
                Destroy(mat);
#else
            Destroy(mat);
#endif
        }

        materialInstances.Clear();
    }
}