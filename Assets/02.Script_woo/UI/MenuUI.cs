using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField] CameraController controller;
    [SerializeField] EnvironmentManager KGS;

    private void OnEnable()
    {
        if (KGS == null)
            KGS = FindFirstObjectByType<EnvironmentManager>();
    }

    public void TutorialScene()
    {
        SwitchScene("Tutorial");
    }

    public void KGSScene(int content)
    {
        var currentScene = SceneManager.GetActiveScene();
        var sceneType = (ESceneName)currentScene.buildIndex;

        var ContentRequest = new ContentRequest
        {
            LastScene = sceneType,
            ContentID = content
        };

        if (currentScene.name != ESceneName.KGSScene.ToString())
        {
            SceneRequest.Request = ContentRequest;

            SwitchScene("KGSScene"); 
        }
        else
        { 
            KGS?.OpenContent(ContentRequest);
            controller.ToggleMenu();
        }
    }

    public void SwitchScene(string sceneName)
    {
        controller.ToggleMenu();
        SceneManager.LoadScene(sceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}

public static class SceneRequest
{
    public static ContentRequest Request;
}

public class ContentRequest
{
    public ESceneName LastScene;
    public int ContentID;
}
