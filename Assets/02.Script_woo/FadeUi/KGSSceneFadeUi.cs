using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KGSSceneFadeUi : OverlayUI
{
    [SerializeField] Image KGS_Img;
    [SerializeField] AudioSource KGS_Audio_PC;
    [SerializeField] AudioSource KGS_Audio_VR;
    [SerializeField] AudioClip KGS_Clip;
    [SerializeField] EnvironmentManager envManager;

    public bool isfinish = false;

    protected override void Awake()
    {
        base.Awake();

        KGS_Img.gameObject.SetActive(false);
    }

    private void Start()
    {
        var req = SceneRequest.Request;

        if (req == null || req.LastScene == ESceneName.Tutorial)
        {
            StartStudyRoom();
        }
        else
        {
            Debug.Log("Skip Startup");
            // envManager.TestRest();
        }
    }

    public void StartStudyRoom()
    {
        StartCoroutine(StartUp());
    }

    private IEnumerator StartUp()
    {
        KGS_Img.gameObject.SetActive(true);
        if(PlayerDeviceManager.IsVR && KGS_Audio_VR != null && KGS_Clip != null)
        {
            KGS_Audio_VR.PlayOneShot(KGS_Clip);
            // 오디오 재생이 끝날 때까지 대기
            yield return new WaitWhile(() => KGS_Audio_VR.isPlaying);
        }
        else if (PlayerDeviceManager.IsDesktop && KGS_Audio_PC != null && KGS_Clip != null)
        {
            KGS_Audio_PC.PlayOneShot(KGS_Clip);

            // 오디오 재생이 끝날 때까지 대기
            yield return new WaitWhile(() => KGS_Audio_PC.isPlaying);
        }

        KGS_Img.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        isfinish = true;

        // envManager.InitializeNPC();
        envManager.AllClear();

        gameObject.SetActive(false);
    }
}
