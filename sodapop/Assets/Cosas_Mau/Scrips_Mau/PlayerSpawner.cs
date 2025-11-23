using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
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
}
