using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LobbyScreen : MonoBehaviour
{
    public Text roomCode;                       // Text which displays the room code.
    public LobbyScreenPlayerUI[] playerUI;      // List of player lobby UI data.
    public Button startGameButton;              // Button to start the game (only visible for host).
    public Button gameSettingsButton;           // Button to change game settings (only visible for host).
    public Text gameSettingsText;               // Text showing game mode and map.

    private string[] mapNames;                  // Array of all map names.

    private MenuUI menu;

    void Awake ()
    {
        menu = GetComponent<MenuUI>();
    }

    // Called when the screen is activated.
    public void OnSetScreen ()
    {
        UpdateUI();
    }

    // Called when the player joins the rooms.
    // Update the player UI.
    public void OnJoinedRoom ()
    {
        // Tell the menu to enable the "Lobby" button.
        menu.EnableLobbyButton();

        // If we're the host, set the default game settings.
        if(PhotonNetwork.isMasterClient)
        {
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("map", Resources.LoadAll<TextAsset>("Maps")[0].name);
            hash.Add("gamemode", (int)GameModeType.ScoreBased);
            hash.Add("gamemodeprop", 5);
            PhotonNetwork.room.SetCustomProperties(hash);
        }

        UpdateUI();
    }

    // Checks if the local player is the master client.
    // If so, enable the "Start Game" and "Game Settings" button.
    void CheckIfMasterClient ()
    {
        // Only show the "Start Game" and "Game Settings" button if the player is the host.
        startGameButton.gameObject.SetActive(PhotonNetwork.isMasterClient ? true : false);
        gameSettingsButton.gameObject.SetActive(PhotonNetwork.isMasterClient ? true : false);
    }

    // Called when a new player joins the room.
    // Update the player UI.
    public void OnPhotonPlayerConnected (PhotonPlayer other)
    {
        UpdateUI();
    }

    // Called when a player leaves the room.
    // Update the player UI.
    void OnPhotonPlayerDisconnected (PhotonPlayer other)
    {
        UpdateUI();
    }

    // Updates the lobby UI to show who's connected, etc.
    [PunRPC]
    void UpdateUI ()
    {
        PhotonPlayer[] players = NetworkManager.inst.GetOrderedPlayerList();

        // Update the room code.
        roomCode.text = PhotonNetwork.room.Name;

        // Loop through all player UI elements.
        for(int x = 0; x < playerUI.Length; ++x)
        {
            // Is this a player in the room?
            if(x < players.Length)
            {
                playerUI[x].nameText.text = players[x].NickName;
                playerUI[x].playerImage.color = Color.white;

                // Is this player us?
                if(players[x].IsLocal)
                    playerUI[x].nameText.fontStyle = FontStyle.Bold;
                else
                    playerUI[x].nameText.fontStyle = FontStyle.Normal;
            }
            // Otherwise, grey them out.
            else
            {
                playerUI[x].nameText.text = "";
                playerUI[x].playerImage.color = Color.grey;
            }       
        }

        CheckIfMasterClient();
    }

    public void OnPhotonCustomRoomPropertiesChanged (ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Update the settings text if the map or gamemode changed.
        if(changedProps.ContainsKey("map") || changedProps.ContainsKey("gamemode"))
            SetGameSettingsText();
    }

    void SetGameSettingsText ()
    {
        // Get the custom properties.
        string mapName = PhotonNetwork.room.CustomProperties["map"].ToString();
        GameModeType gameMode = (GameModeType)PhotonNetwork.room.CustomProperties["gamemode"];
        int gameModeProperty = (int)PhotonNetwork.room.CustomProperties["gamemodeprop"];

        // Set the text.
        string gameModeText = "";

        switch(gameMode)
        {
            case GameModeType.ScoreBased: gameModeText = "First to " + gameModeProperty + " kills"; break;
            case GameModeType.TimeBased:
            {
                if(gameModeProperty >= 60) gameModeText = "Most kills in " + (gameModeProperty / 60) + " mins";
                else gameModeText = "Most kills in " + gameModeProperty + " secs";

                break;
            }
        }

        gameSettingsText.text = gameModeText + "\nMap: " + mapName;
    }

    // Called when the "Leave Lobby" button is pressed.
    // Disconnects from room and leaves the lobby screen.
    public void OnLeaveLobby ()
    {
        PhotonNetwork.LeaveRoom(false);
        menu.SetScreen(MenuUI.MenuScreen.Start);

        // Tell the menu to disable the "Lobby" button.
        menu.DisableLobbyButton();
    }

    // Called when the "Start Game" button is pressed by the host.
    // Sends RPC to all clients to load the Game scene.
    public void OnStartGame ()
    {
        NetworkManager.inst.photonView.RPC("HostStartGame", PhotonTargets.All);
    }

    // Called when the "Game Settings" button is pressed by the host.
    // Opens the Game Settings screen where the host can change the settings.
    public void OnGameSettings ()
    {
        menu.SetScreen(MenuUI.MenuScreen.GameSettings);
    }

    [System.Serializable]
    public class LobbyScreenPlayerUI
    {
        public Text nameText;
        public Image playerImage;
    }
}