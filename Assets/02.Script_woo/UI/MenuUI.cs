using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void TutorialScene()
    {
      SceneManager.LoadScene("Tutorial");
    }
    public void KGSScene()
    {
        SceneManager.LoadScene("Tutorial");
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
