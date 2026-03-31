using UnityEngine;

public class FireTrackTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
           
        }
    }
}
