using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] CameraController controller;
    [SerializeField] EnvironmentManager KGS;
    [SerializeField] Button Selected;
    [SerializeField] Sprite SelectSprite;
    [SerializeField] Sprite NormalSprite;

    [SerializeField] List<MenuButton> SelectContentButtons = new();
    [SerializeField] Dictionary<Button, MenuButton> Buttons = new();

    private void Awake()
    {
        if (SelectContentButtons.Count > 0)
        {
            foreach (var button in SelectContentButtons)
            {
                Buttons[button.Button] = button;
            }
        }
    }

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

    public void ClickButton(Button button)
    {
        if (Buttons.TryGetValue(button, out var sB))
        {
            if (Selected != null)
            {
                Buttons[Selected].Image.sprite = NormalSprite;
                Buttons[Selected].Text.color = Color.black;
            }

            Selected = button;
            sB.Image.sprite = SelectSprite;
            sB.Text.color = Color.white;
        }
    }

    public void OnSelect()
    {
        Buttons[Selected].OnExecute?.Invoke();
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

[System.Serializable]
public class MenuButton
{
    public Button Button;
    public Image Image;
    public TMP_Text Text;
    public UnityEvent OnExecute;
}