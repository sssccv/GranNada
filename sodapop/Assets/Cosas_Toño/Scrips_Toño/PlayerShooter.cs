using System;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    public InputReader inputReader;
    public Transform firePoint;
    public GameObject[] grenadePrefabs;

    [Header("Settings")]
    public float shootForce = 10f;          // Fuerza horizontal principal
    public float upwardForce = 5f;          // Fuerza vertical para crear la parábola
    public float fireRate = 0.5f;

    private bool isFiring;
    private float lastFireTime;

    private void OnEnable()
    {
        inputReader.OnFireEvent += HandleFire;
    }

    private void OnDisable()
    {
        inputReader.OnFireEvent -= HandleFire;
    }

    private void HandleFire(bool isPressed)
    {
        isFiring = isPressed;
    }

    private void Update()
    {
        if (isFiring && Time.time - lastFireTime > fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    private void Shoot()
    {
        if (firePoint == null || grenadePrefabs == null || grenadePrefabs.Length == 0)
        {
            Debug.LogWarning("Faltan referencias o no hay prefabs en PlayerShooter");
            return;
        }

        GameObject selectedPrefab = grenadePrefabs[UnityEngine.Random.Range(0, grenadePrefabs.Length)];
        GameObject grenade = Instantiate(selectedPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Direcciones
            Vector3 forward = firePoint.forward * shootForce;
            Vector3 upward = Vector3.up * upwardForce;

            // Combinamos las fuerzas para formar una parábola
            rb.AddForce(forward + upward, ForceMode.Impulse);
        }
    }
}
