using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionEffect; // opcional, puedes dejarlo vacío

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
