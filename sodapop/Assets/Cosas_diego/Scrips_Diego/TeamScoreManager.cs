using FishNet.Object; // Clase base NetworkBehaviour
using FishNet.Object.Synchronizing; // Necesario para SyncVar<T>
using UnityEngine;
using System;

// Asegúrate de que el enum Team sea accesible (si está en otro archivo, ¡genial!)
// public enum Team { TeamA, TeamB } 

// Heredamos de NetworkBehaviour de FishNet
public class TeamScoreManager : NetworkBehaviour
{
    public static TeamScoreManager Instance;

    // CAMBIO CLAVE: NetworkVariable<int> se reemplaza por SyncVar<int>
    // Usamos el constructor sin parámetros.
    public SyncVar<int> TeamAScore = new SyncVar<int>();
    public SyncVar<int> TeamBScore = new SyncVar<int>();

    [SerializeField] private int scoreToWin = 50;

    // La lógica de eventos C# local (Action) sigue siendo la misma
    public Action<Team> OnTeamWin;

    // Esta variable solo necesita ser actualizada por el servidor,
    // pero si quieres que los clientes sepan si el partido terminó,
    // conviértela en una SyncVar. Si no, solo el servidor la usa para su lógica.
    // La convertiré en SyncVar para que los clientes puedan reaccionar al fin del partido.
    public SyncVar<bool> IsMatchOver = new SyncVar<bool>(false); 


    private void Awake()
    {
        // El patrón Singleton se mantiene
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Usamos OnStartServer para inicializar valores en el servidor
    public override void OnStartServer()
    {
        base.OnStartServer();

        // FishNet: Utilizamos IsServerInitialized.
        if (!base.IsServerInitialized) return;

        // Inicializamos los puntajes en el servidor. 
        // Accedemos a los valores usando .Value
        TeamAScore.Value = 0;
        TeamBScore.Value = 0;
        IsMatchOver.Value = false; // También inicializamos la variable de fin de partida
    }

    /// <summary>
    /// Servidor-only function para añadir puntaje y verificar la condición de victoria.
    /// </summary>
    public void AddScore(Team team, int amount)
    {
        // FishNet: Verificamos si estamos en el servidor usando IsServerInitialized
        if (!base.IsServerInitialized) 
        {
            Debug.LogWarning("❌ AddScore solo puede ser llamado desde el servidor.");
            return;
        }

        if (team == Team.TeamA)
            TeamAScore.Value += amount;
        else
            TeamBScore.Value += amount;

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // La condición IsMatchOver debe usar .Value ya que ahora es un SyncVar
        if (IsMatchOver.Value) return;

        if (TeamAScore.Value >= scoreToWin)
        {
            IsMatchOver.Value = true;
            // El Action/Evento solo se invoca en el servidor (donde se ejecuta CheckWinCondition)
            OnTeamWin?.Invoke(Team.TeamA);
        }
        else if (TeamBScore.Value >= scoreToWin)
        {
            IsMatchOver.Value = true;
            OnTeamWin?.Invoke(Team.TeamB);
        }
    }

    // Opcional: Si quieres que los clientes se enteren cuando termina el juego,
    // puedes usar un delegado de cambio de SyncVar.

    /*
    public override void OnStartClient()
    {
        base.OnStartClient();
        IsMatchOver.OnChange += HandleMatchOverChanged;
    }

    private void HandleMatchOverChanged(bool prev, bool next, bool asServer)
    {
        // Esta lógica se ejecuta en todos los clientes cuando IsMatchOver.Value cambia
        if (next)
        {
            Debug.Log("¡Partido Terminado!");
            // Aquí puedes ejecutar lógica de UI para mostrar la pantalla de victoria/derrota.
        }
    }
    */
}

/*using Unity.Netcode;
using UnityEngine;
using System;

public class TeamScoreManager : NetworkBehaviour
{
    public static TeamScoreManager Instance;

    public NetworkVariable<int> TeamAScore = new NetworkVariable<int>();
    public NetworkVariable<int> TeamBScore = new NetworkVariable<int>();

    [SerializeField] private int scoreToWin = 50;

    public Action<Team> OnTeamWin;

    public bool IsMatchOver = false;


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
    if (IsMatchOver) return;

    if (TeamAScore.Value >= scoreToWin)
    {
        IsMatchOver = true;
        OnTeamWin?.Invoke(Team.TeamA);
    }
    else if (TeamBScore.Value >= scoreToWin)
    {
        IsMatchOver = true;
        OnTeamWin?.Invoke(Team.TeamB);
    }
    }

}*/