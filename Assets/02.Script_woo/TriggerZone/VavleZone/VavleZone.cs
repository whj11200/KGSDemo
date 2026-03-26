using UnityEngine;

public class VavleZone : Minimapfuntioni
{
    [SerializeField] ValveController valveController;
    [SerializeField] PuddleController puddleController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(!valveController.isLeaking)
            valveController.StartLeak();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (valveController.isLeaking)
            {
                puddleController.StartShrinking();
                valveController.ResetValve();
            }
            
        }
    }
}
