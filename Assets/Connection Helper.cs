using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using UnityEngine;

public class ConnectionHelper : MonoBehaviour
{
    private NetworkManager networkManager;
    private UnityTransport networkTransport;

    private GameObject connectionUI;
    private TMP_InputField IPInputField;
        
    void Start()
    {
        networkManager = GetComponent<NetworkManager>();
        networkTransport = GetComponent<UnityTransport>();
        connectionUI = GameObject.Find("ConnectionUI");
        IPInputField = connectionUI.GetComponentInChildren<TMP_InputField>();            

        //Adds a callback which triggers when a client has connected, used here for removing the connection screen.
        networkManager.OnClientConnectedCallback += ClientConnected;        
    }

    public void ConnectToGame()
    {
        networkTransport.ConnectionData.Address = IPInputField.text;
        Debug.Log($"Connecting to {networkTransport.ConnectionData.Address}");
        
        networkManager.StartClient();
    }

    public void HostGame()
    {
        networkTransport.ConnectionData.Address = IPInputField.text;
        Debug.Log($"Hosting Game at {networkTransport.ConnectionData.Address}");        
        networkManager.StartHost();        
    }    

    private void ClientConnected(ulong clientId)
    {
        Debug.Log($"Connected as client {clientId}");
        DestroyConnectionUI();
    }

    private void DestroyConnectionUI()
    {
        Destroy(connectionUI);
    }

}