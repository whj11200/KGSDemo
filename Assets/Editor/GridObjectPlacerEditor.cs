using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridObjectPlacer))]
public class GridObjectPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridObjectPlacer placer = (GridObjectPlacer)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Objects"))
        {
            Undo.RegisterFullObjectHierarchyUndo(
                placer.gameObject,
                "Generate Grid Objects"
            );

            placer.Generate();
        }

        if (GUILayout.Button("Clear Objects"))
        {
            Undo.RegisterFullObjectHierarchyUndo(
                placer.gameObject,
                "Clear Grid Objects"
            );

            placer.Clear();
        }
    }
}
