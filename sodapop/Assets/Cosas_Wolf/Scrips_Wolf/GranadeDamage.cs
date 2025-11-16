using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GranadeDamage : NetworkBehaviour
{
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float lifetime = 3f; // tiempo que dura la zona

    private HashSet<Health> objectsInside = new HashSet<Health>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Health health = other.GetComponent<Health>();
        if (health != null && !objectsInside.Contains(health))
        {
            objectsInside.Add(health);
            StartCoroutine(DamageRoutine(health));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            objectsInside.Remove(health);
        }
    }

    private IEnumerator DamageRoutine(Health health)
    {
        while (objectsInside.Contains(health))
        {
            health.TakeDamage(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        GetComponent<NetworkObject>().Despawn();
    }
}
