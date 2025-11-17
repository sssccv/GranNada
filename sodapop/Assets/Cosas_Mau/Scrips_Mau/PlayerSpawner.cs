using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Spawns del Equipo A")]
    [SerializeField] private Transform[] teamASpawnPoints;

    [Header("Spawns del Equipo B")]
    [SerializeField] private Transform[] teamBSpawnPoints;

    public Vector3 GetSpawnPoint(ulong clientId)
    {
        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        Team team = playerObj.GetComponent<TeamComponent>().PlayerTeam.Value;

        if (team == Team.TeamA && teamASpawnPoints.Length > 0)
            return teamASpawnPoints[Random.Range(0, teamASpawnPoints.Length)].position;

        if (team == Team.TeamB && teamBSpawnPoints.Length > 0)
            return teamBSpawnPoints[Random.Range(0, teamBSpawnPoints.Length)].position;

        Debug.LogWarning("No SpawnPoints encontrados");
        return Vector3.zero;
    }
}



