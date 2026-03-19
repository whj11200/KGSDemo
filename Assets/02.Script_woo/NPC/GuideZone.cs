using UnityEngine;

public class GuideZone : MonoBehaviour
{
    [SerializeField] NPC_Controller anjesonController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           anjesonController.OnPlayerEnteredZone();
           Debug.Log("Player entered guide zone.");
        }
    }
}
