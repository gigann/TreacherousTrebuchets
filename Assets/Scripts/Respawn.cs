using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//
// BUG: client wont die and wont run out of ammo.
//

public class Respawn : NetworkBehaviour
{
    [SerializeField] public float respawnTimer;
    [SerializeField] public float respawnTimerMax;

    public void Update()
    {
        if (IsLocalPlayer)
        {
            // Respawn.
            if (this.gameObject.GetComponent<Health>().currentHealth.Value == 0 && respawnTimer >= 0.0f)
            {
                respawnTimer -= Time.deltaTime;
            }

            if (respawnTimer < 0.0f)
            {
                respawnTimer = respawnTimerMax;
                RespawnServerRpc(NetworkManager.Singleton.LocalClientId, Vector2.zero);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void RespawnServerRpc(ulong respawningPlayer, Vector2 spawnPoint)
    {
        // Stop death animation.
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Animator>().SetBool("diedBool", false);

        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.transform.position = spawnPoint;

        // Enable some components.
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Health>().hardEnabled.Value = true;
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<MovementController>().hardEnabled.Value = true;
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<LookController>().hardEnabled.Value = true;
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Shoot>().hardEnabled.Value = true;

        // Max out currentHealth and currentAmmo.
        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value = NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Health>().maxHealth.Value;

        NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value = NetworkManager.Singleton.ConnectedClients[respawningPlayer].PlayerObject.gameObject.GetComponent<Shoot>().maxAmmo.Value;
            
        // Debug log.
        print("Player " + respawningPlayer.ToString()  + " respawned.");
    }

}