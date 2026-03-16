using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeUi : MonoBehaviour
{
    [SerializeField] Image KGS_Img;
    [SerializeField] AudioSource KGS_Audio;
    [SerializeField] AudioClip KGS_Clip;
    [SerializeField] AnjesonInteraction anjeson;

    public bool isfinish = false;
    private void Start()
    {
        
        StartCoroutine(Fadeout());
    }

    private IEnumerator Fadeout()
    {
        if (KGS_Audio != null)
        {
            KGS_Audio.PlayOneShot(KGS_Clip);
        }
        yield return new WaitForSeconds(4f);
        KGS_Img.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.75f);
        isfinish = true;
        anjeson.HandleHello();
        this.gameObject.SetActive(false);
    }
}
