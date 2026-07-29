using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[DefaultExecutionOrder(-1000)]
public class PlayerModeManager : MonoBehaviour
{
    public static EPlayDevice PlayDevice = EPlayDevice.VR;
    [SerializeField] private GameObject DesktopPlayer;
    [SerializeField] private GameObject VRPlayer;

    private void Awake()
    {
        if (DesktopPlayer == null || VRPlayer == null) return;

        var isVR = PlayDevice == EPlayDevice.VR;

        DesktopPlayer.SetActive(!isVR);
        VRPlayer.SetActive(isVR);
    }
}
