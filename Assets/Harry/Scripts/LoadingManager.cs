using Unity.VisualScripting;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;
    public bool isTest = false;

    private DataManager dataManager;
    private GameManager gameManager;


    private void Awake()
    {
        // 이미 인스턴스가 존재하면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬이 바뀌어도 유지
        DontDestroyOnLoad(gameObject);
        dataManager = DataManager.Instance;

        if (isTest) dataManager.Initialization();
    }

    private void Start()
    {
    }

}
