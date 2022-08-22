using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerStats : NetworkBehaviour
{
    [SerializeField] public string playerName;
    [SerializeField] public Color playerColor;

    [SerializeField] public NetworkVariable<int> playerKills;
    [SerializeField] public NetworkVariable<int> playerDeaths;

    [SerializeField] public NetworkVariable<int> wonOrLost; // 0 for no info/don't do anything, -1 for lost, 1 for won.
    [SerializeField] public bool leavingGame;
    [SerializeField] public float leavingGameTimer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Enable/disable respawn.
        this.gameObject.GetComponent<Respawn>().enabled = GameObject.FindWithTag("GameManager").GetComponent<GameManager>().respawnEnabled.Value;

        // Hide game over text.
        GameObject.FindWithTag("GameOverText").GetComponent<TextMeshProUGUI>().text = "";

        playerKills.OnValueChanged += OnPlayerKillsChanged;
        playerDeaths.OnValueChanged += OnPlayerDeathsChanged;
        wonOrLost.OnValueChanged += OnWonOrLostChanged;

        // Set the local player to blue and all others to red.
        if (IsLocalPlayer)
        {
            // Wood color.
            this.gameObject.transform.GetChild(4).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.3f, 0.8f, 1.0f));
            this.gameObject.transform.GetChild(7).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.3f, 0.8f, 1.0f));
            this.gameObject.transform.GetChild(9).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.3f, 0.8f, 1.0f));
            this.gameObject.transform.GetChild(10).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.3f, 0.8f, 1.0f));
            this.gameObject.transform.GetChild(14).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.3f, 0.3f, 0.8f, 1.0f));

            // Bucket and barrel color.
            this.gameObject.transform.GetChild(8).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.9f, 1.0f));
            this.gameObject.transform.GetChild(13).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.9f, 1.0f));

            // Tire color.
            this.gameObject.transform.GetChild(11).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.1f, 0.1f, 0.5f, 1.0f));
            this.gameObject.transform.GetChild(12).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.1f, 0.1f, 0.5f, 1.0f));
        }
        else
        {
            // Wood color.
            this.gameObject.transform.GetChild(4).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.3f, 0.3f, 1.0f));
            this.gameObject.transform.GetChild(7).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.3f, 0.3f, 1.0f));
            this.gameObject.transform.GetChild(9).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.3f, 0.3f, 1.0f));
            this.gameObject.transform.GetChild(10).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.3f, 0.3f, 1.0f));
            this.gameObject.transform.GetChild(14).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.8f, 0.3f, 0.3f, 1.0f));

            // Bucket and barrel color.
            this.gameObject.transform.GetChild(8).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.9f, 0.2f, 0.2f, 1.0f));
            this.gameObject.transform.GetChild(13).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.9f, 0.2f, 0.2f, 1.0f));

            // Tire color.
            this.gameObject.transform.GetChild(11).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.5f, 0.1f, 0.1f, 1.0f));
            this.gameObject.transform.GetChild(12).gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.5f, 0.1f, 0.1f, 1.0f));
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        playerKills.OnValueChanged -= OnPlayerKillsChanged;
        playerDeaths.OnValueChanged -= OnPlayerDeathsChanged;
        wonOrLost.OnValueChanged -= OnWonOrLostChanged;
    }

    public void OnPlayerKillsChanged(int previous, int current)
    {
        // Update HUD.
        this.gameObject.GetComponent<PlayerHUD>().UpdateKDText();

        if (previous < current)
        {
            // Play kill sound.
            AudioManager.instance.PlaySFX(1);

            // Battle royale.
            if (GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameMode.Value == 0)
            {
                // Check if this player is the last one standing.
                GameObject.FindWithTag("GameManager").GetComponent<GameManager>().GetBattleRoyaleWinnerServerRpc(NetworkManager.Singleton.LocalClientId);
            }

            // Kill limit death match.
            if (GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameMode.Value == 2 && current >= GameObject.FindWithTag("GameManager").GetComponent<GameManager>().winningKills.Value)
            {
                // This player just won.
                GameObject.FindWithTag("GameManager").GetComponent<GameManager>().GetKillLimitDeathMatchWinnerServerRpc(NetworkManager.Singleton.LocalClientId);
            }

        }
    }
    
    public void OnPlayerDeathsChanged(int previous, int current)
    {
        // Update HUD.
        this.gameObject.GetComponent<PlayerHUD>().UpdateKDText();

        if (previous < current)
        {
            // Play death sound.
            AudioManager.instance.PlaySFX(2, 4);

        }
    }

    public void OnWonOrLostChanged(int previous, int current)
    {
        // We won :)
        if (previous < current)
        {
            // Play win music.
            AudioManager.instance.PlayBGM(3);

            // Show win text.
            GameObject.FindWithTag("GameOverText").GetComponent<TextMeshProUGUI>().text = "You Win";

            // Start a timer to return to the main menu.
            leavingGame = true;
        }

        // We lost :(
        if (current < previous)
        {
            // Play lose music.
            AudioManager.instance.PlayBGM(4);

            // Show lose text.
            GameObject.FindWithTag("GameOverText").GetComponent<TextMeshProUGUI>().text = "You Lose";

            // Start a timer to return to the main menu.
            leavingGame = true;
        }
    }

    public void Update()
    {
        if (leavingGame)
        {
            if (leavingGameTimer > 0.0f)
            {
                leavingGameTimer -= Time.deltaTime;
            }
            else
            {
                NetworkManager.Singleton.GetComponent<ConnectionManager>().Disconnect();
            }
        }
    }
}