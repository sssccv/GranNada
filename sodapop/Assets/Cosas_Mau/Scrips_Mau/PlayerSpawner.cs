using UnityEngine;
using FishNet.Connection; // Necesitas esto para NetworkConnection
using FishNet.Managing; // Necesitas esto para NetworkManager
using FishNet.Object; // Necesitas esto para NetworkObject (el objeto del jugador)

// Mantenemos MonoBehaviour ya que es un componente de escena no-red
public class PlayerSpawner : MonoBehaviour
{
    // Obtenemos una referencia al NetworkManager de FishNet
    // Lo ideal es asignarlo en el Inspector, si no, lo buscaremos en Start.
    private NetworkManager _networkManager;

    [Header("Spawns del Equipo A")]
    [SerializeField] private Transform[] teamASpawnPoints;

    [Header("Spawns del Equipo B")]
    [SerializeField] private Transform[] teamBSpawnPoints;

    private void Start()
    {
        // Buscamos el NetworkManager de FishNet al inicio
        if (_networkManager == null)
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("❌ FishNet NetworkManager no encontrado en la escena.");
                enabled = false;
            }
        }
    }

    /// <summary>
    /// Servidor (Host) function. Determines which spawn point this player should use.
    /// </summary>
    // CAMBIO CLAVE: Usamos 'int' para el clientId (OwnerId de FishNet)
    public Vector3 GetSpawnPoint(int clientId)
    {
        // FishNet: Utilizamos _networkManager.IsServerInitialized en lugar de NetworkManager.Singleton.IsServer
        if (!_networkManager.ServerManager.IsServerStarted)
        {
            Debug.LogWarning("❌ GetSpawnPoint llamado antes de que el servidor esté listo. Esto SOLO debe llamarse en el servidor.");
            return Vector3.zero;
        }

        // Obtener la conexión del jugador.
        // FishNet mantiene la lista de conexiones en ServerManager.Clients.
        NetworkConnection conn;

        // Intentamos obtener la conexión del cliente usando su ClientId (que es int)
        if (!_networkManager.ServerManager.Clients.TryGetValue(clientId, out conn))
        {
            Debug.LogError($"❌ No existe una conexión activa para clientId {clientId}");
            return Vector3.zero;
        }

        // Obtener el objeto del jugador.
        // El objeto de red principal del cliente (NetworkObject) está en la propiedad PlayerObject de la conexión.
        NetworkObject playerObj = conn.FirstObject;

        if (playerObj == null)
        {
            Debug.LogError($"❌ PlayerObject es null para el cliente {clientId} en GetSpawnPoint. ¿El jugador ha sido spawned?");
            return Vector3.zero;
        }

        // Obtener el equipo
        var teamComponent = playerObj.GetComponent<TeamComponent>();

        if (teamComponent == null)
        {
            Debug.LogError("❌ TeamComponent no encontrado en PlayerObject. Asegúrate de que tu objeto de jugador lo tenga.");
            return Vector3.zero;
        }

        // Asumiendo que PlayerTeam.Value es accesible (si es un SyncVar)
        // Nota: Asegúrate de que TeamComponent y Team estén definidos y accesibles.
        Team team = teamComponent.PlayerTeam.Value;

        // Seleccionar spawn según el equipo
        if (team == Team.TeamA && teamASpawnPoints.Length > 0)
            return teamASpawnPoints[Random.Range(0, teamASpawnPoints.Length)].position;

        if (team == Team.TeamB && teamBSpawnPoints.Length > 0)
            return teamBSpawnPoints[Random.Range(0, teamBSpawnPoints.Length)].position;

        Debug.LogWarning($"⚠ No hay SpawnPoints configurados para el equipo {team}");
        return Vector3.zero;
    }

    // Eliminamos el método obsoleto que generaba la excepción.
    // internal Vector3 GetSpawnPoint(int ownerId)
    // {
    //     throw new System.NotImplementedException();
    // }
}

/*using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawns del Equipo A")]
    [SerializeField] private Transform[] teamASpawnPoints;

    [Header("Spawns del Equipo B")]
    [SerializeField] private Transform[] teamBSpawnPoints;

    /// <summary>
    /// Host-only function. Determines which spawn point this player should use.
    /// </summary>
    public Vector3 GetSpawnPoint(ulong clientId)
    {
        // Solo el HOST tiene ConnectedClients
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("❌ GetSpawnPoint llamado por cliente. Esto SOLO debe llamarse en el servidor.");
            return Vector3.zero;
        }

        // Obtener el objeto del jugador
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogError($"❌ No existe un jugador con clientId {clientId}");
            return Vector3.zero;
        }

        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        if (playerObj == null)
        {
            Debug.LogError("❌ PlayerObject es null en GetSpawnPoint.");
            return Vector3.zero;
        }

        // Obtener el equipo
        var teamComponent = playerObj.GetComponent<TeamComponent>();

        if (teamComponent == null)
        {
            Debug.LogError("❌ TeamComponent no encontrado en PlayerObject.");
            return Vector3.zero;
        }

        Team team = teamComponent.PlayerTeam.Value;

        // Seleccionar spawn según el equipo
        if (team == Team.TeamA && teamASpawnPoints.Length > 0)
            return teamASpawnPoints[Random.Range(0, teamASpawnPoints.Length)].position;

        if (team == Team.TeamB && teamBSpawnPoints.Length > 0)
            return teamBSpawnPoints[Random.Range(0, teamBSpawnPoints.Length)].position;

        Debug.LogWarning($"⚠ No hay SpawnPoints configurados para el equipo {team}");
        return Vector3.zero;
    }

    internal Vector3 GetSpawnPoint(int ownerId)
    {
        throw new System.NotImplementedException();
    }
}*/
