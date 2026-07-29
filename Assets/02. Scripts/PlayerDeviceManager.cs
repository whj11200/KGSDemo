using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PlayerDeviceManager : MonoBehaviour
{
    public static EPlayDevice PlayDevice = EPlayDevice.VR;
    [SerializeField] private GameObject DesktopPlayer;
    [SerializeField] private GameObject VRPlayer;
    public static bool IsVR => PlayDevice == EPlayDevice.VR;
    public static bool IsDesktop => PlayDevice == EPlayDevice.Desktop;
    private void Awake()
    {
        #if DESKTOP_BUILD
            PlayDevice = EPlayDevice.Desktop;
        #elif VR_BUILD
            PlayDevice = EPlayDevice.VR;
        #endif

        if (DesktopPlayer == null || VRPlayer == null) return;

        var isVR = PlayDevice == EPlayDevice.VR;

        DesktopPlayer.SetActive(!isVR);
        VRPlayer.SetActive(isVR);
    }
}
