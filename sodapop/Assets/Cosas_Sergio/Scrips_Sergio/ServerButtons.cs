using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class ServerButtons : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject button;

    private void Start()
    {
        panel.SetActive(false);
        button.SetActive(true);
    }

    public void ActivatePanel()
    {
        panel.SetActive(true);
        button.SetActive(false);
    }

    public void DeactivatePanel()
    {
        panel.SetActive(false);
        button.SetActive(true);
    }

    public void HostServer()
    {
        NetworkManager.Singleton.StartHost();
        DeactivatePanel();
    }
    public void JoinServer()
    {
        NetworkManager.Singleton.StartClient();
        DeactivatePanel();
    }
    public void CloseServer()
    {
        NetworkManager.Singleton.Shutdown();
        DeactivatePanel();
    }
}
