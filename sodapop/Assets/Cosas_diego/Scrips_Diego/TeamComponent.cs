using FishNet.Object; // Clase base NetworkBehaviour
using FishNet.Object.Synchronizing; // Necesario para SyncVar<T>
using UnityEngine;

public enum Team
{
    TeamA,
    TeamB
}

// Heredamos de NetworkBehaviour de FishNet
public class TeamComponent : NetworkBehaviour
{
    // CORRECCIÓN CLAVE: Usamos la clase SyncVar<Team>
    // Ya no necesitas el atributo [SyncVar] ni el parámetro Group.
    public readonly SyncVar<Team> PlayerTeam = new SyncVar<Team>();

    // Utilizamos OnStartServer para la lógica que solo debe ejecutarse en el servidor.
    public override void OnStartServer()
    {
        base.OnStartServer();

        // FishNet: Utilizamos IsServerInitialized.
        if (!base.IsServerInitialized) return; 

        // FishNet usa OwnerId (int) en lugar de OwnerClientId (ulong).
        // Para asignar el valor, se usa la propiedad .Value, igual que en Netcode.
        PlayerTeam.Value = (base.OwnerId % 2 == 0) ? Team.TeamA : Team.TeamB;
    }
}

/*using Unity.Netcode;
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
}*/