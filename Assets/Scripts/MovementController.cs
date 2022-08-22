using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MovementController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> hardEnabled;
    [SerializeField] public NetworkVariable<float> speed;

    [SerializeField] private Vector2 moveVector;

    void Update()
    {
        if (IsLocalPlayer && hardEnabled.Value)
        {
            
            moveVector = new Vector2(Input.GetAxis("Horizontal"), 0.0f);

            moveVector = Vector2.ClampMagnitude(moveVector, 1.0f) * speed.Value * Time.deltaTime;

            MoveServerRpc(moveVector, NetworkManager.Singleton.LocalClientId);
        }
    }

    // Executes on server.
    [ServerRpc (RequireOwnership = false)]
    void MoveServerRpc(Vector2 moveVector, ulong clientID)
    {
        NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.transform.Translate(moveVector);

        if (moveVector.x == 0.0f)
        {
            NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<Animator>().SetBool("movingBool", false);
        }
        else
        {
            NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<Animator>().SetBool("movingBool", true);
        }
    }

    // Special movement.
    public void OnTriggerEnter2D(Collider2D col)
    {
        if (IsLocalPlayer)
        {
            if (col.gameObject.GetComponent<JumpPad>() != null)
            {
                Vector2 jumpForce = col.gameObject.transform.right * col.gameObject.GetComponent<JumpPad>().boostVelocity;
                JumpPadServerRpc(NetworkManager.Singleton.LocalClientId, jumpForce);

                // Play jump pad SFX.
                AudioManager.instance.PlaySFX(10, 12);
            }
            
            if (col.gameObject.GetComponent<Portal>() != null)
            {
                Vector2 displaceVector = col.gameObject.GetComponent<Portal>().partner.gameObject.transform.right * col.gameObject.GetComponent<Portal>().partner.GetComponent<Portal>().teleDistance;
                PortalServerRpc(NetworkManager.Singleton.LocalClientId, col.gameObject.GetComponent<Portal>().partner.gameObject.transform.position, displaceVector);

                // Play portal SFX.
                AudioManager.instance.PlaySFX(13);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void JumpPadServerRpc(ulong jumpReceiver, Vector2 jumpForce)
    {
        // Apply jump.
        NetworkManager.Singleton.ConnectedClients[jumpReceiver].PlayerObject.gameObject.GetComponent<Rigidbody2D>().AddForce(jumpForce);
        print(jumpForce.ToString());
    }

    [ServerRpc (RequireOwnership = false)]
    public void PortalServerRpc(ulong portalReceiver, Vector2 partnerPortalPosition, Vector2 displaceFromPortal)//, Vector2 partnerPortalRotation)
    {
        // Apply teleport.
        NetworkManager.Singleton.ConnectedClients[portalReceiver].PlayerObject.transform.position = partnerPortalPosition;
        //NetworkManager.Singleton.ConnectedClients[portalReceiver].PlayerObject.transform.rotation = partnerPortalRotation;

        NetworkManager.Singleton.ConnectedClients[portalReceiver].PlayerObject.transform.Translate(displaceFromPortal);
        print("Used portal.");
    }
}
