using UnityEngine;

public class VavleZone : MonoBehaviour
{
    [SerializeField] ValveController valveController;
    [SerializeField] PuddleController puddleController;
    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        valveController.StartLeak();
    //    }
    //}
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puddleController.StartShrinking();
            valveController.ResetValve();
        }
    }
}
