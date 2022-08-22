using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

// GameManager for managing game state.

public class GameManager : NetworkBehaviour
{
    [SerializeField] public NetworkVariable<bool> respawnEnabled;
    [SerializeField] public NetworkVariable<bool> timerEnabled;
    [SerializeField] public NetworkVariable<bool> killLimitEnabled;

    [SerializeField] public NetworkVariable<float> timerTime; // Time limit of a game, in seconds.

    [SerializeField] public NetworkVariable<int> winningKills; // # of kills to win.

    [SerializeField] public NetworkVariable<int> gameMode;
    [SerializeField] public NetworkVariable<bool> gameOver; // Triggered under certain win/lose conditions.

    [SerializeField] public bool countingDown;

    public void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameOver.OnValueChanged += OnGameOverChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        gameOver.OnValueChanged -= OnGameOverChanged;
    }

    public void OnGameOverChanged(bool previous, bool current)
    {
        // The game just ended.
        if (!previous && current)
        {
            print("game over");
            // Pause the game.
            //EndGameServerRpc();
        }
    }

    [ServerRpc]
    public void SetBattleRoyaleServerRpc()
    {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameMode.Value = 0;

        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().respawnEnabled.Value = false;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().timerEnabled.Value = false;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().killLimitEnabled.Value = false;

        // Enable/disable timer.
        GameObject.FindWithTag("TimerText").SetActive(false);
    }

    [ServerRpc]
    public void SetTimedDeathMatchServerRpc()
    {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameMode.Value = 1;

        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().respawnEnabled.Value = true;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().timerEnabled.Value = true;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().killLimitEnabled.Value = false;

        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().countingDown = true;

        // Enable/disable timer.
        GameObject.FindWithTag("TimerText").SetActive(true);
    }

    [ServerRpc]
    public void SetKillLimitDeathMatchServerRpc()
    {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameMode.Value = 2;

        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().respawnEnabled.Value = true;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().timerEnabled.Value = false;
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().killLimitEnabled.Value= true;

        // Enable/disable timer.
        GameObject.FindWithTag("TimerText").SetActive(false);
    }

    public void Update()
    {
        if (countingDown && timerTime.Value > 0.0f)
        {
            CountdownTimerServerRpc();
        }
        // Game timer went off and timed death match.
        else if (timerTime.Value <= 0.0f && gameMode.Value == 1)
        {
            GetTimedDeathMatchWinnerServerRpc();
        }
    }

    [ServerRpc]
    public void CountdownTimerServerRpc()
    {
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().timerTime.Value -= Time.deltaTime;
    }

    [ServerRpc]
    public void GetBattleRoyaleWinnerServerRpc(ulong potentialWinnerID)
    {
        // If # of alive players == 1, then this player won.
        int alivePlayers = 0;

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<Health>().currentHealth.Value > 0.0f)
            {
                alivePlayers ++;
            }
            else
            {
                NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = -1;
            }
        }

        if (alivePlayers <= 1)
        {
            NetworkManager.Singleton.ConnectedClients[potentialWinnerID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = 1;
        }
    }

    [ServerRpc]
    public void GetTimedDeathMatchWinnerServerRpc()
    {
        ulong currentWinnerID = 0;
        int highestScore = 0;

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (highestScore < NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<PlayerStats>().playerKills.Value)
            {
                highestScore = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<PlayerStats>().playerKills.Value;

                currentWinnerID = clientID;
            }
            else
            {
                NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = -1;
            }
        }

        NetworkManager.Singleton.ConnectedClients[currentWinnerID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = 1;
    }
    
    [ServerRpc]
    public void GetKillLimitDeathMatchWinnerServerRpc(ulong winnerID)
    {
        // We know this player just won.
        GameObject.FindWithTag("GameManager").GetComponent<GameManager>().gameOver.Value = true;

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (winnerID != clientID)
            {
                NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = -1;
            }
        }

        NetworkManager.Singleton.ConnectedClients[winnerID].PlayerObject.gameObject.GetComponent<PlayerStats>().wonOrLost.Value = 1;
    }
}




