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

    [HideInInspector] public bool spawnerAssigned = false;
    [HideInInspector] public Vector3 initialSpawnPosition;

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
        spawnerAssigned = true;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= HandleDeath;
    }

    private void HandleDeath(Health h)
    {
        if (!IsServer || isRespawning || !spawnerAssigned)
            return;

        isRespawning = true;

        PlayDeathClientRpc();
        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (!spawnerAssigned)
            yield break;

        health.currentHealth.Value = health.maxHealth;
        health.ResetDeathFlag();

        // 🔥 Ahora YA NO movemos al jugador desde el servidor
        // Solo avisamos y el cliente dueños se teletransporta
        RespawnClientRpc(initialSpawnPosition);

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
    private void RespawnClientRpc(Vector3 respawnPos)
    {
        if (!IsOwner) return;

        // 🔥 Teleport garantizado para ClientNetworkTransform
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = respawnPos;
            cc.enabled = true;
        }
        else
        {
            transform.position = respawnPos;
        }

        movement.enabled = true;
        shooter.enabled = true;

        movement.ResetMovementState();
    }
}
