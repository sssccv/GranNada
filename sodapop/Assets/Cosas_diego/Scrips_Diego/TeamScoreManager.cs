using Unity.Netcode;
using UnityEngine;
using System;

public class TeamScoreManager : NetworkBehaviour
{
    public static TeamScoreManager Instance;

    public NetworkVariable<int> TeamAScore = new NetworkVariable<int>();
    public NetworkVariable<int> TeamBScore = new NetworkVariable<int>();

    [SerializeField] private int scoreToWin = 50;

    public Action<Team> OnTeamWin;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        TeamAScore.Value = 0;
        TeamBScore.Value = 0;
    }

    public void AddScore(Team team, int amount)
    {
        if (!IsServer) return;

        if (team == Team.TeamA)
            TeamAScore.Value += amount;
        else
            TeamBScore.Value += amount;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (TeamAScore.Value >= scoreToWin)
        {
            OnTeamWin?.Invoke(Team.TeamA);
        }
        else if (TeamBScore.Value >= scoreToWin)
        {
            OnTeamWin?.Invoke(Team.TeamB);
        }
    }
}
