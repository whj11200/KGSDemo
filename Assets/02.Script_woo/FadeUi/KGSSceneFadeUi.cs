using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class KGSSceneFadeUi : MonoBehaviour
{
    [SerializeField] Image KGS_Img;
    [SerializeField] AudioSource KGS_Audio;
    [SerializeField] AudioClip KGS_Clip;
    [SerializeField] EnvironmentManager envManager;

    public bool isfinish = false;

    private void Awake()
    {
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
            envManager.TestRest();
        }
    }

    public void StartStudyRoom()
    {
        StartCoroutine(StartUp());
    }

    private IEnumerator StartUp()
    {
        if (KGS_Audio != null)
        {
            KGS_Audio.PlayOneShot(KGS_Clip);
        }
        KGS_Img.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);
        KGS_Img.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.75f);
        isfinish = true;

        envManager.InitializeNPC();
        this.gameObject.SetActive(false);
    }
}
