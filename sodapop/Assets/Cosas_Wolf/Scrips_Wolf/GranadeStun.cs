using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GranadeStun : NetworkBehaviour
{
    [Header("Stun Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;   // reduce velocidad a la mitad
    [SerializeField] private float reducedJumpMultiplier = 0.5f; // salto más bajo
    [SerializeField] private float stunDuration = 2f;       // cuánto dura el efecto en el jugador
    [SerializeField] private float lifetime = 3f;           // tiempo que dura la zona en escena

    private HashSet<PlayerMovement> playersInside = new HashSet<PlayerMovement>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(DestroyAfterLifetime());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && !playersInside.Contains(player))
        {
            playersInside.Add(player);
            StartCoroutine(ApplyStun(player));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null && playersInside.Contains(player))
        {
            playersInside.Remove(player);
        }
    }

    private IEnumerator ApplyStun(PlayerMovement player)
    {
        // Guardamos valores originales
        float originalSpeed = player.movementSpeed;
        float originalSprint = player.sprintMultiplier;
        float originalJump = player.jumpHeight;

        // Aplicamos el efecto
        player.movementSpeed *= slowMultiplier;
        player.sprintMultiplier = 1f; // desactiva sprint
        player.jumpHeight *= reducedJumpMultiplier;

        // Esperamos la duración del stun
        yield return new WaitForSeconds(stunDuration);

        // Restauramos valores originales
        player.movementSpeed = originalSpeed;
        player.sprintMultiplier = originalSprint;
        player.jumpHeight = originalJump;
    }

    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        // Al destruir la zona, quitamos el efecto a todos los jugadores dentro
        foreach (var player in playersInside)
        {
            // Restauramos valores originales por seguridad
            player.movementSpeed = 10f; // valor por defecto
            player.sprintMultiplier = 2f;
            player.jumpHeight = 2f;
        }

        GetComponent<NetworkObject>().Despawn();
    }
}
