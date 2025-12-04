using System.Collections;
using UnityEngine;
using FishNet.Object;

public class Mine : NetworkBehaviour
{
    [Header("Settings")]
    public float lifetime = 20f;
    public GameObject explosionEffect;
    public GameObject damageZonePrefab;

    private bool isArmed = false;
    private ulong attackerId;

    // Recibir el ID del jugador que la colocó
    public void Initialize(ulong attacker)
    {
        attackerId = attacker;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServerInitialized) return;

        if (!isArmed)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // se queda pegada
            }
            isArmed = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || !isArmed) return;

        if (other.GetComponent<PlayerMovement>() != null || other.GetComponent<Health>() != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Efecto visual sincronizado en todos los clientes
        ExplodeObserversRpc(transform.position);

        // Zona de daño en red (solo servidor)
        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);

            var dmg = zone.GetComponent<GranadeDamage>();
            if (dmg != null)
                dmg.Initialize(attackerId);

            ServerManager.Spawn(zone);
        }

        // Despawn en red
        ServerManager.Despawn(gameObject);
    }

    [ObserversRpc]
    private void ExplodeObserversRpc(Vector3 explosionPosition)
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

        if (IsServerInitialized)
            ServerManager.Despawn(gameObject);
    }
}
