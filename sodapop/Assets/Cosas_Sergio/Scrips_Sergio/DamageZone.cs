using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class DamageZone : NetworkBehaviour
{
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float tickInterval = 1f;

    private HashSet<Health> objectsInside = new HashSet<Health>();

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
}
