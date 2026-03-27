using UnityEngine;

public abstract class Minimapfuntioni : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] protected Material mt_material;

    private Color originalEmissionColor;
    private static readonly int EmissionColorPropertyId = Shader.PropertyToID("_EmissionColor");

    // 상태 관리 변수
    protected bool isHovered = false;
    protected bool isSelected = false;

    protected virtual void Awake()
    {
        InitializeEmission();
    }

    private void InitializeEmission()
    {
        if (mt_material == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null) mt_material = renderer.material;
        }

        if (mt_material != null && mt_material.HasProperty(EmissionColorPropertyId))
        {
            originalEmissionColor = mt_material.GetColor(EmissionColorPropertyId);
            // 초기 상태는 꺼진 상태로 시작
            ApplyEmission(false);
        }
    }

    // 핵심: 상태에 따라 비주얼을 업데이트
    protected void UpdateVisual()
    {
        // 호버 중이거나, 선택된 상태라면 불을 켭니다.
        bool shouldBeOn = isHovered || isSelected;
        ApplyEmission(shouldBeOn);
    }

    private void ApplyEmission(bool isOn)
    {
        if (mt_material == null) return;

        if (isOn)
        {
            mt_material.SetColor(EmissionColorPropertyId, originalEmissionColor);
            mt_material.EnableKeyword("_EMISSION");
        }
        else
        {
            mt_material.SetColor(EmissionColorPropertyId, Color.black);
            mt_material.DisableKeyword("_EMISSION");
        }
    }
}