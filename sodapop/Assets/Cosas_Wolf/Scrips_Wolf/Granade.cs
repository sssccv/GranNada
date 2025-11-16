using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Granade : NetworkBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public GameObject damageZonePrefab;

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

        // Efecto visual local
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Zona de daño en red
        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);
            zone.GetComponent<NetworkObject>().Spawn();
        }

        // Despawn en red
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
