using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public bool isClear =false;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (!isClear) return;
            Debug.Log("Player has entered the trigger. Change scene here.");
            SceneManager.LoadScene("KGSScene"); // Uncomment and specify the scene name to change scenes
        }
    }
}
