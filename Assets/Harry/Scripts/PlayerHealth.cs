using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 100f;
    private float currentHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

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
        if (currentHp <= 0f)
        {
            currentHp = 0f;
            isDead = true;
            TPSGameManager manager = TPSGameManager.Instance;
            if (manager != null)
            {
                manager.OnPlayerDead();
            }
        }
    }
}
