using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    [SerializeField] private float attackDistance = 1.6f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float lookSpeed = 8f;

    private NavMeshAgent agent;
    private Transform player;
    private PlayerHealth playerHealth;
    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (player == null || playerHealth == null || playerHealth.IsDead)
        {
            if (agent.enabled)
            {
                agent.isStopped = true;
            }
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            agent.isStopped = true;
            FacePlayer();
            TryAttack();
        }
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        playerHealth.TakeDamage(attackDamage);
    }
}
