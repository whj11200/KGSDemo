using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
          
            Debug.Log("Player has entered the trigger. Change scene here.");
            SceneManager.LoadScene("KGSScene"); // Uncomment and specify the scene name to change scenes
        }
    }
}
