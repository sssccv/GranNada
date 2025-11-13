using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthImageUI;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        health.currentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.currentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        health.currentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth)

    {
        healthImageUI.fillAmount = (float)newHealth / health.maxHealth;
    }
}

//Variables y Referencias
	//•	health: Referencia al componente Health que gestiona la lógica de vida del objeto.
	//•	healthImageUI: Componente Image de Unity que representa visualmente la barra de vida mediante el fillAmount.

//Ciclo de Vida y Suscripción OnNetworkSpawn
	//•	Solo ejecuta la lógica en el cliente.
	//•	Se suscribe al evento OnValueChanged de la NetworkVariable currentHealth del Health, para recibir actualizaciones cuando la salud cambie.
	//•	Llama de inmediato a HandleHealthChanged para inicializar el UI con el valor actual.
	//•	Elimina la suscripción cuando el objeto desaparece, evitando fugas de memoria y referencias inválidas.

//Actualización Visual HandleHealthChanged
	//•	Actualiza la barra de vida en UI escalando el fillAmount entre 0 (sin vida) y 1 (vida completa), según el valor actual y máximo de salud.
	//•	Lógica sencilla y directa para representar visualmente la vida del objeto en la interfaz del usuario multijugador.
