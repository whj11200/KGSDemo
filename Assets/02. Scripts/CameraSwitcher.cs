using Unity.Cinemachine;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineBrain Brain;
    [SerializeField] private CinemachineCamera TPSCam;
    [SerializeField] private CinemachineCamera CurrentCam;

    int defaultPriority = 0;

    private void Awake()
    {
        defaultPriority = TPSCam.Priority;
        CurrentCam = TPSCam;
    }


    public void SetCamera(CinemachineCamera camera)
    {
        CurrentCam = camera;

        TPSCam.Priority = defaultPriority - 1;
        CurrentCam.Priority = defaultPriority;
    }

    public void Revert()
    {
        CurrentCam.Priority = defaultPriority - 1;
        TPSCam.Priority = defaultPriority;

        CurrentCam = TPSCam;
    }
}
