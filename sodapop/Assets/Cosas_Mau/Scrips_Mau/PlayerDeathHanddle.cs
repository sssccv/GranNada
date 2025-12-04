using FishNet.Object;
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

    private void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        shooter = GetComponent<PlayerShooter>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        health.OnDie += HandleDeath;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (health != null)
            health.OnDie -= HandleDeath;
    }

    public void SetSpawner(PlayerSpawner spawner)
    {
        this.spawner = spawner;
        spawnerAssigned = true;
    }

    private void HandleDeath(Health h)
    {
        if (!base.IsServerInitialized || isRespawning || !spawnerAssigned)
            return;

        isRespawning = true;

        PlayDeathObserversRpc();
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
        RespawnObserversRpc(initialSpawnPosition);

        isRespawning = false;
    }

    [ObserversRpc]
    private void PlayDeathObserversRpc()
    {
        if (base.IsOwner)
        {
            movement.enabled = false;
            shooter.enabled = false;
        }
    }

    [ObserversRpc]
    private void RespawnObserversRpc(Vector3 respawnPos)
    {
        if (!base.IsOwner) return;

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
