using UnityEngine;
using TMPro;
// Ya no necesitamos los usings de FishNet/Netcode en este script, 
// solo TMPro y UnityEngine.
// using Unity.Netcode; // Lo eliminamos

// CAMBIO CLAVE: Cambiamos a MonoBehaviour, ya que solo escucha eventos
public class TeamScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text teamAText;
    [SerializeField] private TMP_Text teamBText;

    private void Start()
    {
        // 🚨 Asegúrate de que TeamScoreManager.Instance ya esté inicializado antes de llamar a Start() 
        // (por ejemplo, asegurándote de que TeamScoreManager se ejecute primero).
        
        // CORRECCIÓN CLAVE: En FishNet, para SyncVar<T>, el evento es OnChange.
        // El delegado de OnChange requiere tres parámetros (anterior, nuevo, esServidor).
        TeamScoreManager.Instance.TeamAScore.OnChange += UpdateUI;
        TeamScoreManager.Instance.TeamBScore.OnChange += UpdateUI;

        // Llamamos a UpdateUI inicialmente para establecer los textos
        // Nota: Solo necesitas llamar a UpdateUI, no pasarle argumentos si usa los valores actuales del Manager.
        UpdateUI(default, default, false); // Llamada inicial con valores dummy para activar la actualización.
    }

    private void OnDestroy()
    {
        // Es una buena práctica desuscribirse de los eventos al destruir el objeto.
        if (TeamScoreManager.Instance != null)
        {
            TeamScoreManager.Instance.TeamAScore.OnChange -= UpdateUI;
            TeamScoreManager.Instance.TeamBScore.OnChange -= UpdateUI;
        }
    }

    /// <summary>
    /// Actualiza la UI cuando cambia una SyncVar.
    /// FishNet requiere (T previous, T current, bool asServer)
    /// </summary>
    private void UpdateUI(int previous, int current, bool asServer)
    {
        // La lógica de la actualización de texto se mantiene igual, usando .Value
        // (Asumiendo que TeamScoreManager ya usa SyncVar<int> con .Value)
        teamAText.text = "Team A: " + TeamScoreManager.Instance.TeamAScore.Value;
        teamBText.text = "Team B: " + TeamScoreManager.Instance.TeamBScore.Value;
    }
}

/*using UnityEngine;
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
}*/