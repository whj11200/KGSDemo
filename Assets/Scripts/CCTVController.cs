using UnityEngine;

public class CCTVController : MonoBehaviour
{
    [Header("타겟 위치가 있는 씬과 로드 여부")]
    [SerializeField] string SceneName = "AdditiveScene";
    [SerializeField] bool isCameraWork = true;
    public bool IsCameraWork
    {
        get => isCameraWork;
        private set
        {
            if (isCameraWork == value) return;

            isCameraWork = value;
            CCTVParent.SetActive(value);
            CCTVNotReadyText.SetActive(!value);
        }
    }
    [SerializeField] GameObject CCTVParent;
    [SerializeField] GameObject CCTVNotReadyText;

    private void Awake()
    {
        IsCameraWork = false;
    }

    private void OnEnable()
    {
        Teleporter.OnAddScene += CheckCameraTarget;
    }

    private void OnDisable()
    {
        Teleporter.OnAddScene -= CheckCameraTarget;
    }

    private void CheckCameraTarget(string addedSceneName)
    {
        if (SceneName == addedSceneName)
        {
            IsCameraWork = true;
        }  
    }
}
