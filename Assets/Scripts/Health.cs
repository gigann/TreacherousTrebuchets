using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//
// BUG: client wont die and wont run out of ammo.
//

public class Health : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> hardEnabled;

    [SerializeField] public NetworkVariable<float> currentHealth;
    [SerializeField] public NetworkVariable<float> maxHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    public void OnHealthChanged(float previous, float current)
    {
        // Update HUD.
        this.gameObject.GetComponent<PlayerHUD>().UpdateHealthBar();

        if (previous > current)
        {
            // Play hurt animation.
            this.gameObject.GetComponent<Animator>().SetTrigger("takeDamageTrigger");

            // Play damage sound.
            AudioManager.instance.PlaySFX(19, 21);
        }

        if (previous < current)
        {
            // Play replenish health sound.
            AudioManager.instance.PlaySFX(18);
        }
    }

// https://docs.unity3d.com/ScriptReference/Collider.OnCollisionEnter.html

//https://docs-multiplayer.unity3d.com/docs/advanced-topics/physics/index.html
// Projectile MUST be trigger, otherwise clients will not detect collision.


    // Commented out because clients cannot detect collisions :(

    // public void OnCollisionEnter2D(Collision2D col)
    // {
    //     if (col.relativeVelocity.magnitude > 0.5f)
    //     {
    //         // Play collision sound.
    //         AudioManager.instance.PlaySFX(5, 7);
    //     }
    // }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (IsLocalPlayer && hardEnabled.Value)
        {
            // Someone shot me!
            if (col.gameObject.GetComponent<Projectile>() != null)
            {
                // Tell the server to apply damage to me and for the version of myself on other machines.
                // The owner of the projectile is the player who shot me.
                // Any damage modifiers should apply to the playerstats damage value.
                // This will modify its spawned projectile damage.
                
                // Play collision sound.
                AudioManager.instance.PlaySFX(5, 7);

                takeDamageServerRpc(col.gameObject.GetComponent<NetworkObject>().OwnerClientId, NetworkManager.Singleton.LocalClientId, col.gameObject.GetComponent<Projectile>().damage);
            }

            // Something harmful.
            if (col.gameObject.GetComponent<Harmful>() != null)
            {
                takeDamageServerRpc(NetworkManager.Singleton.LocalClientId, NetworkManager.Singleton.LocalClientId, col.gameObject.GetComponent<Harmful>().damage);
            }

            // Something lethal.
            if (col.gameObject.tag == "Kill")
            {
                takeDamageServerRpc(NetworkManager.Singleton.LocalClientId, NetworkManager.Singleton.LocalClientId, currentHealth.Value);
            }

            // Health! And maybe ammo!
            if (col.gameObject.GetComponent<PickUp>() != null && !col.gameObject.GetComponent<PickUp>().usedPickup.Value)
            {
                // Play pick up sound.
                AudioManager.instance.PlaySFX(0);

                if (col.gameObject.GetComponent<PickUp>().ammo.Value > 0)
                {
                    ReloadServerRpc(NetworkManager.Singleton.LocalClientId, col.gameObject.GetComponent<NetworkObject>());
                }

                if (col.gameObject.GetComponent<PickUp>().health.Value > 0)
                {
                    HealServerRpc(NetworkManager.Singleton.LocalClientId, col.gameObject.GetComponent<NetworkObject>());
                }
            }
        }
    }

    // The client requests for the server to modify the damage of the player they hit.
    [ServerRpc (RequireOwnership = false)]
    public void takeDamageServerRpc(ulong damageSender, ulong damageReceiver, float damage)
    {
        // Apply damage.
        NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value -= damage;

        // IDEA:
        // Adjust the mass of the player rigidbody according to their health, increasing the knockback other players deal to them.
        // This would be kinda like smash bros.

        // NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Rigidbody2D>().mass = 1.0f + NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value / 10.0f;

        print("Player " + damageSender.ToString()  + " hit " + damageReceiver.ToString()  + " for " + damage.ToString() + " damage");
        print("Player " + damageReceiver.ToString()  + " has " + NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value.ToString()  + "/" + NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().maxHealth.Value.ToString() + " Health.");

        // KILL.
        if (NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().currentHealth .Value<= 0.0f)
        {
            // Play death animation.
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Animator>().SetBool("diedBool", true);

            // Disable some components while dead and waiting for respawn.
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().hardEnabled.Value = false;
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<MovementController>().hardEnabled.Value = false;
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<LookController>().hardEnabled.Value = false;
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Shoot>().hardEnabled.Value = false;

            // Zero out currentHealth.
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value = 0.0f;

            // Edit player stats.
            NetworkManager.Singleton.ConnectedClients[damageSender].PlayerObject.gameObject.GetComponent<PlayerStats>().playerKills.Value += 1;
            NetworkManager.Singleton.ConnectedClients[damageReceiver].PlayerObject.gameObject.GetComponent<PlayerStats>().playerDeaths.Value += 1;

            // Debug log.
            print("Player " + damageSender.ToString()  + " killed " + damageReceiver.ToString());
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void HealServerRpc(ulong replenishedID, NetworkObjectReference pickup)
    {
        // print("Used health pickup");
        if (pickup.TryGet(out NetworkObject pickupObject))
        {
            // Use Pickup
            pickupObject.gameObject.GetComponent<PickUp>().usedPickup.Value = true;

            // Apply healing.
            NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value += (float)pickupObject.gameObject.GetComponent<PickUp>().health.Value;

            // Clamp health.
            if (NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value > NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Health>().maxHealth.Value)
            {
                NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value = NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Health>().maxHealth.Value;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void ReloadServerRpc(ulong replenishedID, NetworkObjectReference pickup)
    {
        // print("Used ammo pickup");
        if (pickup.TryGet(out NetworkObject pickupObject))
        {
            // Use Pickup
            pickupObject.gameObject.GetComponent<PickUp>().usedPickup.Value = true;

            // Apply ammo.
            NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value += pickupObject.gameObject.GetComponent<PickUp>().ammo.Value;

            // Clamp ammo.
            if (NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value > NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Shoot>().maxAmmo.Value)
            {
                NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value = NetworkManager.Singleton.ConnectedClients[replenishedID].PlayerObject.gameObject.GetComponent<Shoot>().maxAmmo.Value;
            }
        }
    }

}
