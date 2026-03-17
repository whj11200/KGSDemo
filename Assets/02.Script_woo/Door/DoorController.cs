using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Objects")]
    [SerializeField] private Transform doorL;
    [SerializeField] private Transform doorR;

    [Header("콜라이더")]
    [SerializeField] private Collider doorCollider; 
    [Header("Settings")]
    [SerializeField] private float openAngle = 90f;   
    [SerializeField] private float smoothTime = 2.0f; 
    [SerializeField] bool canOpen = false;                     //튜토리얼 클리어 여부 임시 

    [Header("UI Reference")]
    [SerializeField] MessageUI messageUI;     // 아까 만든 MessageUI 연결

    private bool isOpen = false;
    private Coroutine doorRoutine;
    

    // 하위 오브젝트(마우스 인터페이스 등)에서 클릭 시 이 함수를 호출하게 하세요.
    public void RequestToggleDoor()
    {
        if (canOpen)
        {
            // 튜토리얼 클리어 상태면 문을 토글
            isOpen = !isOpen;

            if (doorRoutine != null) StopCoroutine(doorRoutine);
            doorRoutine = StartCoroutine(CoMoveDoor());
        }
        else
        {
            // 클리어 전이면 메시지 출력
            if (messageUI != null)
            {
                messageUI.ShowMessage("안내가 끝나야 문을 열 수 있습니다.");
            }
        }
    }

    private IEnumerator CoMoveDoor()
    {
        doorCollider.enabled = false; // 문이 움직이는 동안 콜라이더 비활성화
        // 목표 각도 설정 (Left는 -90, Right는 90)
        float targetLAngle = isOpen ? -openAngle : 0f;
        float targetRAngle = isOpen ? openAngle : 0f;

        Quaternion targetRotL = Quaternion.Euler(-90, 0, targetLAngle);
        Quaternion targetRotR = Quaternion.Euler(-90, 0, targetRAngle);

        while (Quaternion.Angle(doorL.localRotation, targetRotL) > 0.01f)
        {
            doorL.localRotation = Quaternion.Slerp(doorL.localRotation, targetRotL, Time.deltaTime * smoothTime);
            doorR.localRotation = Quaternion.Slerp(doorR.localRotation, targetRotR, Time.deltaTime * smoothTime);
            yield return null;
        }

        // 최종 각도 보정
        doorL.localRotation = targetRotL;
        doorR.localRotation = targetRotR;
    }
}