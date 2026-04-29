using UnityEngine;
[ExecuteAlways] // <-- 핵심: 에디터 모드에서도 실행되게 합니다.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RopeTubeRenderer : MonoBehaviour
{
    [Header("Source Rope")]
    public Rope rope; //  Rope 스크립트 참조

    [Header("Tube Settings")]
    [Min(3)] public int sides = 10;      // 단면 분할 수 (8~14 추천)
    public float radius = 0.05f;         // 로프 반지름
    public float uvTiling = 2f;          // 길이 방향 UV 타일

    [Header("Twist (Visual)")]
    public float twistPerMeter = 6f;     // 계류줄 느낌: 미터당 회전량(라디안 아님, "회전 횟수" 느낌)

    Mesh mesh;
    Vector3[] verts;
    Vector3[] norms;
    Vector2[] uvs;
    int[] tris;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "RopeTubeMesh";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    void LateUpdate()
    {
        if (!rope || rope.Nodes == null || rope.NodeCount < 2) return;
        Build(rope.Nodes, rope.NodeCount);
    }

    void Build(Vector3[] nodes, int count)
    {
        int ringVerts = sides;
        int vertCount = count * ringVerts;
        int triCount = (count - 1) * sides * 2 * 3; // 2 triangles per quad

        EnsureArrays(vertCount, triCount);

        // 길이 누적 (UV + twist용)
        float accLen = 0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = nodes[i];
            Vector3 forward =
                (i == count - 1) ? (nodes[i] - nodes[i - 1]) : (nodes[i + 1] - nodes[i]);

            if (forward.sqrMagnitude < 1e-8f) forward = Vector3.forward;
            forward.Normalize();

            // 기준 Up을 만들기 위한 안정적인 프레임
            // worldUp과 거의 평행하면 다른 축 사용
            Vector3 worldUp = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(forward, worldUp)) > 0.95f) worldUp = Vector3.right;

            Vector3 right = Vector3.Cross(worldUp, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            // 길이 누적
            if (i > 0) accLen += Vector3.Distance(nodes[i - 1], nodes[i]);

            // 계류줄 “꼬임” 연출 (렌더링만)
            float twist = accLen * twistPerMeter * Mathf.PI * 2f; // meters * turns -> radians
            float cosT = Mathf.Cos(twist);
            float sinT = Mathf.Sin(twist);

            // 단면 링 생성
            for (int s = 0; s < sides; s++)
            {
                float a = (s / (float)sides) * Mathf.PI * 2f;
                float ca = Mathf.Cos(a);
                float sa = Mathf.Sin(a);

                // 원 단면 벡터 (right/up) + twist 적용
                Vector3 dir = (right * ca + up * sa);
                Vector3 dirTwisted = (right * (ca * cosT - sa * sinT)) + (up * (ca * sinT + sa * cosT));
                dirTwisted.Normalize();

                int v = i * ringVerts + s;
                verts[v] = transform.InverseTransformPoint(p) + dirTwisted * radius;
                norms[v] = dirTwisted;

                float u = s / (float)sides;
                float vCoord = (accLen * uvTiling);
                uvs[v] = new Vector2(u, vCoord);
            }
        }

        // 삼각형 인덱스
        int t = 0;
        for (int i = 0; i < count - 1; i++)
        {
            int i0 = i * ringVerts;
            int i1 = (i + 1) * ringVerts;

            for (int s = 0; s < sides; s++)
            {
                int s0 = s;
                int s1 = (s + 1) % sides;

                int a = i0 + s0;
                int b = i1 + s0;
                int c = i1 + s1;
                int d = i0 + s1;

                // a-b-c, a-c-d
                tris[t++] = a; tris[t++] = b; tris[t++] = c;
                tris[t++] = a; tris[t++] = c; tris[t++] = d;
            }
        }

        mesh.Clear(false);
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
    }

    void EnsureArrays(int vCount, int triIndexCount)
    {
        if (verts == null || verts.Length != vCount)
        {
            verts = new Vector3[vCount];
            norms = new Vector3[vCount];
            uvs = new Vector2[vCount];
        }
        if (tris == null || tris.Length != triIndexCount)
        {
            tris = new int[triIndexCount];
        }
    }
}
