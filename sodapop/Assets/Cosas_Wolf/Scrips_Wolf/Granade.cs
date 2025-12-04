using System.Collections;
using UnityEngine;
using FishNet.Object;          // Para NetworkBehaviour, NetworkObject
using FishNet.Object.Synchronizing; // Para RPCs

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

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServerInitialized) return;

        Explode();
    }

    private void Explode()
    {
        // Efecto visual sincronizado en todos los clientes
        ExplodeObserversRpc(transform.position);

        // Zona de daño en red (solo servidor)
        if (damageZonePrefab != null)
        {
            GameObject zone = Instantiate(damageZonePrefab, transform.position, Quaternion.identity);

            var damageComp = zone.GetComponent<GranadeDamage>();
            if (damageComp != null)
                damageComp.Initialize(attackerId);
            else
                Debug.LogError("❌ EL PREFAB damageZonePrefab NO TIENE 'GranadeDamage'");

            // Spawnear en red
            ServerManager.Spawn(zone);
        }

        // Despawn en red
        ServerManager.Despawn(gameObject);
    }

    [ObserversRpc]
    private void ExplodeObserversRpc(Vector3 explosionPosition)
    {
        // Este código se ejecuta en todos los clientes
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
