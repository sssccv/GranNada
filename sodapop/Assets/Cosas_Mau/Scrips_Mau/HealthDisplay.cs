using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthImageUI;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsClientInitialized) { return; }

        health.currentHealth.OnChange += HandleHealthChanged;
        UpdateHealthUI(health.currentHealth.Value);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (!base.IsClientInitialized) { return; }

        health.currentHealth.OnChange -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth, bool asServer)
    {
        UpdateHealthUI(newHealth);
    }

    private void UpdateHealthUI(int currentHealth)
    {
        healthImageUI.fillAmount = (float)currentHealth / health.maxHealth;
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
