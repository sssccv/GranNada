using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    public InputReader inputReader;
    public Transform firePoint;
    public GameObject[] grenadePrefabs; // aquí agregas tus granadas, minas, etc.

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

        // Seleccionar prefab aleatoriamente
        GameObject selectedPrefab = grenadePrefabs[UnityEngine.Random.Range(0, grenadePrefabs.Length)];

        // Instancia la granada o mina
        GameObject grenade = Instantiate(selectedPrefab, firePoint.position, firePoint.rotation);

        // Spawn en red
        var netObj = grenade.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("❌ Este prefab no tiene NetworkObject: " + selectedPrefab.name);
            Destroy(grenade);
            return;
        }
        netObj.Spawn();

        // ----- INICIALIZAR SEGÚN EL TIPO -----

        // Si es una granada
        var granadeScript = grenade.GetComponent<Granade>();
        if (granadeScript != null)
        {
            granadeScript.Initialize(OwnerClientId);
        }

        // Si es una mina
        var mineScript = grenade.GetComponent<Mine>();
        if (mineScript != null)
        {
            mineScript.Initialize(OwnerClientId);
        }

        // ----- APLICAR FÍSICA -----
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forward = firePoint.forward * shootForce;
            Vector3 upward = Vector3.up * upwardForce;

            rb.AddForce(forward + upward, ForceMode.Impulse);
        }
    }
}
