using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime; // For RoomInfo

/// <summary>
/// Manages navigation between screens and button presses.
/// </summary>
public class MenuUI : MonoBehaviour
{
    [Header("Screens")]
    public GameObject startScreenObj;
    public GameObject lobbyScreenObj;
    public GameObject connectScreenObj;
    public GameObject settingsScreenObj;
    public GameObject gameSettingsScreenObj;

    [Header("Buttons")]
    public Button createGameButton;
    public Button joinGameButton;
    public Button settingsButton;
    public Button quitButton;
    public Button lobbyButton;

    [Header("Room List UI")]
    public RectTransform roomListParent; // Parent object for room buttons
    public GameObject roomButtonPrefab; // Prefab for room button

    [Header("Screen Components")]
    public LobbyScreen lobbyScreen;
    public ConnectScreen connectScreen;
    public SettingsScreen settingsScreen;
    public GameSettingsScreen gameSettingsScreen;

    [Header("Components")]
    public Animator screenAnimator;
    public PhotonView photonView;

    public enum MenuScreen
    {
        Start,
        Lobby,
        Connect,
        Settings,
        GameSettings
    }

    void Start()
    {
        createGameButton.interactable = false;
        joinGameButton.interactable = false;
        settingsButton.interactable = false;

        if (PhotonNetwork.inRoom)
            EndGameReturnToLobby();
        else
        {
            SetScreen(MenuScreen.Start);
        }
    }

    #region Photon Events

    public void OnConnectedToMaster()
    {
        createGameButton.interactable = true;
        joinGameButton.interactable = true;
        settingsButton.interactable = true;

        // Refresh the list of rooms whenever connected to the master
        RefreshRoomList();
    }

    #endregion

    // Sets the currently visible screen.
    public void SetScreen(MenuScreen screen)
    {
        startScreenObj.SetActive(false);
        lobbyScreenObj.SetActive(false);
        connectScreenObj.SetActive(false);
        settingsScreenObj.SetActive(false);
        gameSettingsScreenObj.SetActive(false);

        switch (screen)
        {
            case MenuScreen.Start:
                startScreenObj.SetActive(true);
                break;
            case MenuScreen.Lobby:
                lobbyScreenObj.SetActive(true);
                lobbyScreen.OnSetScreen();
                break;
            case MenuScreen.Connect:
                connectScreenObj.SetActive(true);
                connectScreen.OnSetScreen();
                PopulateRoomList();
                break;
            case MenuScreen.Settings:
                settingsScreenObj.SetActive(true);
                settingsScreen.OnSetScreen();
                break;
            case MenuScreen.GameSettings:
                gameSettingsScreenObj.SetActive(true);
                gameSettingsScreen.OnSetScreen();
                break;
        }

        screenAnimator.Rebind();
    }

    public void PopulateRoomList()
    {
        // Clear existing buttons
        foreach (Transform child in roomListParent)
        {
            Destroy(child.gameObject);
        }

        // Get a list of rooms from Photon and create buttons for them
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();
        foreach (RoomInfo room in rooms)
        {
            GameObject buttonObj = Instantiate(roomButtonPrefab, roomListParent);
            Button roomButton = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.GetComponentInChildren<Text>();

            buttonText.text = "Room: " + room.Name;
            roomButton.onClick.AddListener(() => NetworkManager.inst.AttemptJoinRoom(room.Name));
        }
    }

    public void OnCreateGameButton()
    {
        NetworkManager.inst.CreateRoom();
    }

    public void OnJoinGameButton()
    {
        SetScreen(MenuScreen.Connect);
    }

    public void OnSettingsButton()
    {
        SetScreen(MenuScreen.Settings);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    public void OnLobbyButton()
    {
        SetScreen(MenuScreen.Lobby);
    }

    public void EnableLobbyButton()
    {
        lobbyButton.gameObject.SetActive(true);
        createGameButton.gameObject.SetActive(false);
        joinGameButton.interactable = false;
    }

    public void DisableLobbyButton()
    {
        lobbyButton.gameObject.SetActive(false);
        createGameButton.gameObject.SetActive(true);
        joinGameButton.interactable = true;
    }

    void EndGameReturnToLobby()
    {
        EnableLobbyButton();
        SetScreen(MenuScreen.Lobby);
    }

    // Refresh room list manually
    public void RefreshRoomList()
    {
        PopulateRoomList();
    }
}
