using UnityEngine;

public class Pipe_Pin : MonoBehaviour,IMouseInteractable
{
    [SerializeField] PipeInterestion pipeInterestion;
    [SerializeField] MeshRenderer[] meshes;
    [SerializeField] Color HoverColor;
    [SerializeField] Color OriginColor;
    private string baseColorProp = "_BaseColor";
    private string baseColorPropMax = "_BASE_COLOR";
    private void OnEnable()
    {
        if (meshes == null || meshes.Length == 0)
            meshes = GetComponentsInChildren<MeshRenderer>();
    }
    private void SetColor(Color color)
    {
        var baseMat = meshes[0].material;

        if (baseMat.HasProperty(baseColorPropMax))
        {
            meshes[0].material.SetColor(baseColorPropMax, color);
        }
        else if (baseMat.HasProperty(baseColorProp))
        {
            meshes[0].material.SetColor(baseColorProp, color);
        }
        else
        {
            meshes[0].material.color = color;
        }
    }
    public void ClickCancle()
    {
        SetColor(OriginColor);
    }

    public void ClickEnter()
    {
        pipeInterestion.TogglePipeState();
        SetColor(HoverColor);
    }

    public void ClickExit()
    {
        SetColor(OriginColor);
    }

    public void HoverEnter()
    {
        SetColor(HoverColor);
    }
    

    public void HoverExit()
    {
        SetColor(OriginColor);
    }
}
