using UnityEngine;
using UnityEditor;

public class PivotEditorTool : EditorWindow
{
    [MenuItem("Tools/Custom Pivot Tool")]
    public static void ShowWindow()
    {
        GetWindow<PivotEditorTool>("Pivot Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("선택한 오브젝트의 피벗 변경 (Undo 지원)", EditorStyles.boldLabel);

        if (GUILayout.Button("피벗을 중앙(Center)으로"))
        {
            AdjustPivot(new Vector3(0.5f, 0.5f, 0.5f));
        }

        if (GUILayout.Button("피벗을 바닥(Bottom)으로"))
        {
            AdjustPivot(new Vector3(0.5f, 0f, 0.5f));
        }
    }

    void AdjustPivot(Vector3 pivotRatio)
    {
        GameObject target = Selection.activeGameObject;
        if (target == null || target.GetComponent<MeshFilter>() == null)
        {
            Debug.LogWarning("MeshFilter가 있는 오브젝트를 선택해주세요.");
            return;
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        // --- 핵심: Undo 기록 시작 ---
        // Transform과 MeshFilter 두 가지의 변경 사항을 모두 기록합니다.
        Undo.RecordObjects(new Object[] { target.transform, meshFilter }, "Adjust Pivot");

        // 메시 데이터를 복사
        Mesh newMesh = Instantiate(mesh);
        newMesh.name = mesh.name + "_Pivoted";
        Vector3[] vertices = newMesh.vertices;
        Bounds bounds = newMesh.bounds;

        Vector3 offset = new Vector3(
            Mathf.Lerp(bounds.min.x, bounds.max.x, pivotRatio.x),
            Mathf.Lerp(bounds.min.y, bounds.max.y, pivotRatio.y),
            Mathf.Lerp(bounds.min.z, bounds.max.z, pivotRatio.z)
        );

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= offset;
        }

        newMesh.vertices = vertices;
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();

        // 위치 보정 (오브젝트가 튀는 현상 방지)
        target.transform.position += target.transform.TransformDirection(offset);

        // 변경된 메시 할당
        meshFilter.sharedMesh = newMesh;

        // 에디터에게 변경 사항이 있음을 알림 (저장 필요 표시)
        EditorUtility.SetDirty(target);

        Debug.Log($"{target.name}의 피벗을 수정했습니다. (Ctrl+Z 가능)");
    }
}