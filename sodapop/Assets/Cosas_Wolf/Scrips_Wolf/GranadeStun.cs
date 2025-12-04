using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;

public class GranadeStun : NetworkBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;   // reduce velocidad a la mitad
    [SerializeField] private float reducedJumpMultiplier = 0.5f; // salto más bajo
    [SerializeField] private float stunDuration = 2f;       // cuánto dura el efecto en el jugador
    [SerializeField] private float lifetime = 3f;           // tiempo que dura la zona en escena

    // Jugadores dentro de la zona
    private HashSet<PlayerMovement> playersInside = new HashSet<PlayerMovement>();

    // Diccionario para guardar valores originales de cada jugador
    private Dictionary<PlayerMovement, (float speed, float sprint, float jump)> originalValues
        = new Dictionary<PlayerMovement, (float, float, float)>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && !playersInside.Contains(player))
        {
            playersInside.Add(player);

            // Guardamos valores originales
            if (!originalValues.ContainsKey(player))
            {
                originalValues[player] = (player.movementSpeed, player.sprintMultiplier, player.jumpHeight);
            }

            StartCoroutine(ApplyStun(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServerInitialized) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && playersInside.Contains(player))
        {
            playersInside.Remove(player);

            // Restauramos valores originales al salir
            if (originalValues.ContainsKey(player))
            {
                var values = originalValues[player];
                player.movementSpeed = values.speed;
                player.sprintMultiplier = values.sprint;
                player.jumpHeight = values.jump;

                originalValues.Remove(player);
            }
        }
    }

    private IEnumerator ApplyStun(PlayerMovement player)
    {
        // Aplicamos el efecto
        player.movementSpeed *= slowMultiplier;
        player.sprintMultiplier = 1f; // desactiva sprint
        player.jumpHeight *= reducedJumpMultiplier;

        // Esperamos la duración del stun
        yield return new WaitForSeconds(stunDuration);

        // Restauramos valores originales
        if (originalValues.ContainsKey(player))
        {
            var values = originalValues[player];
            player.movementSpeed = values.speed;
            player.sprintMultiplier = values.sprint;
            player.jumpHeight = values.jump;

            originalValues.Remove(player);
        }

        playersInside.Remove(player);
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        // Al destruir la zona, restauramos valores originales de todos los jugadores dentro
        foreach (var player in playersInside)
        {
            if (originalValues.ContainsKey(player))
            {
                var values = originalValues[player];
                player.movementSpeed = values.speed;
                player.sprintMultiplier = values.sprint;
                player.jumpHeight = values.jump;
            }
        }

        playersInside.Clear();
        originalValues.Clear();

        if (IsServerInitialized)
            ServerManager.Despawn(gameObject);
    }
}
