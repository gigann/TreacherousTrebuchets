using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Controls where the player is looking.
public class LookController : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> hardEnabled;

    [SerializeField] public bool flipped;
    [SerializeField] public Vector3 mouseWorldLoc;
    [SerializeField] public Transform aim;

    [SerializeField] public Vector3 facingRight;
    [SerializeField] public Vector3 facingLeft;

    void Update()
    {
        if (IsLocalPlayer && hardEnabled.Value)
        {
            mouseWorldLoc = this.gameObject.GetComponent<CameraController>().playerCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));

            // Setting the target.
            if (!flipped)
            {
                aim.right = mouseWorldLoc - aim.position;
            }
            else
            {
                aim.right = -mouseWorldLoc + aim.position;
            }

            // debug
            //print(target.rotation.ToString());

            if (flipped && GetLookAtX() > aim.position.x)
            {
                flipped = false;
                FlipServerRpc(flipped, facingRight, NetworkManager.Singleton.LocalClientId);

                this.gameObject.GetComponent<Shoot>().projectileSpeed *= -1.0f;
                
            }
            else if (!flipped && GetLookAtX()< aim.position.x)
            {
                flipped = true;
                FlipServerRpc(flipped, facingLeft, NetworkManager.Singleton.LocalClientId);

                this.gameObject.GetComponent<Shoot>().projectileSpeed *= -1.0f;
            }
        }
    }

    // Flips the player. Executes on server.
    [ServerRpc (RequireOwnership = false)]
    void FlipServerRpc(bool flipped, Vector3 reflection, ulong clientID)
    {
        // Vector3 originalScale = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.transform.localScale;

        NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.transform.localScale = reflection;

        // Set sprite renderer to originalScale, but flip it. This fixes a weird graphical glitch with interpolation that stretches out sprites / makes them flip like paper mario.
        // NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<SpriteRenderer>().transform.localScale = originalScale;

        // NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<SpriteRenderer>().flipX = flipped;

        // Set flipped.
        NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<LookController>().flipped = flipped;
    }

    // Returns the XY coordinates the player is currently looking at with their mouse.
    public Vector2 GetLookAt()
    {
        return mouseWorldLoc;
    }

    public float GetLookAtX()
    {
        return mouseWorldLoc.x;
    }

    public float GetLookAtY()
    {
        return mouseWorldLoc.y;
    }
}
