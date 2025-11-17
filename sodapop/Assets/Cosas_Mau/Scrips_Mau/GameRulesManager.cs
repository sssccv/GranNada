using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameRulesManager : NetworkBehaviour
{
    public static GameRulesManager Instance { get; private set; }

    [SerializeField] private int targetScore = 10;
    [SerializeField] private List<Transform> spawnPoints = new();
    private int _nextSpawnIdx;

    public NetworkVariable<int> CurrentScore = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> GameOver = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentScore.Value = 0;
            GameOver.Value = false;
        }
    }

    public bool CanRespawnServer() => IsServer && !GameOver.Value;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreServerRpc(int amount)
    {
        if (!IsServer) return;
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
