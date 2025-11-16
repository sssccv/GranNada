using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    public int damage = 20;
    private ulong attackerId;
    public float lifetime = 5f;

    public GameObject explosionEffect;

    public void Initialize(ulong ownerId, int dmg)
    {
        attackerId = ownerId;
        damage = dmg;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        if (collision.collider.TryGetComponent<Health>(out Health health))
        {
            // evitar auto daño
            if (health.OwnerClientId != attackerId)
            {
                health.TakeDamage(damage, attackerId);
            }
        }

        Destroy(gameObject);
    }
}
