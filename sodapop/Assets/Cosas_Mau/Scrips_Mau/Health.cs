using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [field: SerializeField] public int maxHealth { get; private set; } = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private bool isDead;
    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        currentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (isDead) { return; }

        int newHealth = currentHealth.Value + value;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if(currentHealth.Value == 0)
        {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
}


//Variables y Propiedades
	//•	maxHealth (solo lectura pública, con serialización para el editor): Vida máxima que puede tener este objeto, configurada inicialmente en 100.
	//•	currentHealth (NetworkVariable): Vida actual sincronizada en red, que se replica automáticamente a los clientes.
	//•	isDead: Estado interno booleano para marcar si el objeto ya está “muerto” y evitar efectos múltiples.
	//•	OnDie (Action): Evento público que notifica a otros sistemas cuando este objeto muere.

//Ciclo de Vida en Red método OnNetworkSpawn
	//•	Solo el servidor inicializa currentHealth al máximo cuando el objeto aparece en la red.
	//•	Los clientes reciben esta información automáticamente por la NetworkVariable.

//Modificación de Salud
	//•	TakeDamage(int damageValue)
	//•	Resta vida llamando a ModifyHealth con un valor negativo.
	//•	RestoreHealth(int healValue)
	//•	Cura sumando vida con ModifyHealth.

	//Método privado ModifyHealth(int value)
	//•	Si ya está muerto, ignora cualquier cambio.
	//•	Calcula nueva salud sumando el valor recibido (positivo o negativo).
	//•	Usa Mathf.Clamp para asegurarse que la salud permanezca entre 0 y maxHealth.
	//•	Si la salud llega a 0, dispara el evento OnDie y marca isDead para evitar reejecuciones.
//Resumen
//Este script es una implementación estándar para gestionar salud en un juego multijugador con Unity Netcode, usando la sincronización automática de NetworkVariables y control autoritario del servidor para mantener coherencia. Está preparado para integrar fácilmente acciones al morir mediante eventos sin acoplar lógica concreta al sistema de salud.//