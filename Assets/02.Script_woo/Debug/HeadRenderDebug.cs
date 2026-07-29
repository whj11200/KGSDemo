using UnityEngine;
using UnityEngine.Rendering;

public class HeadRenderDebug : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer headRenderer;

    private void Awake()
    {
        if (headRenderer == null)
            headRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void LateUpdate()
    {
        if (headRenderer == null)
            return;

        headRenderer.enabled = true;
        headRenderer.forceRenderingOff = false;
        headRenderer.shadowCastingMode = ShadowCastingMode.On;

        // 스크립트가 설정한 MaterialPropertyBlock 제거
        headRenderer.SetPropertyBlock(null);
    }
}