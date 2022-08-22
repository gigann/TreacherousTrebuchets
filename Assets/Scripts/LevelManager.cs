using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

// Manages level changes.

public class LevelManager : MonoBehaviour
{
    [SerializeField] public bool gameLevel;
    [SerializeField] public int music;
    [SerializeField] public ConnectionOptionSaver hostOptions;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (music != -1)
        {
            AudioManager.instance.PlayBGM(music);
        }

        // If the scene has loaded, and this is a game level, we need to start hosting if the player is a host.
        if (gameLevel)
        {
            ConnectionOptionSaver playerOptions = GameObject.FindWithTag("ConnectionOptionSaver").GetComponent<ConnectionOptionSaver>();

            // The host who has not started hosting yet.
            if (playerOptions.selectedHost)
            {
                hostOptions = playerOptions;

                // Start hosting.
                NetworkManager.Singleton.GetComponent<ConnectionManager>().StartLANHost(hostOptions.roomCode);

                // Set game mode.
                if (hostOptions.selectedGamemode == "Battle Royale")
                {
                    GameObject.FindWithTag("GameManager").GetComponent<GameManager>().SetBattleRoyaleServerRpc();
                }
                else if (hostOptions.selectedGamemode == "First to Fifty")
                {
                   GameObject.FindWithTag("GameManager").GetComponent<GameManager>().SetKillLimitDeathMatchServerRpc();
                }
                else if (hostOptions.selectedGamemode == "10m Deathmatch")
                {
                    GameObject.FindWithTag("GameManager").GetComponent<GameManager>().SetTimedDeathMatchServerRpc();
                }

                // Adjust any player-affected game settings here, such as respawning. This is purely for the host, since other players will receive the updated values.
                NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.gameObject.GetComponent<Respawn>().enabled = GameObject.FindWithTag("GameManager").GetComponent<GameManager>().respawnEnabled.Value;

                GameObject.FindWithTag("GameOverText").GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }
}
