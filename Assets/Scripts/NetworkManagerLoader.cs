using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

// Handles scene loading.
public class NetworkManagerLoader : NetworkBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // This is necessary for maintaining the NetworkManager singleton between scene changes.
        if (SceneManager.GetActiveScene().name == "NetworkManagerLoader")
        {
            SceneManager.LoadScene(1);
        }
    }
}