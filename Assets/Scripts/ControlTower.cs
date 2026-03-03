using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ControlTower : MonoBehaviour, IMouseInteractable
{
    public static GridObjectPlacer Grid;
    // public Vector2 selfPos;

    [SerializeField] MeshRenderer[] meshes;
    [SerializeField] Transform viewPos;
    // [SerializeField] GameObject ButtonParent;

    [SerializeField] Color HoverColor;
    [SerializeField] Color OriginColor;

    private void Awake()
    {
        if (meshes == null || meshes.Length == 0) 
            meshes = GetComponentsInChildren<MeshRenderer>();

        if (meshes.Length > 0)
        {
            OriginColor = meshes[0].material.color;
        }

        // 다른 컨트롤 타워를 직접 클릭해서 이동시키는 방식으로 변경
        //foreach (var button in ButtonParent.GetComponentsInChildren<Button>())
        //{
        //    button.onClick.RemoveAllListeners();
        //    button.onClick.AddListener(() =>
        //    {
        //        Vector2 dir = Vector2.zero;
        //        switch (button.name)
        //        {
        //            case "Up":
        //                dir = Vector2.up;
        //                break;
        //            case "Down":
        //                dir = Vector2.down;
        //                break;
        //            case "Left":
        //                dir = Vector2.left;
        //                break;
        //            case "Right":
        //                dir = Vector2.right;
        //                break;
        //            case "UpLeft":
        //                dir = Vector2.up + Vector2.left;
        //                break;
        //            case "UpRight":
        //                dir = Vector2.up + Vector2.right;
        //                break;
        //            case "DownLeft":
        //                dir = Vector2.down + Vector2.left;
        //                break;
        //            case "DownRight":
        //                dir = Vector2.down + Vector2.right;
        //                break;
        //        }

        //        MoveOtherControlTower(dir);
        //    });
        //}
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        ToggleMesh(false);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        ToggleMesh(true);
    //    }
    //}

    public void ToggleMesh(bool isOn)
    {
        foreach (var mesh in meshes)
        {
            mesh.enabled = isOn;
        }
    }

    //public void MoveOtherControlTower(Vector2 direction)
    //{
    //    // direction 방향의 다른 ControlTower 이동
    //    // 상하좌우/대각선 한 칸씩 이동
    //    Debug.Log($"Request MoveOtherControlTower: CurName={gameObject.name} Direction={direction}");
    //    Debug.Log($"Current pos : {viewPos.position}");
    //    Instance.MoveOtherControlTower(selfPos, direction);
    //}

    public Vector3 GetViewPos()
    {
        var movePos = viewPos.position;
        movePos.y += 0.5f;
        return movePos;
    }

    public void HoverEnter()
    {
        if (Grid.CurrentTower == this)
            return;
        meshes[0].material.color = HoverColor;
    }

    public void HoverExit()
    {
        if (Grid.CurrentTower == this)
            return;
        meshes[0].material.color = OriginColor;
    }

    public void ClickEnter()
    {
        
    }

    public void ClickExit()
    {
        var CurrentTowerName = Grid.CurrentTower != null ? Grid.CurrentTower.gameObject.name : "null";
        Debug.Log($"ControlTower-{gameObject.name} to Grid Cached Tower-{CurrentTowerName}");

        if (Grid.CurrentTower == this)
            return;
        meshes[0].material.color = OriginColor;
        Grid.RequestMove(this);
    }

    public void ClickCancle()
    {
        meshes[0].material.color = OriginColor;
    }
}