using UnityEngine;

public class MiniatureSoundBox : MonoBehaviour
{
    [SerializeField] AudioSource playersound;
    [SerializeField] AudioClip sound;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            
            playersound.PlayOneShot(sound);
            this.gameObject.SetActive(false);
        }
    }
}
