using Unity.Netcode;
using UnityEngine;

public enum Team
{
    TeamA,
    TeamB
}

public class TeamComponent : NetworkBehaviour
{
    public NetworkVariable<Team> PlayerTeam = new NetworkVariable<Team>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Asignación automática de equipo
        // Puedes cambiarlo por matchmaking real
        PlayerTeam.Value = (OwnerClientId % 2 == 0) ? Team.TeamA : Team.TeamB;
    }
}
