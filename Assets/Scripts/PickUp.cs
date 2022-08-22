using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// For replenishing Health and Ammo.

public class PickUp : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<int> health;
    [SerializeField] public NetworkVariable<int> ammo;

    [SerializeField] public NetworkVariable<bool> usedPickup;
    [SerializeField] public float pickupTimer;
    [SerializeField] public float pickupTimerMax;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        usedPickup.OnValueChanged += OnUsedPickupChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        usedPickup.OnValueChanged -= OnUsedPickupChanged;
    }

    public void OnUsedPickupChanged(bool previous, bool current)
    {
        // Just used.
        if (!previous && current)
        {
            this.gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }

        // Just refreshed.
        if (previous && !current)
        {
            this.gameObject.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void Update()
    {
        if (usedPickup.Value)
        {
            if (pickupTimer > 0.0f)
            {
                pickupTimer -= Time.deltaTime;
            }
            else
            {
                ResetPickUpServerRpc(this.gameObject.GetComponent<NetworkObject>());
                pickupTimer = pickupTimerMax;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void ResetPickUpServerRpc(NetworkObjectReference pickup)
    {
        if (pickup.TryGet(out NetworkObject pickupObject))
        {
            // Reset Pickup
            pickupObject.gameObject.GetComponent<PickUp>().usedPickup.Value = false;
        }
    }
}
