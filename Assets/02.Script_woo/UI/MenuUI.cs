using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    [SerializeField]CameraController controller;
    public void TutorialScene()
    {
      SceneManager.LoadScene("Tutorial");
      controller.ToggleMenu();
    }
    public void KGSScene()
    {
        SceneManager.LoadScene("KGSScene");
        controller.ToggleMenu();
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
