using UnityEngine;
using Unity.Netcode;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    public InputReader inputReader;
    public Transform firePoint;
    public GameObject[] grenadePrefabs;

    [Header("Settings")]
    public float shootForce = 10f;
    public float upwardForce = 5f;
    public float fireRate = 0.5f;
    public int projectileDamage = 20;

    private bool isFiring;
    private float lastFireTime;

    private ulong attackerId;

    private void Awake()
    {
        // Esto solo funciona después del spawn en Netcode.
        var netObj = GetComponent<NetworkObject>();
        attackerId = netObj != null ? netObj.OwnerClientId : ulong.MaxValue;
    }

    private void OnEnable()
    {
        inputReader.OnFireEvent += HandleFire;
    }

    private void OnDisable()
    {
        inputReader.OnFireEvent -= HandleFire;
    }

    private void HandleFire(bool isPressed)
    {
        isFiring = isPressed;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (isFiring && Time.time - lastFireTime > fireRate)
        {
            ShootServerRpc();
            lastFireTime = Time.time;
        }
    }

    // ----- RPC PARA DISPARAR -----
    [ServerRpc]
    private void ShootServerRpc()
    {
        Shoot();
    }

    private void Shoot()
    {
        if (firePoint == null || grenadePrefabs == null || grenadePrefabs.Length == 0)
        {
            Debug.LogWarning("Faltan referencias en PlayerShooter");
            return;
        }

        GameObject prefab = grenadePrefabs[Random.Range(0, grenadePrefabs.Length)];
        GameObject proj = Instantiate(prefab, firePoint.position, firePoint.rotation);

        // Spawn en red
        proj.GetComponent<NetworkObject>().Spawn(true);

        // Inicializar daño + atacante
        if (proj.TryGetComponent<Bullet>(out Bullet bullet))
        {
            bullet.Initialize(OwnerClientId, projectileDamage);
        }

        // Física
        if (proj.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 forward = firePoint.forward * shootForce;
            Vector3 upward = Vector3.up * upwardForce;

            rb.AddForce(forward + upward, ForceMode.Impulse);
        }
    }
}
