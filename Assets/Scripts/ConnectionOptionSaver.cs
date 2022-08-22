using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remembers if the player wanted to host or join a game. Used after loading a level.

public class ConnectionOptionSaver : MonoBehaviour
{
    [SerializeField] public bool selectedHost;
    [SerializeField] public string playerName;
    [SerializeField] public string roomCode;
    [SerializeField] public string selectedGamemode;
    [SerializeField] public Color playerColor;

    private void Awake() 
    { 
        DontDestroyOnLoad(this.gameObject);
    }
}
