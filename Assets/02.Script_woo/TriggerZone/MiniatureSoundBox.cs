using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MiniatureSoundBox : MonoBehaviour
{
    [SerializeField] GameObject KGS_Canvas;
    [SerializeField] Image KGS_Img;
    [SerializeField] TextMeshProUGUI KGS_Text;
    [SerializeField] AudioSource KGS_Audio;
    [SerializeField] AudioClip KGS_Clip;

    bool isPlaying = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")&& !isPlaying)
        {

            StartStudyRoom();
            isPlaying = true;
        }
    }


    public void StartStudyRoom()
    {
        StartCoroutine(StartUp());
    }

    private IEnumerator StartUp()
    {
        KGS_Canvas.SetActive(true);
        KGS_Img.gameObject.SetActive(true);
        KGS_Text.text = "이곳은 가상체험관입니다. \r\n벽면의 지도에서 지역을 선택하여 \r\n누출 및 화재 시뮬레이션을 시작할 수 있으며  \r\n뒤쪽의 문을 통해 \r\n관리소 및 통제소 작업자 학습관으로 \r\n이동할 수 있습니다";
        
        if (KGS_Audio != null && KGS_Clip != null)
        {
            KGS_Audio.PlayOneShot(KGS_Clip);

            // 오디오 재생이 끝날 때까지 대기
            yield return new WaitWhile(() => KGS_Audio.isPlaying);
        }

        KGS_Canvas.SetActive(false);
        KGS_Img.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
    }
}
