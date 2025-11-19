using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Granade : NetworkBehaviour
{
    [Header("Settings")]
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public GameObject damageZonePrefab;

    private ulong attackerId;

    // Recibir el ID del jugador que lanzó la granada
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

        Explode();
    }

    private void Explode()
    {
        //  Efecto visual sincronizado en todos los clientes con posición enviada
        ExplodeClientRpc(transform.position);

        //  Zona de daño en red (solo servidor)
        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);

            var damageComp = zone.GetComponent<GranadeDamage>();
            if (damageComp != null)
                damageComp.Initialize(attackerId);
            else
                Debug.LogError("❌ EL PREFAB damageZonePrefab NO TIENE 'GranadeDamage'");

            zone.GetComponent<NetworkObject>().Spawn();
        }

        //  Despawn en red
        GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void ExplodeClientRpc(Vector3 explosionPosition)
    {
        // Este código se ejecuta en todos los clientes con la posición correcta
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
        }
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
