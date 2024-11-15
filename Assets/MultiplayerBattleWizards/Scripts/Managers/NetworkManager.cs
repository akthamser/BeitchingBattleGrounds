using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages connecting to master server and joining/creating rooms.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    [Header("Data")]
    public int roomCodeLength = 5;      // Length of the generated room codes.
    public int maxPlayers = 4;          // Maximum number of players allowed in a game.
    private int playersInGame;          // Number of players in the Game scene.

    [Header("Components")]
    public PhotonView photonView;

    // Instance
    public static NetworkManager inst;

    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != null && inst != this)
        {
            gameObject.SetActive(false);
            //Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        // Dont destroy on load.
        DontDestroyOnLoad(gameObject);

        #endregion
    }

    void Start ()
    {
        // Connect to master server.
        PhotonNetwork.ConnectUsingSettings("1.0");
    }

    // Called when the player connects to the master server.
    public void OnConnectedToMaster ()
    {
        // If we don't have a saved player name, generate a random one.
        if(!PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.playerName = GetDefaultPlayerName();
            PlayerPrefs.SetString("PlayerName", PhotonNetwork.playerName);
        }
        // Otherwise set our nickname to the saved one.
        else
        {
            PhotonNetwork.playerName = PlayerPrefs.GetString("PlayerName");
        }
    }

    #region Create Room

    // Called when the player wants to create a game.
    public void CreateRoom ()
    {
        // Set max players.
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayers;

        // Attempt to create the room.
        PhotonNetwork.CreateRoom(GetRoomCode(), roomOptions, null);
    }

    // Called when the player creates a room.
    public void OnCreatedRoom ()
    {
        Debug.Log("Room created with the code of '" + PhotonNetwork.room.Name + "'");
    }

    #endregion

    #region Join Room

    // Called when the player wants to join a specific game.
    public void AttemptJoinRoom (string requestedRoomCode)
    {
        PhotonNetwork.JoinRoom(requestedRoomCode);
    }

    // Called when the player joins a room.
    public void OnJoinedRoom ()
    {
        
    }

    #endregion

    #region Scene Loading

    // Called when the host starts the game in the lobby.
    // Loads the Game scene.
    [PunRPC]
    public void HostStartGame ()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    // Called when the game is finished.
    // Loads the Menu scene.
    [PunRPC]
    public void HostBackToMenu ()
    {
        playersInGame = 0;
        PhotonNetwork.LoadLevel("Menu");
    }

    #endregion

    // Called by a client when they join the Game scene.
    [PunRPC]
    public void ImInGame ()
    {
        playersInGame++;

        // Are we the host?
        if(PhotonNetwork.isMasterClient)
        {
            // Are all the players in game?
            if(playersInGame == PhotonNetwork.playerList.Length)
            {
                // Then initiate the players.
                PlayerManager.inst.photonView.RPC("InitiatePlayers", PhotonTargets.All);
            }
        }
    }

    // Returns a random room code.
    string GetRoomCode ()
    {
        string roomCode = "";
        string usableChars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        // Get a random character for the length of the code.
        for(int x = 0; x < roomCodeLength; ++x)
        {
            roomCode += usableChars[Random.Range(0, usableChars.Length)];
        }

        return roomCode;
    }

    // Returns a random player name.
    string GetDefaultPlayerName ()
    {
        return "Player" + Random.Range(0, 9999).ToString();
    }

    // Returns a photon player with the requested id.
    public PhotonPlayer GetPlayerOfID (int id)
    {
        return PhotonNetwork.playerList.ToList().Find(x => x.ID == id);
    }

    // Returns the PhotonNetwork.playerList sorted by ID.
    public PhotonPlayer[] GetOrderedPlayerList ()
    {
        List<PhotonPlayer> players = PhotonNetwork.playerList.ToList();
        players.Sort((a, b) => a.ID.CompareTo(b.ID));

        return players.ToArray();
    }
}