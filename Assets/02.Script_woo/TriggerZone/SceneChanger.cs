using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : SupportXRInteractable
{
    public bool isClear = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            LoadScnene();
        }
    }

    public override void ClickExit()
    {
        LoadScnene();
    }

    private void LoadScnene()
    {
        if (!isClear) return;
        Debug.Log("Player has entered the trigger. Change scene here.");
        SceneManager.LoadScene("KGSScene"); // Uncomment and specify the scene name to change scenes
    }
}
