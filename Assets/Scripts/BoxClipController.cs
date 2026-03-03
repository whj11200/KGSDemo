using UnityEngine;

public class GlobalBoxClipController : MonoBehaviour
{
    public Vector3 boxCenter;
    public Vector3 boxSize = new Vector3(10, 5, 10);
    public float sectionThickness = 0.02f;
    BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;

        // 상단 절반 트리거
        boxCollider.size = new Vector3(
            boxSize.x,
            boxSize.y * 0.5f,
            boxSize.z
        );

        boxCollider.center = new Vector3(
            boxCenter.x,
            boxCenter.y + boxSize.y * 0.25f,
            boxCenter.z
        );
    }

    void Update()
    {
        Vector3 half = boxSize * 0.5f;
        Vector3 boxMin = boxCenter - half;
        Vector3 boxMax = boxCenter + half;

        Shader.SetGlobalVector("_BoxMin", boxMin);
        Shader.SetGlobalVector("_BoxMax", boxMax);
        Shader.SetGlobalFloat("_SectionThickness", sectionThickness);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
#endif
}
