using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Transports.UNET;

public class GUINetwork : MonoBehaviour
{

    public GameData CurrentGame;
    public UNetTransport UNetwork;

    private string hostIp;
    private string hostPort;
    private bool connectAsHost = false;
    private bool connectAsClient = false;

    private void Start()
    {
        hostIp = UNetwork.ConnectAddress;
        hostPort = UNetwork.ConnectPort.ToString();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (!connectAsClient && !connectAsHost)
                StartButtons();
            else
                ConnectUI();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) connectAsHost = true;
        if (GUILayout.Button("Client")) connectAsClient = true;
    }

    void ConnectUI()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;

        GUILayout.Label("Host IP", style);
        hostIp = GUILayout.TextField(hostIp);
        GUILayout.Label("Host Port", style);
        hostPort = GUILayout.TextField(hostPort);
        UNetwork.ConnectAddress = hostIp;
        UNetwork.ConnectPort = int.Parse(hostPort);

        if (connectAsClient)
            if (GUILayout.Button("Connect")) NetworkManager.Singleton.StartClient();
        if(connectAsHost)
            if (GUILayout.Button("Start Hosting")) NetworkManager.Singleton.StartHost();

        if (GUILayout.Button("Cancel")) 
        {
            connectAsClient = false;
            connectAsHost = false;
        };
    }

    void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name, style);
        GUILayout.Label("Mode: " + mode, style);

        if (NetworkManager.Singleton.IsHost && !CurrentGame.GameStarted.Value)
            if (GUILayout.Button("Start Game")) CurrentGame.StartGameServerRpc();
    }
}
