using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TPSPlayerController playerController;
    [SerializeField] private float damage = 25f;
    [SerializeField] private float fireRate = 8f;
    [SerializeField] private float range = 120f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private int shootMouseButton = 0;

    private float nextFireTime;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerController == null)
        {
            playerController = GetComponent<TPSPlayerController>();
        }
    }

    private void Update()
    {
        if (playerController == null || !playerController.IsAiming)
        {
            return;
        }

        if (!Input.GetMouseButton(shootMouseButton))
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + (1f / fireRate);
        Shoot();
    }

    private void Shoot()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            return;
        }

        ZombieHealth health = hit.collider.GetComponentInParent<ZombieHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }
}
