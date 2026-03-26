using UnityEngine;

// 이 클래스는 직접 오브젝트에 붙이기보다 상속용으로 사용하므로 추상 클래스(abstract)로 선언하는 것이 좋습니다.
// (선택사항이지만 권장합니다. 직접 붙이려면 abstract를 지우세요.)
public abstract class Minimapfuntioni : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] protected Material mt_material; // 자식 클래스에서도 접근 가능하도록 protected로 변경

    // 머티리얼의 고유 Emission 색상을 저장할 변수
    private Color originalEmissionColor;
    // 현재 Emission이 켜져 있는지 여부
    private bool isEmissionOn = false;

    // Emission 색상을 제어하기 위한 셰이더 속성 ID (성능 향상을 위해 ID 사용)
    private static readonly int EmissionColorPropertyId = Shader.PropertyToID("_EmissionColor");

    protected virtual void Awake()
    {
        InitializeEmission();
    }

    // 1. 초기화: 머티리얼의 원래 Emission 색상을 기억하고 초기 상태 설정
    private void InitializeEmission()
    {
        if (mt_material == null)
        {
            // 인스펙터에서 머티리얼이 할당되지 않았을 때를 대비한 방어 코드
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                mt_material = renderer.material;
            }
        }

        if (mt_material != null)
        {
            // 셰이더가 Emission을 지원하는지 확인
            if (mt_material.HasProperty(EmissionColorPropertyId))
            {
                // 현재 머티리얼에 설정된 Emission 색상을 가져와 저장
                originalEmissionColor = mt_material.GetColor(EmissionColorPropertyId);

                // 시작할 때 Emission을 끄고 싶다면 아래 주석을 해제하세요.
                // SetEmission(false); 
            }
            else
            {
                //Debug.LogWarning($"[MinimapInterestion] {gameObject.name}의 머티리얼 셰이더가 _EmissionColor 속성을 지원하지 않습니다.");
            }
        }
        else
        {
            //Debug.LogError($"[MinimapInterestion] {gameObject.name}에 머티리얼이 할당되지 않았습니다.");
        }
    }

    // 2. 외부(또는 자식 클래스)에서 호출할 핵심 함수: Emission 상태 설정
    public void SetEmission(bool isOn)
    {
        if (mt_material == null || !mt_material.HasProperty(EmissionColorPropertyId)) return;

        isEmissionOn = isOn;

        if (isEmissionOn)
        {
            // 켜기: 원래 색상을 적용
            mt_material.SetColor(EmissionColorPropertyId, originalEmissionColor);
            // 셰이더 키워드를 활성화해야 적용되는 경우도 있습니다 (예: Legacy Shaders)
            mt_material.EnableKeyword("_EMISSION");
        }
        else
        {
            // 끄기: 검은색을 적용 (빛나지 않음)
            mt_material.SetColor(EmissionColorPropertyId, Color.black);
            // 셰이더 키워드 비활성화
            mt_material.DisableKeyword("_EMISSION");
        }

        // 유니티 터미널에서 변화를 즉시 반영하도록 강제 (필요시)
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mt_material);
#endif
    }

    public void ToggleEmission()
    {
        SetEmission(!isEmissionOn);
    }
}