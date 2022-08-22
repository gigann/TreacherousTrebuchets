using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using  UnityEngine.UI;

// Client side only.
public class PlayerHUD : NetworkBehaviour
{
    [SerializeField] public GameObject healthBar;
    [SerializeField] public GameObject ammoText;
    [SerializeField] public GameObject kdText;
    [SerializeField] public GameObject timerText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Enable the player hud for only the local player.
        if (IsLocalPlayer)
        {
            healthBar = GameObject.FindWithTag("HealthBar");
            if (healthBar != null)
            {
                UpdateHealthBar();
            }
            
            ammoText = GameObject.FindWithTag("AmmoText");
            if (ammoText != null)
            {
                UpdateAmmoText();
            }

            kdText = GameObject.FindWithTag("KDText");
            if (kdText != null)
            {
                UpdateKDText();
            }

            timerText = GameObject.FindWithTag("TimerText");
        }
    }

    public void Update()
    {
        UpdateTimerText();
    }

    public void UpdateHealthBar()
    {
        
        if (IsLocalPlayer && healthBar != null)
        {
            healthBar.GetComponent<Slider>().value = this.gameObject.GetComponent<Health>().currentHealth.Value;

            healthBar.GetComponent<Slider>().transform.Find("Health Text").GetComponent<TextMeshProUGUI>().text = "HP " + this.gameObject.GetComponent<Health>().currentHealth.Value.ToString() + "/" + this.gameObject.GetComponent<Health>().maxHealth.Value.ToString();

            if (this.gameObject.GetComponent<Health>().currentHealth.Value <= 0.0f)
            {
                healthBar.GetComponent<Slider>().value = 100.0f;
                healthBar.GetComponent<Slider>().transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
                healthBar.GetComponent<Slider>().transform.Find("Health Text").GetComponent<TextMeshProUGUI>().text = "d e a d";
            }
            else
            {
                healthBar.GetComponent<Slider>().transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = new Color(0.97f, 0.0f, 0.0f, 1.0f);
            }
        }
    }

    public void UpdateAmmoText()
    {
        if (IsLocalPlayer && ammoText != null)
        {
            ammoText.GetComponent<TextMeshProUGUI>().text = this.gameObject.GetComponent<Shoot>().currentAmmo.Value.ToString() + "/" + this.gameObject.GetComponent<Shoot>().maxAmmo.Value.ToString() + " stones";
        }
    }

    public void UpdateKDText()
    {
        if (IsLocalPlayer && kdText != null)
        {
            kdText.GetComponent<TextMeshProUGUI>().text = this.gameObject.GetComponent<PlayerStats>().playerKills.Value.ToString() + " kills / " + this.gameObject.GetComponent<PlayerStats>().playerDeaths.Value.ToString() + " deaths";
        }
    }

    public void UpdateTimerText()
    {
        if (IsLocalPlayer && timerText != null)
        {
            timerText.GetComponent<TextMeshProUGUI>().text = TimeSpan.FromSeconds(Math.Round(GameObject.FindWithTag("GameManager").GetComponent<GameManager>().timerTime.Value)).ToString();
            //print(NetworkManager.Singleton.GetComponent<GameManager>().timerTime.ToString());
        }
    }
}