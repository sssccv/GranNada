using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    public int maxHealth => _maxHealth;

    public readonly SyncVar<int> currentHealth = new SyncVar<int>();
    private bool isDead = false;

    public event System.Action<Health> OnDie;

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!base.IsServerInitialized) return;
        currentHealth.Value = maxHealth;
    }

    // Server-only damage method (call from ServerRpc or server code)
    public void TakeDamage(int amount, int attackerId = -1)
    {
        if (!base.IsServerInitialized) return;
        if (isDead) return;

        ModifyHealth(-Mathf.Abs(amount));

        if (currentHealth.Value == 0)
        {
            // award points if attacker valid
            if (attackerId != -1 && base.NetworkManager.ClientManager.Clients.TryGetValue(attackerId, out NetworkConnection attackerConnection))
            {
                var killerObj = attackerConnection.FirstObject;
                if (killerObj != null)
                {
                    var teamComp = killerObj.GetComponent<TeamComponent>();
                    if (teamComp != null)
                    {
                        Team killerTeam = teamComp.PlayerTeam.Value;
                        TeamScoreManager.Instance?.AddScore(killerTeam, 1);
                    }
                }
            }
        }
    }

    public void RestoreHealth(int healValue)
    {
        if (!base.IsServerInitialized) return;
        ModifyHealth(Mathf.Abs(healValue));
    }

    private void ModifyHealth(int delta)
    {
        if (isDead) return;

        int newHealth = currentHealth.Value + delta;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value == 0)
        {
            isDead = true;
            OnDie?.Invoke(this);
        }
    }

    // Called by server on respawn to allow further damage
    public void ResetDeathFlag()
    {
        isDead = false;
    }
}

