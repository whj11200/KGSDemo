using UnityEngine;

public class PuddleCollisionProxy : MonoBehaviour
{
    public PuddleController parentController; // 부모 스크립트 연결

    void OnParticleCollision(GameObject other)
    {
        if (parentController != null)
        {
            parentController.GrowPuddle();
        }
    }
}