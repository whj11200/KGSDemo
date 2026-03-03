using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum StructureType
{
    Miniature,
    Field,
}

[RequireComponent(typeof(MeshCollider))]
public class StructureComp : MonoBehaviour, IMouseInteractable
{
    [SerializeField] MeshRenderer[] meshes;
    [SerializeField] Transform viewPos;
    [SerializeField] Color HoverColor;
    [SerializeField] Color OriginColor;
    public Transform viewPoint;
    private string baseColorProp = "_BaseColor";
    private string baseColorPropMax = "_BASE_COLOR";

    // 최초 씬 로드 시에만 할당, 플레이어 등 오브젝트 접근용
    public static StructureParent StructureParent;
    public StructureType structureType;
    public int structID;
    public static bool isFieldLoaded = false;

    private void Awake()
    {
        if (meshes == null || meshes.Length == 0)
            meshes = GetComponentsInChildren<MeshRenderer>();

        //var mat = meshes[0].material;
        //foreach(var props in mat.GetPropertyNames(MaterialPropertyType.Vector))
        //{
        //    Debug.Log($"StructureComp-{gameObject.name} Material Property Vector: {props}");
        //}

        SetOriginColor();
        HoverColor = StructureParent.GetStructureParent(structureType).HoverColor;

        var ID = gameObject.name.Split('-');
    }

    public Vector3 GetViewPos()
    {
        var movePos = viewPos.position;
        movePos.y += 0.5f;
        return movePos;
    }

    public void HoverEnter()
    {
        if (!isFieldLoaded) return;

        if (StructureParent.cachedStruct == this)
            return;
        SetColor(HoverColor);
    }

    public void HoverExit()
    {
        if (!isFieldLoaded) return;

        if (StructureParent.cachedStruct == this)
            return;

        SetColor(OriginColor);
    }

    public void ClickEnter()
    {
        if (!isFieldLoaded) return;
    }

    public void ToggleMesh(bool isOn)
    {
        foreach (var mesh in meshes)
        {
            mesh.enabled = isOn;
        }
    }

    public void ClickExit()
    {
        if (!isFieldLoaded) return;
        SetColor(OriginColor);

        // 실제 필드에서의 동작 : 다른 건물로 이동 요청
        if (structureType == StructureType.Field)
        {
            var CurrentTowerName = StructureParent.cachedStruct != null ? StructureParent.cachedStruct.gameObject.name : "null";
 
            if (StructureParent.cachedStruct == this)
                return;

            StructureParent.RequestMove(this);
        }
        // 미니어처에서의 동작 : 필드에서 ID가 같은 건물로 이동 요청
        else if (structureType == StructureType.Miniature)
        {
            Debug.Log($"StructureComp-{gameObject.name} Miniature Clicked");
            StructureParent.RequestMove(structID);
        }
    }

    public void ClickCancle()
    {
        SetColor(OriginColor);
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

    private void SetOriginColor()
    {
        if (meshes.Length > 0)
        {
            var baseMat = meshes[0].material;   

            if (baseMat.HasProperty(baseColorPropMax))
            {
                OriginColor = baseMat.GetColor(baseColorPropMax);
            }
            else if (baseMat.HasProperty(baseColorProp))
            {
                OriginColor = baseMat.GetColor(baseColorProp);
            }
            else
            {
                OriginColor = baseMat.color;
            }
        }
    }
}
