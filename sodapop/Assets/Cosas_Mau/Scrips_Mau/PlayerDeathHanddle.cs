using Unity.Netcode;
using UnityEngine;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private float respawnDelay = 3f;

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

        PlayDeathClientRpc();  // SOLO desactiva movimiento/disparo
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
            Debug.LogError("❌ Spawner no asignado en PlayerDeathHandler.");
            return;
        }

        // Restaurar salud
        health.currentHealth.Value = health.maxHealth;

        // Obtener nueva posición de respawn
        Vector3 spawnPos = spawner.GetSpawnPoint(OwnerClientId);

        // Teletransportar al jugador
        transform.position = spawnPos;

        // Avisar al cliente para reactivar movimiento
        RespawnClientRpc();

        isRespawning = false;
    }

    [ClientRpc]
    private void PlayDeathClientRpc()
    {
        if (IsOwner)
        {
            movement.enabled = false;
            shooter.enabled = false;
        }
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        if (IsOwner)
        {
            // 🔥 Reset CharacterController completo
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                cc.enabled = true;
            }

            // Reactivar sistemas
            movement.enabled = true;
            shooter.enabled = true;

            // Limpiar estados previos
            movement.ResetMovementState();
        }
    }
}




