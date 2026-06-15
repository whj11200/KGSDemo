using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiSliderTable : MonoBehaviour
{
    [Header("Fill Image")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float fillDuration = 1f;

    [Header("Show Objects")]
    [SerializeField] private List<GameObject> showObjects = new();
    [SerializeField] private int showIndex = 0;

    private Coroutine fillCoroutine;

    private void Awake()
    {
        if (fillImage == null)
            fillImage = GetComponent<Image>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (fillImage != null)
            fillImage.fillAmount = 0f;

        foreach (GameObject obj in showObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    /// <summary>
    /// 버튼이나 Ray 클릭에서 호출할 함수
    /// </summary>
    public void StartFill()
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(FillRoutine());
    }

    private IEnumerator FillRoutine()
    {
        if (fillImage == null)
            yield break;

        fillImage.fillAmount = 0f;

        float time = 0f;

        while (time < fillDuration)
        {
            time += Time.deltaTime;

            float value = time / fillDuration;
            fillImage.fillAmount = Mathf.Clamp01(value);

            yield return null;
        }

        fillImage.fillAmount = 1f;

        OnFillComplete();
    }

    private void OnFillComplete()
    {
        if (showObjects == null || showObjects.Count == 0)
            return;

        if (showIndex < 0 || showIndex >= showObjects.Count)
            return;

        if (showObjects[showIndex] != null)
            showObjects[showIndex].SetActive(true);
    }

    /// <summary>
    /// 특정 인덱스 오브젝트를 켜고 싶을 때 사용
    /// </summary>
    public void StartFillWithIndex(int index)
    {
        showIndex = index;
        StartFill();
    }

    /// <summary>
    /// 초기화용
    /// </summary>
    public void ResetFill()
    {
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        Init();
    }
}