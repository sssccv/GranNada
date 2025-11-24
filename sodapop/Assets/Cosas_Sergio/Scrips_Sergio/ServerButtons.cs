using Unity.Netcode;
using Unity.Netcode.Transports.UTP; // Necesario
using UnityEngine;
using UnityEngine.UI; // Para usar InputFields


public class ServerButtons : MonoBehaviour
{
    public TMPro.TMP_InputField ipAddressInput;

    private void Start()
    {
        // Por defecto ponemos localhost para pruebas rápidas
        ipAddressInput.text = "127.0.0.1";
    }

    public void HostServer()
    {
        NetworkManager.Singleton.StartHost();

    }
    public void JoinServer()
    {
        // 1. Obtenemos el componente de transporte
        var transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // 2. Le decimos a qué IP conectarse (la que escribiste en el InputField)
        // El puerto por defecto de Netcode es 7777, asegúrate de no haberlo cambiado
        transporte.SetConnectionData(
            ipAddressInput.text,  // La IP del Host (ej: 192.168.1.50)
            7777                  // El puerto (ushort)
        );

        // 3. Iniciamos el cliente
        NetworkManager.Singleton.StartClient();
    }

    public void CloseServer()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
