using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{
    [SerializeField] public GameObject playerCamera;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Enable the player camera for only the local player.
        if (IsLocalPlayer)
        {
            playerCamera.SetActive(true);
        }
        else
        {
            playerCamera.SetActive(false);
        }
    }
}
