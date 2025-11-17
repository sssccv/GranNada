using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameScoreManager : NetworkBehaviour
{
    public static GameScoreManager Instance { get; private set; }

    [SerializeField] private int targetScore = 10;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    public int TargetScore => targetScore;
    public NetworkVariable<int> CurrentScore = new NetworkVariable<int>(0);
    private int _nextSpawnIndex;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanRespawnServer()
    {
        // Solo el servidor decide si se puede respawnear.
        return CurrentScore.Value < targetScore;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreServerRpc(int amount)
    {
        CurrentScore.Value = Mathf.Max(0, CurrentScore.Value + amount);
    }

    public Transform GetSpawnPoint()
    {
        // Fallback si no hay puntos configurados
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return null;
        }

        // Round-robin
        var t = spawnPoints[_nextSpawnIndex % spawnPoints.Count];
        _nextSpawnIndex++;
        return t;
    }
}
