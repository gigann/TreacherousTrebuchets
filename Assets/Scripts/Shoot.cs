using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// When the player releases LMB, they shoot.

public class Shoot : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> hardEnabled;
    // Player damage. MIGHT MOVE THIS.
    [SerializeField] public NetworkVariable<float> baseDamage;

    [SerializeField] public GameObject stonePrefab;
    [SerializeField] public float projectileSpeed;

    // Shooting delay.
    [SerializeField] public NetworkVariable<bool> isShootingAnim;
    [SerializeField] public bool isShooting;
    [SerializeField] public bool hasShot;
    [SerializeField] public float timeShooting;

    // When the projectile leaves the bucket.
    [SerializeField] public float launchTime;
    [SerializeField] public float maxTimeShooting;

    // Inputs.
    [SerializeField] public bool pressedAim;
    [SerializeField] public bool pressedShoot;

    // Ammo.
    [SerializeField] public NetworkVariable<int> currentAmmo;
    [SerializeField] public NetworkVariable<int> maxAmmo;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isShootingAnim.OnValueChanged += OnIsShootingAnimChanged;
        currentAmmo.OnValueChanged += OnCurrentAmmoChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        isShootingAnim.OnValueChanged -= OnIsShootingAnimChanged;
        currentAmmo.OnValueChanged -= OnCurrentAmmoChanged;
    }

    public void OnIsShootingAnimChanged(bool previous, bool current)
    {
        // If shooting changed to true, play the shooting animation.
        if (!previous && current)
        {
            this.gameObject.GetComponent<Animator>().SetTrigger("shootTrigger");
        }
    }

    public void OnCurrentAmmoChanged(int previous, int current)
    {
        // Update HUD.
        this.gameObject.GetComponent<PlayerHUD>().UpdateAmmoText();

        // We got ammo!
        if (previous < current)
        {
            // Play reload sound.
            AudioManager.instance.PlaySFX(8,9);
        }
    }

    void Update()
    {
        if (IsLocalPlayer && hardEnabled.Value)
        {
            ShootProjectile();
        }
    }

    // For shooting the projectile.
    void ShootProjectile()
    {
        pressedShoot = Input.GetMouseButtonDown(0);
        
        if (!isShooting)
        {
            // Play the shooting animation but do not shoot yet.
            if (pressedShoot && currentAmmo.Value > 0)
            {
                timeShooting = maxTimeShooting;
                isShooting = true;
                SetIsShootingAnimServerRpc(NetworkManager.Singleton.LocalClientId, true);
            }
        }
        else
        {
            // Fire!
            if (!hasShot && timeShooting <= launchTime)
            {
                hasShot =  true;

                ShootServerRpc(NetworkManager.Singleton.LocalClientId, this.gameObject.GetComponent<LookController>().aim.position, this.gameObject.GetComponent<LookController>().aim.rotation, this.gameObject.GetComponent<LookController>().aim.right * projectileSpeed);

                // Play shooting SFX.
                AudioManager.instance.PlaySFX(14, 17);
            }

            if ((timeShooting > 0.0f))
            {
                timeShooting -= Time.deltaTime;
            }
            else
            { 
                hasShot = false;
                isShooting = false;
                SetIsShootingAnimServerRpc(NetworkManager.Singleton.LocalClientId, false);
            }
        }
    }

    // Executes on server.

    [ServerRpc (RequireOwnership = false)]
    void SetIsShootingAnimServerRpc(ulong shooterID, bool value)
    {
        NetworkManager.Singleton.ConnectedClients[shooterID].PlayerObject.gameObject.GetComponent<Shoot>().isShootingAnim.Value = value;
    }

    [ServerRpc (RequireOwnership = false)]
    void ShootServerRpc(ulong shooterID, Vector2 spawnPosition, Quaternion spawnRotation, Vector2 spawnForce)
    {
        // Instantiate projectile.
        GameObject projectile = Instantiate(stonePrefab, spawnPosition, spawnRotation);
        
        // Spawn with shooter as owner.
        projectile.GetComponent<NetworkObject>().SpawnWithOwnership(shooterID);

        // Add force to the projectle.

        projectile.GetComponent<Rigidbody2D>().AddForce(spawnForce);

        // Decrement ammo.
        NetworkManager.Singleton.ConnectedClients[shooterID].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value -= 1;

        // Debug logging.
        print("Player " + shooterID + " shot a projectile. Ammo: " + NetworkManager.Singleton.ConnectedClients[shooterID].PlayerObject.gameObject.GetComponent<Shoot>().currentAmmo.Value.ToString() + "/" + NetworkManager.Singleton.ConnectedClients[shooterID].PlayerObject.gameObject.GetComponent<Shoot>().maxAmmo.Value.ToString());
    }
}
