using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Old network UI.
public class NetworkUI : MonoBehaviour
{
    public void Host()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
