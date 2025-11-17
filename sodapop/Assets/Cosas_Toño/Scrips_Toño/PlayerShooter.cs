using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    public InputReader inputReader;
    public Transform firePoint;
    public GameObject[] grenadePrefabs; // aquí agregas todas tus variantes de granadas

    [Header("Settings")]
    public float shootForce = 10f;          // Fuerza horizontal principal
    public float upwardForce = 5f;          // Fuerza vertical para crear la parábola
    public float fireRate = 0.5f;

    private bool isFiring;
    private float lastFireTime;

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
        if (!IsOwner) return; // Solo el jugador dueño puede disparar

        if (isFiring && Time.time - lastFireTime > fireRate)
        {
            ShootServerRpc(); // Pedimos al servidor que dispare
            lastFireTime = Time.time;
        }
    }

    [ServerRpc]
private void ShootServerRpc()
{
    if (firePoint == null || grenadePrefabs == null || grenadePrefabs.Length == 0)
    {
        Debug.LogWarning("Faltan referencias o no hay prefabs en PlayerShooter");
        return;
    }

    GameObject selectedPrefab = grenadePrefabs[UnityEngine.Random.Range(0, grenadePrefabs.Length)];

    GameObject grenade = Instantiate(selectedPrefab, firePoint.position, firePoint.rotation);

    // Spawn en red
    var netObj = grenade.GetComponent<NetworkObject>();
    netObj.Spawn();

    // PASAR EL ID DEL ATACANTE
    grenade.GetComponent<Granade>().Initialize(OwnerClientId);

    // Física
    Rigidbody rb = grenade.GetComponent<Rigidbody>();
    if (rb != null)
    {
        Vector3 forward = firePoint.forward * shootForce;
        Vector3 upward = Vector3.up * upwardForce;

        rb.AddForce(forward + upward, ForceMode.Impulse);
    }
}

    /*[ServerRpc]
    private void ShootServerRpc()
    {
        if (firePoint == null || grenadePrefabs == null || grenadePrefabs.Length == 0)
        {
            Debug.LogWarning("Faltan referencias o no hay prefabs en PlayerShooter");
            return;
        }

        // Selecciona una granada aleatoria del array
        GameObject selectedPrefab = grenadePrefabs[UnityEngine.Random.Range(0, grenadePrefabs.Length)];

        // Instancia la granada en el servidor
        GameObject grenade = Instantiate(selectedPrefab, firePoint.position, firePoint.rotation);

        // MUY IMPORTANTE: Spawn en red
        grenade.GetComponent<NetworkObject>().Spawn();

        // Aplica fuerza para que la granada se mueva
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forward = firePoint.forward * shootForce;
            Vector3 upward = Vector3.up * upwardForce;
            rb.AddForce(forward + upward, ForceMode.Impulse);
        }
    }*/
}
