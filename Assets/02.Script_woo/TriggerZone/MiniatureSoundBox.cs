using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniatureSoundBox : MonoBehaviour
{
    public enum DialogueType
    {
        VirtualExperienceHall,
        ExperienceHallway,
    }

    [Header("Dialogue Setting")]
    [SerializeField] private DialogueType dialogueType;



    [Header("UI")]
    [SerializeField] private GameObject KGS_Canvas;
    [SerializeField] private Image KGS_Img;
    [SerializeField] private TextMeshProUGUI KGS_Text;

    [Header("Audio")]
    [SerializeField] private AudioSource KGS_Audio_PC;
    [SerializeField] private AudioSource KGS_Audio_VR;
    [SerializeField] private AudioClip KGS_Clip;

    private bool isPlaying;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isPlaying)
            return;

        StartStudyRoom();
    }

    public void StartStudyRoom()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        StartCoroutine(StartUp());
    }

    private IEnumerator StartUp()
    {
        if (KGS_Canvas != null)
            KGS_Canvas.SetActive(true);

        if (KGS_Img != null)
            KGS_Img.gameObject.SetActive(true);

        if (KGS_Text != null)
            KGS_Text.text = GetDialogueText();
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
        else
        {
            // 오디오가 없을 경우 텍스트 확인 시간
            yield return new WaitForSeconds(5f);
        }

        if (KGS_Canvas != null)
            KGS_Canvas.SetActive(false);

        if (KGS_Img != null)
            KGS_Img.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.1f);

        gameObject.SetActive(false);
    }

    private string GetDialogueText()
    {
        switch (dialogueType)
        {
            case DialogueType.VirtualExperienceHall:
                return
                    "이곳은 가상체험관입니다.\n" +
                    "벽면의 지도에서 지역을 선택하여\n" +
                    "누출 및 화재 시뮬레이션을 시작할 수 있습니다.";

            case DialogueType.ExperienceHallway:
                return
                    "뒤쪽의 문을 통해 " +
                    "\r\n관리소 및 통제소 작업자 학습관으로 " +
                    "\r\n이동할 수 있습니다 ";

            default:
                return string.Empty;
        }
    }
}