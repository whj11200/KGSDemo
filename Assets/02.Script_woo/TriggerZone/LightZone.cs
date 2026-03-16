using UnityEngine;

public class LightZone : MonoBehaviour
{
    public GameObject[] Lights;

    private void OnTriggerEnter(Collider other)
    {
      if(other.CompareTag("Player"))
        {
            foreach (var light in Lights)
            {
                light.SetActive(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            foreach (var light in Lights)
            {
                light.SetActive(false);
            }
        }
    }
}
