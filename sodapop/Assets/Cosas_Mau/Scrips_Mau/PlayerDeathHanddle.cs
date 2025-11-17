using Unity.Netcode;
using UnityEngine;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private float respawnDelay = 3f;
    //[SerializeField] private Animator animator;

    private PlayerSpawner spawner;

    private Health health;
    private PlayerMovement movement;
    private PlayerShooter shooter;

    private bool isRespawning = false;

    public override void OnNetworkSpawn()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        shooter = GetComponent<PlayerShooter>();

        health.OnDie += HandleDeath;
    }

    public void SetSpawner(PlayerSpawner spawner)
    {
        this.spawner = spawner;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= HandleDeath;
    }

    private void HandleDeath(Health h)
    {
        if (!IsServer || isRespawning) return;

        isRespawning = true;

        PlayDeathClientRpc();
        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (TeamScoreManager.Instance.IsMatchOver)
            yield break;

        RespawnServerRpc();
    }

    [ServerRpc]
    private void RespawnServerRpc()
    {
        if (spawner == null)
        {
            Debug.LogError("Spawner no asignado");
            return;
        }

        health.currentHealth.Value = health.maxHealth;

        Vector3 spawnPos = spawner.GetSpawnPoint(OwnerClientId);
        transform.position = spawnPos;

        RespawnClientRpc();
        isRespawning = false;
    }

    [ClientRpc]
    private void PlayDeathClientRpc()
    {
        //animator.SetTrigger("Death");

        if (IsOwner)
        {
            movement.enabled = false;
            shooter.enabled = false;
        }
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        //animator.SetTrigger("Respawn");

        if (IsOwner)
        {
            movement.enabled = true;
            shooter.enabled = true;
        }
    }
}



