using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameRulesManager : NetworkBehaviour
{
    public static GameRulesManager Instance { get; private set; }

    [SerializeField] private int targetScore = 10;
    [SerializeField] private List<Transform> spawnPoints = new();
    private int _nextSpawnIdx;

    public readonly SyncVar<int> CurrentScore = new SyncVar<int>();
    public readonly SyncVar<bool> GameOver = new SyncVar<bool>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (!base.IsServerInitialized) return;
        
        CurrentScore.Value = 0;
        GameOver.Value = false;
    }

    public bool CanRespawnServer() => base.IsServerInitialized && !GameOver.Value;

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int amount)
    {
        // No need for IsServer check in ServerRpc, but keeping logic safe
        CurrentScore.Value = Mathf.Max(0, CurrentScore.Value + Mathf.Abs(amount));
        if (CurrentScore.Value >= targetScore)
        {
            GameOver.Value = true;
        }
    }

    public Transform GetNextSpawn()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return null;
        var t = spawnPoints[_nextSpawnIdx % spawnPoints.Count];
        _nextSpawnIdx++;
        return t;
    }
}
