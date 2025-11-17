using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Granade : NetworkBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public GameObject damageZonePrefab;

    private ulong attackerId;

    public void Initialize(ulong attacker)
    {
        attackerId = attacker;
    }

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

        // Efecto visual
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Spawnear zona de daño con attackerId
        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);

            var damageComp = zone.GetComponent<GranadeDamage>();
            damageComp.Initialize(attackerId);

            zone.GetComponent<NetworkObject>().Spawn();
        }

        GetComponent<NetworkObject>().Despawn();
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        if (GetComponent<NetworkObject>() != null)
            GetComponent<NetworkObject>().Despawn();
    }
}

/*public class Granade : NetworkBehaviour
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

        // Zona de da�o en red
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
}*/
