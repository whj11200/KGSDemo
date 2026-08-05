using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineBrain Brain;
    [SerializeField] private CinemachineCamera TPSCam;
    [SerializeField] private CinemachineCamera CurrentCam;
    [SerializeField] private Image Aim;

    public event Action<CinemachineCamera> OnCameraChanged;
    public event Action OnCameraReverted;

    int defaultPriority = 0;

    private void Awake()
    {
        defaultPriority = TPSCam.Priority;
        CurrentCam = TPSCam;
    }


    public void SetCamera(CinemachineCamera camera)
    {
        if (CurrentCam != null)
        {
            CurrentCam.Priority = defaultPriority - 1;
        }

        CurrentCam = camera;

        TPSCam.Priority = defaultPriority - 1;
        CurrentCam.Priority = defaultPriority;

        OnCameraChanged?.Invoke(camera);
    }

    public void Revert()
    {
        CurrentCam.Priority = defaultPriority - 1;
        TPSCam.Priority = defaultPriority;

        CurrentCam = TPSCam;
        Aim.enabled = true;

        OnCameraReverted?.Invoke(); 
    }

    public bool IsCurrentCamera(CinemachineCamera camera)
    {
        return CurrentCam == camera;
    }
}
