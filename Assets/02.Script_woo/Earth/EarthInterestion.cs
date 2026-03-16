using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum CityType
{
    Seoul,
    Busan,
    Jeongeup
}

public class EarthInterestion : MonoBehaviour
{
    [SerializeField] GameObject earth;
    [SerializeField] Transform MapArea;
    [SerializeField] Animator earthAnimator;

    [SerializeField] Image StartButton;
    [SerializeField] GameObject KoreaMapUI;
    [SerializeField] RotationController rotationController;
    [SerializeField] ParticleSystem Smoke;


    [SerializeField] Animator EarthAnimator;

    Coroutine fadeRoutine;
    float duration = 1f;

    [SerializeField] float scaleDuration = 3f;
    const float TARGET_SCALE = 0.005397157f;
    Coroutine scaleRoutine;

    private void Start()
    {
        MapArea.localScale = new Vector3(0,0,0);
    }
    public void Normal()
    {
        
        earth.SetActive(true);
        EarthAnimator.enabled = false;
        rotationController.StartOrbitRotation();
        StartButton.gameObject.SetActive(true);
        StartButton.fillAmount = 1f;
        KoreaMapUI.SetActive(false);
       
    }

    public void StepOne()
    {
        EarthAnimator.enabled = true;
        rotationController.StopAllRotation();
        earthAnimator.enabled = true;
        earthAnimator.SetTrigger("On");
        KoreaMapUI.SetActive(false);
    }
    
    public void NomalMap()
    {
        MapArea.localScale = new Vector3(0,0,0);
    }

    public void ScaleUpMAP()
    {
        StartScale(Vector3.one * TARGET_SCALE);
    }

    public void ScaleDownMAP()
    {
        StartScale(Vector3.zero);
        Smoke.Stop();
    }

    void StartScale(Vector3 target)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleCoroutine(target));
    }

    IEnumerator ScaleCoroutine(Vector3 targetScale)
    {
        Vector3 startScale = MapArea.localScale;
        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;

            MapArea.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        MapArea.localScale = targetScale;
        scaleRoutine = null;
    }





    // 정월읍 버튼에 할당
    public void PlayCityEarthEffectByIndex(int cityIndex)
    {
        PlayCityEarthEffect((CityType)cityIndex);
    }

  
    public void PlayCityEarthEffect(CityType city)
    {
        if (earthAnimator == null) return;

        string trigger = city switch
        {
            CityType.Seoul => "Seoul",
            CityType.Busan => "Busan",
            CityType.Jeongeup => "Jeongeup",
            _ => null
        };

        if (!string.IsNullOrEmpty(trigger))
            earthAnimator.SetTrigger(trigger);

        if (KoreaMapUI != null)
            KoreaMapUI.SetActive(false);
    }
    // 시작 버튼 할당 
    public void StepOneEarthButtonAnimation()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FillFadein());
    }
    public void BackAnimation()
    {
        earth.SetActive(true);
        Debug.Log("BackAnimation called");
        if (earthAnimator != null)
            earthAnimator.SetTrigger("Back");
      
    }
    IEnumerator FillFadein()
    {
        float start = StartButton.fillAmount;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            StartButton.fillAmount = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }

        StartButton.fillAmount = 0f;
        fadeRoutine = null;
        StartButton.gameObject.SetActive(false);
    }
}
