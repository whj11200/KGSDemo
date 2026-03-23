using TMPro;
using UnityEngine;

public abstract class BaseEquippable : MonoBehaviour
{
    [Header("Base Settings")]
    public Transform handPos; // 플레이어 손 위치
    public GameObject item;   // 이동시킬 '자식' 혹은 '특정' 오브젝트
    public TextMeshPro infoText; // 화면에 띄울 3D 텍스트 (또는 가이드)

    // [본체 상태 저장]
    protected Vector3 originPosition;
    protected Quaternion originRotation;
    protected Vector3 originScale;
    protected Transform originParent;

    // [아이템(자식) 상태 저장]
    protected Vector3 itemOriginPos;
    protected Quaternion itemOriginRot;
    protected Vector3 itemOriginScale;
    protected Transform itemOriginParent;

    public bool isEquipped { get; protected set; } = false;
    public bool isEquipped_Child { get; protected set; } = false;

    protected virtual void Awake()
    {

        infoText.gameObject.SetActive(false);
        // 1. 나 자신의 초기 상태 기억
        originPosition = transform.position;
        originRotation = transform.rotation;
        originScale = transform.localScale;
        originParent = transform.parent;

        // 2. 만약 지정된 'item'이 있다면 그 아이템의 초기 상태도 기억
        if (item != null)
        {
            itemOriginPos = item.transform.position;
            itemOriginRot = item.transform.rotation;
            itemOriginScale = item.transform.localScale;
            itemOriginParent = item.transform.parent;
        }
    }

    // --- [기능 1: 나 자신을 이동] ---
    public void My_ToggleEquip()
    {
        if (isEquipped) Drop();
        else Equip();
    }

    public virtual void Equip()
    {
        if (handPos == null) return;
        isEquipped = true;
        transform.SetParent(handPos);
        SetToHand(transform);
    }

    public virtual void Drop()
    {
        isEquipped = false;
        transform.SetParent(originParent);
        transform.position = originPosition;
        transform.rotation = originRotation;
        transform.localScale = originScale;
    }

    // --- [기능 2: 특정 아이템(자식)만 이동] ---
    public void Child_ToggleEquip()
    {
        if (item == null) return;
        if (isEquipped_Child) C_Drop();
        else C_Equip();
    }

    public virtual void C_Equip()
    {
        if (handPos == null || item == null) return;
        isEquipped_Child = true;
        item.transform.SetParent(handPos);
        SetToHand(item.transform);
        ToggleText(true);
    }

    public virtual void C_Drop()
    {
        if (item == null) return;
        isEquipped_Child = false;
        item.transform.SetParent(itemOriginParent);
        item.transform.position = itemOriginPos;
        item.transform.rotation = itemOriginRot;
        item.transform.localScale = itemOriginScale;
        ToggleText(false);
    }

    // 공통 로직: 손 위치로 보낼 때 좌표 초기화
    private void SetToHand(Transform target)
    {
        target.localPosition = Vector3.zero;
        target.localRotation = Quaternion.identity;
        target.localScale = target.localScale;
    }

    private void ToggleText(bool show)
    {
        if (infoText != null)
        {
            infoText.gameObject.SetActive(show);
            // 필요하다면 여기서 텍스트 내용을 바꿀 수도 있습니다.
            // infoText.text = show ? "장착됨" : ""; 
        }
    }
}