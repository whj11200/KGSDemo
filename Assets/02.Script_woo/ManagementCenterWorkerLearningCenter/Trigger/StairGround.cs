using UnityEngine;

public class StairGround : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController character =
            other.GetComponentInParent<CharacterController>();

        if (character != null && character.CompareTag("Player"))
        {
            cameraController.moveSpeed = 3f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController character =
            other.GetComponentInParent<CharacterController>();

        if (character != null && character.CompareTag("Player"))
        {
            cameraController.moveSpeed = 5f;
        }
    }
}