using System;
using UnityEngine;

public class SceneLifeManager : MonoBehaviour
{
    private GameManager gameManager;
    private LoadingManager loadingManager;
    private DataManager dataManager;


    private bool isSceneInitialized = false;


    public ESceneName currentScene;
    public event Action<string> OnSceneInitialized;

    private void Awake()
    {

    }


    void Start()
    {
        Intialization();

        loadingManager.Intialization(currentScene);
    }

    void Update()
    {
        if (!isSceneInitialized)
        {
            isSceneInitialized = true;
        }
    }

    private void Intialization()
    {
        gameManager = GameManager.Instance;
        loadingManager = LoadingManager.Instance;
        dataManager = DataManager.Instance;
    }
}
