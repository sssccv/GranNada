using UnityEngine;

public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private TMPro.TMP_Text winnerText;

    private void Start()
    {
        TeamScoreManager.Instance.OnTeamWin += ShowWinner;
    }

    private void ShowWinner(Team team)
    {
        winScreen.SetActive(true);
        winnerText.text = team + " Wins!";
    }
}
