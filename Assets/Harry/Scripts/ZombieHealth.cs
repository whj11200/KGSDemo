
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 60f;
    private float currentHp;
    private bool isDead;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHp -= amount;
        if (currentHp > 0f)
        {
            return;
        }

        isDead = true;
        TPSGameManager manager = TPSGameManager.Instance;
        if (manager != null)
        {
            manager.OnZombieKilled();
        }
        Destroy(gameObject);
    }
}
