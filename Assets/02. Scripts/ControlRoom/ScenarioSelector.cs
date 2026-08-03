using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScenarioSelector : MonoBehaviour
{
    [SerializeField] private ControlScenarioPlayer ScenarioPlayer;
    [SerializeField] private int ScenarioIndex;

    [SerializeField] List<Image> Buttons;
    [SerializeField] Color SelectedColor;

    [SerializeField] CinemachineCamera ScenarioCam;
    [SerializeField] CameraSwitcher CameraSwitcher;
    [SerializeField] CameraController CameraController;

    [SerializeField] AudioSource AudioSource;
    [SerializeField] AudioClip ClickClip;

    [SerializeField] Transform VRPlayer;
    [SerializeField] Transform TargetMonitor;
    [SerializeField] GameObject PlayerChair;
    [SerializeField] Transform CameraOffset;
    [SerializeField] Transform StandPos;
    [SerializeField] XRSimpleInteractable SelectModeTrigger;
    [SerializeField] XRTeleportController XRTeleport;

    private void Awake()
    {
        if (PlayerDeviceManager.IsVR)
        {
            CameraController = VRPlayer.GetComponentInChildren<CameraController>();
        }
    }

    private void Start()
    {
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        ScenarioIndex = 0;
    }

    public void SelectScenario(int _chooseIdx)
    {
        AudioSource.PlayOneShot(ClickClip);

        Buttons[ScenarioIndex].color = Color.white;

        ScenarioIndex = _chooseIdx;
        Buttons[ScenarioIndex].color = SelectedColor;
    }

    public void ApplySelect()
    {
        ScenarioPlayer.InitializeScenario(ScenarioIndex);
        gameObject.SetActive(false);
    }
    float prevYOffest;

    // 착석
    public void EnterSelctionMode()
    {
        switch (PlayerDeviceManager.PlayDevice)
        {
            case EPlayDevice.VR:
                XRTeleport.enabled = false;
                CameraController.SetMoveLockState(true);

                SelectModeTrigger.enabled = false;

                // 의자 위치 이동
                VRPlayer.position = PlayerChair.transform.position;

                // 모니터 바라보도록 회전 보정
                Transform head = Camera.main.transform;

                Vector3 headForward = head.forward;
                headForward.y = 0;
                headForward.Normalize();

                Vector3 targetForward = TargetMonitor.position - head.position;
                targetForward.y = 0;
                targetForward.Normalize();

                float angle = Vector3.SignedAngle(
                    headForward,
                    targetForward,
                    Vector3.up
                );

                VRPlayer.Rotate(Vector3.up, angle, Space.World);

                // 카메라 높이 보정
                var deskPos = CameraOffset.localPosition;
                prevYOffest = deskPos.y;

                Vector3 localHead = CameraOffset.InverseTransformPoint(head.position);
                Vector3 localMonitor = CameraOffset.InverseTransformPoint(TargetMonitor.position);

                float deltaY = localMonitor.y - localHead.y;

                float targetDelta = 0.05f;

                deskPos.y += deltaY - targetDelta;

                CameraOffset.localPosition = deskPos;

                PlayerChair.SetActive(false);
                break;

            case EPlayDevice.Desktop:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                CameraController.SetMoveLockState(true);
                CameraSwitcher.SetCamera(ScenarioCam);
                break;
        }
    }

    public void ExitSelectionMode()
    {
        switch (PlayerDeviceManager.PlayDevice)
        {
            case EPlayDevice.VR:
                SelectModeTrigger.enabled = true;

                // XR Origin 위치 + 회전 복원
                var standPosNormal = StandPos.position;
                standPosNormal.y = 0;

                VRPlayer.SetPositionAndRotation(standPosNormal, StandPos.rotation);

                // Camera Offset 높이 복원
                var deskPos = CameraOffset.localPosition;
                deskPos.y = prevYOffest;

                CameraOffset.localPosition = deskPos;

                PlayerChair.SetActive(true);
                XRTeleport.enabled = true;
                CameraController.SetMoveLockState(false);

                break;

            case EPlayDevice.Desktop:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                Buttons[ScenarioIndex].color = Color.white;
                ScenarioIndex = 0;

                CameraController.SetMoveLockState(false);
                CameraSwitcher.Revert();

                break;
        }
    }
}
