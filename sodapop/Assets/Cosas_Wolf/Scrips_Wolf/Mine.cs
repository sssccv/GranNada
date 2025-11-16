using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Mine : NetworkBehaviour
{
    [Header("Settings")]
    public float lifetime = 20f;
    public GameObject explosionEffect;
    public GameObject damageZonePrefab;

    private bool isArmed = false;

    private void Start()
    {
        if (IsServer)
        {
            StartCoroutine(DestroyAfterLifetime());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        // Cuando toca el suelo, se queda fija
        if (!isArmed)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            isArmed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !isArmed) return;

        // Detecta jugador por trigger
        if (other.GetComponent<PlayerMovement>() != null || other.GetComponent<Health>() != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);
            zone.GetComponent<NetworkObject>().Spawn();
        }

        GetComponent<NetworkObject>().Despawn();
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        if (GetComponent<NetworkObject>() != null)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
