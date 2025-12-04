using System;
using UnityEngine;
using FishNet.Object;

public class PlayerShooter : NetworkBehaviour
{
    [Header("References")]
    public InputReader inputReader;
    public Transform firePoint;
    public GameObject[] grenadePrefabs;

    [Header("Settings")]
    public float shootForce = 10f;          // Fuerza horizontal principal
    public float upwardForce = 5f;          // Fuerza vertical para crear la parábola
    public float fireRate = 0.5f;

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
        // Al soltar el botón, disparamos UNA sola vez
        if (!isPressed && base.IsOwner && Time.time - lastFireTime > fireRate)
        {
            // Intentar usar el método DoThrow() del componente PlayerMovement (usa Trigger en PlayerMovement)
            var pm = GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.DoThrow();
            }
            else
            {
                // Fallback: reproducir la animación localmente en el dueño
                PlayThrowLocal();
            }

            // Enviar al servidor para que haga el spawn y notifique a los demás clientes
            ShootServerRpc(firePoint.position, firePoint.rotation);
            lastFireTime = Time.time;
        }
    }

        
    [ServerRpc]
    public void ShootServerRpc(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        if (firePoint == null || grenadePrefabs == null || grenadePrefabs.Length == 0)
        {
            Debug.LogWarning(" Faltan referencias o no hay prefabs en PlayerShooter");
            return;
        }

        // Seleccionar prefab aleatoriamente
        GameObject selectedPrefab = grenadePrefabs[UnityEngine.Random.Range(0, grenadePrefabs.Length)];

        // Instancia la granada o mina en el servidor
        GameObject grenade = Instantiate(selectedPrefab, spawnPosition, spawnRotation);

        // Spawn en red
        var netObj = grenade.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError(" Este prefab no tiene NetworkObject: " + selectedPrefab.name);
            Destroy(grenade);
            return;
        }
        base.Spawn(grenade);

        // ----- INICIALIZAR SEGÚN EL TIPO -----
        var granadeScript = grenade.GetComponent<Granade>();
        if (granadeScript != null)
        {
            // FishNet usa int para OwnerId. Casteamos a ulong por compatibilidad con scripts antiguos si es necesario.
            granadeScript.Initialize((ulong)base.OwnerId);
        }

        var mineScript = grenade.GetComponent<Mine>();
        if (mineScript != null)
        {
            mineScript.Initialize((ulong)base.OwnerId);
        }

        // ----- APLICAR FÍSICA -----
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forward = firePoint.forward * shootForce;
            Vector3 upward = Vector3.up * upwardForce;

            rb.AddForce(forward + upward, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning(" El prefab " + selectedPrefab.name + " no tiene Rigidbody");
        }
    }

    private void PlayThrowLocal()
    {
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }
    }
}
