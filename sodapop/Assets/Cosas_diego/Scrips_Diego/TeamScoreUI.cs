using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TeamScoreUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text teamAText;
    [SerializeField] private TMP_Text teamBText;

    private void Start()
    {
        TeamScoreManager.Instance.TeamAScore.OnValueChanged += UpdateUI;
        TeamScoreManager.Instance.TeamBScore.OnValueChanged += UpdateUI;

        UpdateUI(0, 0);
    }

    private void UpdateUI(int previous, int current)
    {
        teamAText.text = "Team A: " + TeamScoreManager.Instance.TeamAScore.Value;
        teamBText.text = "Team B: " + TeamScoreManager.Instance.TeamBScore.Value;
    }
}
