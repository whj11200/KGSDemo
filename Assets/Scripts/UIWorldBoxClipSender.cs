using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIWorldBoxClipSender : MonoBehaviour
{
    [Header("Clip Box")]
    [SerializeField] private BoxCollider clipBox;

    [Header("Target UI Root")]
    [SerializeField] private Transform targetRoot;

    [Header("Target Images")]
    [SerializeField] private Image[] targetImages;

    [Header("Material")]
    [SerializeField] private Material clipMaterialTemplate;
    [SerializeField] private bool createMaterialInstances = true;

    [Header("Options")]
    [SerializeField] private bool includeInactive = true;

    private static readonly int WorldToClipBoxID = Shader.PropertyToID("_WorldToClipBox");
    private static readonly int BoxSizeID = Shader.PropertyToID("_BoxSize");

    private void OnEnable()
    {
        if (clipBox == null)
            clipBox = GetComponent<BoxCollider>();

        RefreshImages();
        PrepareMaterials();
        UpdateClipBox();
    }

    private void Update()
    {
        UpdateClipBox();
    }

    [ContextMenu("Refresh Images")]
    public void RefreshImages()
    {
        if (targetRoot == null)
            return;

        targetImages = targetRoot.GetComponentsInChildren<Image>(includeInactive);
    }

    private void PrepareMaterials()
    {
        if (targetImages == null)
            return;

        foreach (Image image in targetImages)
        {
            if (image == null)
                continue;

            Material source = clipMaterialTemplate != null
                ? clipMaterialTemplate
                : image.material;

            if (source == null)
                continue;

            if (createMaterialInstances)
            {
                Material instance = Application.isPlaying
                    ? Instantiate(source)
                    : new Material(source);

                instance.name = source.name + "_UI_Instance";

                if (!Application.isPlaying)
                    instance.hideFlags = HideFlags.DontSave;

                image.material = instance;
            }
            else
            {
                image.material = source;
            }

            image.SetMaterialDirty();
        }
    }

    private void UpdateClipBox()
    {
        if (clipBox == null || targetImages == null)
            return;

        Transform boxTransform = clipBox.transform;

        // BoxCollider의 center까지 반영
        Matrix4x4 worldToLocal = boxTransform.worldToLocalMatrix;
        Matrix4x4 centerOffset = Matrix4x4.Translate(-clipBox.center);
        Matrix4x4 worldToClipBox = centerOffset * worldToLocal;

        Vector4 boxSize = new Vector4(
            clipBox.size.x,
            clipBox.size.y,
            clipBox.size.z,
            0f
        );

        foreach (Image image in targetImages)
        {
            if (image == null || image.material == null)
                continue;

            Material mat = image.material;

            mat.SetMatrix(WorldToClipBoxID, worldToClipBox);
            mat.SetVector(BoxSizeID, boxSize);

            image.SetMaterialDirty();
        }
    }
}