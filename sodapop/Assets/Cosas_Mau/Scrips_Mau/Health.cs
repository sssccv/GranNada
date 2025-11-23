using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;
    public int maxHealth => _maxHealth;

    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0);
    private bool isDead = false;

    public event System.Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        currentHealth.Value = maxHealth;
    }

    // Server-only damage method (call from ServerRpc or server code)
    public void TakeDamage(int amount, ulong attackerId = ulong.MaxValue)
    {
        if (!IsServer) return;
        if (isDead) return;

        ModifyHealth(-Mathf.Abs(amount));

        if (currentHealth.Value == 0)
        {
            // award points if attacker valid
            if (attackerId != ulong.MaxValue && NetworkManager.Singleton.ConnectedClients.ContainsKey(attackerId))
            {
                var killerObj = NetworkManager.Singleton.ConnectedClients[attackerId].PlayerObject;
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
        if (!IsServer) return;
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

