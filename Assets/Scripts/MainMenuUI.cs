using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using  UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    // Initial variables.
    [SerializeField] public string playerName;
    [SerializeField] public string roomCode;
    [SerializeField] public string selectedLevel;
    [SerializeField] public string selectedGamemode;
    [SerializeField] public Color playerColor;
    [SerializeField] public GameObject connectionOptionSaver;

////////////////////////////////////////////////////////////////
    // Main Menu.
    [SerializeField] public GameObject NetSetPanel;

    public void ShowNetSetPanel()
    {
        AudioManager.instance.PlaySFX(0);
        NetSetPanel.SetActive(true);
        QuitPanel.SetActive(false);
        HostSetPanel.SetActive(false);
        ClientSetPanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

////////////////////////////////////////////////////////////////
    
    // Host.
    [SerializeField] public GameObject HostSetPanel;
    [SerializeField] public GameObject HostRoomCodeIF;
    [SerializeField] public GameObject HostNameIF;
    [SerializeField] public GameObject HostRSlider;
    [SerializeField] public GameObject HostGSlider;
    [SerializeField] public GameObject HostBSlider;
    [SerializeField] public GameObject HostDummy;
    [SerializeField] public GameObject HostModeDropdown;
    [SerializeField] public GameObject HostLevelDropdown;

    public void ShowHostSetPanel()
    {
        AudioManager.instance.PlaySFX(0);
        NetSetPanel.SetActive(false);
        QuitPanel.SetActive(false);
        HostSetPanel.SetActive(true);
        ClientSetPanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

    public void HostLocal()
    {
        AudioManager.instance.PlaySFX(0);

        // Get player name and color.
        playerName = HostNameIF.GetComponent<TMP_InputField>().text;
        playerColor = new Color(HostRSlider.GetComponent<Slider>().value, HostGSlider.GetComponent<Slider>().value, HostBSlider.GetComponent<Slider>().value, 1.0f);

        // Get room code.
        roomCode = HostRoomCodeIF.GetComponent<TMP_InputField>().text;

        // Get gamemode.
        selectedGamemode = HostModeDropdown.GetComponent<TMP_Dropdown>().captionText.text;
        
        // Get level.
        selectedLevel = HostLevelDropdown.GetComponent<TMP_Dropdown>().captionText.text;

        // Save host selection.
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().selectedHost = true;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().selectedGamemode = selectedGamemode;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().roomCode = roomCode;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().playerName = playerName;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().playerColor = playerColor;

        // Load level.
        SceneManager.LoadSceneAsync(selectedLevel);
    }

    public void HostExternal()
    {
        AudioManager.instance.PlaySFX(0);
        print("unimplemented");
    }

    // Set the player color.
    public void OnHostColorChanged()
    {
        AudioManager.instance.PlaySFX(0);
        playerColor = new Color(HostRSlider.GetComponent<Slider>().value, HostGSlider.GetComponent<Slider>().value, HostBSlider.GetComponent<Slider>().value, 1.0f);

        foreach (Transform child in HostDummy.transform)
        {
            if (child.gameObject.GetComponent<Renderer>() != null)
            {
                child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", playerColor);
            }
        }
    }


////////////////////////////////////////////////////////////////
    
    // Client.
    [SerializeField] public GameObject ClientSetPanel;
    [SerializeField] public GameObject ClientRoomCodeIF;
    [SerializeField] public GameObject ClientIPAddressIF;
    [SerializeField] public GameObject ClientNameIF;
    [SerializeField] public GameObject ClientRSlider;
    [SerializeField] public GameObject ClientGSlider;
    [SerializeField] public GameObject ClientBSlider;
    [SerializeField] public GameObject ClientDummy;

    public void ShowClientSetPanel()
    {
        AudioManager.instance.PlaySFX(0);
        NetSetPanel.SetActive(false);
        QuitPanel.SetActive(false);
        HostSetPanel.SetActive(false);
        ClientSetPanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }

    public void Join()
    {
        AudioManager.instance.PlaySFX(0);

        // Get player name and color.
        playerName = ClientNameIF.GetComponent<TMP_InputField>().text;
        playerColor = new Color(ClientRSlider.GetComponent<Slider>().value, ClientGSlider.GetComponent<Slider>().value, ClientBSlider.GetComponent<Slider>().value, 1.0f);

        // Get room code.
        roomCode = ClientRoomCodeIF.GetComponent<TMP_InputField>().text;

        // Save host selection.
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().selectedHost = false;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().playerName = playerName;
        connectionOptionSaver.GetComponent<ConnectionOptionSaver>().playerColor = playerColor;

        // Attempt to connect.
        NetworkManager.Singleton.GetComponent<ConnectionManager>().StartClient(roomCode);
    }

    // Set the player color.
    public void OnClientColorChanged()
    {
        AudioManager.instance.PlaySFX(0);
        playerColor = new Color(ClientRSlider.GetComponent<Slider>().value, ClientGSlider.GetComponent<Slider>().value, ClientBSlider.GetComponent<Slider>().value, 1.0f);

        foreach (Transform child in ClientDummy.transform)
        {
            if (child.gameObject.GetComponent<Renderer>() != null)
            {
                child.gameObject.GetComponent<Renderer>().material.SetColor("_Color", playerColor);
            }
        }
    }


////////////////////////////////////////////////////////////////
    
    // SETTINGS
    [SerializeField] public GameObject SettingsPanel;
    [SerializeField] public GameObject SFXSlider;
    [SerializeField] public GameObject BGMSlider;
    [SerializeField] public GameObject MasterVolumeSlider;

    public void ShowSettingsPanel()
    {
        AudioManager.instance.PlaySFX(0);
        NetSetPanel.SetActive(false);
        QuitPanel.SetActive(false);
        HostSetPanel.SetActive(false);
        ClientSetPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }

    // Set audio manager singleton settings.
    public void OnSFXChanged()
    {
        AudioManager.instance.AdjustSFXVolume(SFXSlider.GetComponent<Slider>().value * 10.0f);

        AudioManager.instance.PlaySFX(0);
    }

    public void OnBGMChanged()
    {
        AudioManager.instance.PlaySFX(0);
        AudioManager.instance.AdjustBGMVolume(BGMSlider.GetComponent<Slider>().value * 10.0f);
    }

    public void OnMasterVolumeChanged()
    {
        AudioManager.instance.PlaySFX(0);
        AudioManager.instance.AdjustMasterVolume(MasterVolumeSlider.GetComponent<Slider>().value * 10.0f);
    }

////////////////////////////////////////////////////////////////
    // QUIT
    [SerializeField] public GameObject QuitPanel;

    public void ShowQuitPanel()
    {
        AudioManager.instance.PlaySFX(0);
        NetSetPanel.SetActive(false);
        QuitPanel.SetActive(true);
        HostSetPanel.SetActive(false);
        ClientSetPanel.SetActive(false);
        SettingsPanel.SetActive(false);
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }
}
