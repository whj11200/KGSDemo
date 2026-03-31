using UnityEngine;

public class GuideZone : MonoBehaviour
{
    [SerializeField] NPC_Controller controller;
    [SerializeField] NPC_StaticSpeaker staticSpeaker;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
          if(controller != null)
          {
            controller.OnPlayerEnteredZone();
            Debug.Log("Player entered guide zone.");
          }
          if(staticSpeaker != null)
            {
                staticSpeaker.OnPlayerEnteredZone();
            }
         
        }

       
    }
}
