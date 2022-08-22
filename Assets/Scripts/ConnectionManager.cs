using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
// using TMPro;
using UnityEngine.SceneManagement;
using System.Text;

// This script has functions for managing network connections.

/*
Idea

add notificaiton for when player joins

then host will spawn them in under their ID >:)

*/

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] public GameObject playerPrefab;

    // The room code of the actual host.
    public string canonicalCode;

    public void StartClient(string clientCode)
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(clientCode);

        // Try to join as a client.
        NetworkManager.Singleton.StartClient();
    }

    public void StartLANHost(string hostCode)//, string selectedLevel, string selectedGamemode, string playerName, Color playerColor)
    {
        canonicalCode = hostCode;
        //NetworkManager.Singleton.ConnectAddress = LOCAL_IP

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        
        // Start hosting locally.
        NetworkManager.Singleton.StartHost();

        // Spawn player

    }

    public void StartWANHost()
    {
        //TBD
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(1);
        Application.Quit();
    }


    // Approves network connections.
    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        print("Checking connection...");
        
        bool validConnection = (Encoding.ASCII.GetString(connectionData) == canonicalCode);

        // Call back with a boolean validConnection. True lets a client join. Hosts always join.
        callback(true, null, validConnection, null, null);
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetStatsServerRpc(ulong playerID, string playerName, Color playerColor)
    {
        NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject.GetComponent<PlayerStats>().playerName = playerName;

        NetworkManager.Singleton.ConnectedClients[playerID].PlayerObject.gameObject.GetComponent<PlayerStats>().playerColor = playerColor;
    }

    // [ServerRpc]
    // public void PlayerSpawnServerRpc(ulong playerID, string playerName, Color playerColor)
    // {
    //     // Instantiate player.
    //     GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        
    //     player.GetComponent<PlayerStats>().playerName = playerName;
    //     player.GetComponent<PlayerStats>().playerColor = playerColor;

    //     // Spawn with player as controller.
    //     player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerID);
    // }

    // [ServerRpc]
    // public void PlayerSpawnForAllServerRpc(ulong playerID, string playerName, Color playerColor)
    // {
    //     PlayerSpawnClientRpc(playerID, playerName, playerColor);
    // }

    // [ClientRpc]
    // public void PlayerSpawnClientRpc(ulong playerID, string playerName, Color playerColor)
    // {
    //     // Instantiate player.
    //     GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        
    //     player.GetComponent<PlayerStats>().playerName = playerName;
    //     player.GetComponent<PlayerStats>().playerColor = playerColor;

    //     // Spawn with player as controller.
    //     player.GetComponent<NetworkObject>().SpawnAsPlayerObject(playerID);
    // }
}
