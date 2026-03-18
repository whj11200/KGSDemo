using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;


    public bool isTest = false;

    private DataManager dataManager;
    private GameManager gameManager;
    private UiManager uiManager;
    private SceneLifeManager sceneLifeManager;

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
    }
    private void Start()
    {
    }

    private void OnEnable()
    {
        dataManager = DataManager.Instance;
        gameManager = GameManager.Instance;
        uiManager = UiManager.Instance;
    }

    #region Loading System
    public void Intialization(ESceneName SceneName)
    {
        switch (SceneName)
        {
            case ESceneName.Loading:
                dataManager.Initialization();
                gameManager.ReportResult("SceneMove", (int)ESceneName.Tutorial);
                break;
            case ESceneName.Loading_Test:
                dataManager.Initialization();
                gameManager.ReportResult("SceneMove", (int)ESceneName.Tutorial_Test);
                break;
            case ESceneName.Tutorial:
                gameManager.StartScenario(SceneName.ToString());
                break;
            case ESceneName.Tutorial_Test:
                uiManager.OnUiSelect();
                gameManager.StartScenario("Tutorial");
                break;
        }
    }
    

    #endregion

}
