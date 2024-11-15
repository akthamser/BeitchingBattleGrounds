using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Data")]
    public NetworkPlayer[] players;                             // Array of players in the game.
    public string playerPrefabLocation;                         // Location of player prefab in the Resources folder.
    public RuntimeAnimatorController[] playerAnimators;         // Animators for each player color.

    [Header("Components")]
    public PhotonView photonView;

    // Events
    //[HideInInspector] public UnityEvent<int> onPlayerInitialized;       // Called when the player is initialized (int = player's ID).
    [HideInInspector] public System.Action<int> onPlayerInitialized;

    // Instance
    public static PlayerManager inst;

    void Awake ()
    {
        #region Singleton

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    // Initiates and sets up the player list.
    [PunRPC]
    public void InitiatePlayers ()
    {
        players = new NetworkPlayer[PhotonNetwork.playerList.Length];
        PhotonPlayer[] photonPlayerList = NetworkManager.inst.GetOrderedPlayerList();

        for(int x = 0; x < players.Length; ++x)
        {
            players[x] = new NetworkPlayer();
            players[x].networkID = x;
            players[x].photonPlayer = photonPlayerList[x];
        }

        // If we're the host, network spawn all of the players.
        if(PhotonNetwork.isMasterClient)
            HostSpawnPlayers();
    }

    // Called by the host when all the players have joined the Game scene.
    // Instantiates and initiates all of the players.
    void HostSpawnPlayers ()
    {
        // Loop through all of the players.
        for(int x = 0; x < players.Length; ++x)
        {
            // Network instantaite the player prefab.
            GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, GameManager.inst.spawnPoints[x], Quaternion.identity, 0);

            // Transfer ownership to the respective player.
            PhotonView playerPhotonView = playerObj.GetComponent<PhotonView>();
            playerPhotonView.TransferOwnership(players[x].photonPlayer);

            // Set the player object data.
            photonView.RPC("SetPlayerObjectData", PhotonTargets.All, new object[2] { x, playerPhotonView.viewID });

            // Spawn the player.
            playerPhotonView.RPC("Spawn", players[x].photonPlayer, GameManager.inst.spawnPoints[x]);
        }

        // Once all the players have been spawned.
        photonView.RPC("AllPlayersSpawned", PhotonTargets.All);
    }

    // Called when the host instantiates the player's GameObject.
    // Connects that player object to the corresponding network player.
    [PunRPC]
    public void SetPlayerObjectData (int id, int viewId)
    {
        GameObject playerObj = PhotonView.Find(viewId).gameObject;

        // Get player script.
        Player playerScript = playerObj.GetComponent<Player>();

        // Set values for the Player script.
        players[id].player = playerScript;
        playerScript.networkPlayer = players[id];

        playerScript.move.canMove = false;
        playerScript.attack.canAttack = false;

        if(onPlayerInitialized != null)
            onPlayerInitialized.Invoke(id);
    }

    // Called when all the players have been spawned.
    [PunRPC]
    void AllPlayersSpawned ()
    {
        GameManager.inst.onPlayersReady.Invoke();
    }

    // Returns the player with the respective ID.
    public NetworkPlayer GetPlayer (int id)
    {
        return players.First<NetworkPlayer>(x => x.networkID == id);
    }

    // Returns the player with the respective GameObject.
    public NetworkPlayer GetPlayer (GameObject playerObject)
    {
        return players.First<NetworkPlayer>(x => x.player.gameObject == playerObject);
    }

    // Returns a list of players excluding the sent one.
    public NetworkPlayer[] GetOtherPlayers (int excludingID)
    {
        List<NetworkPlayer> others = players.ToList();
        others.RemoveAll(x => x.networkID == excludingID);
        return others.ToArray();
    }

    // Returns the respective animator controller for the requested player.
    public RuntimeAnimatorController GetAnimatorController (int playerID)
    {
        return playerAnimators[playerID];
    }

    // Called when a player disconnects from the game.
    // Removes them from the game.
    void RemovePlayer (PhotonPlayer player)
    {
        // Get the network player.
        NetworkPlayer networkPlayer = players.First<NetworkPlayer>(x => x.photonPlayer == player);
        PhotonNetwork.Destroy(networkPlayer.player.photonView);

        networkPlayer = null;

        // If we're the only player left, go back to lobby.
        if(PhotonNetwork.room.PlayerCount == 1)
            NetworkManager.inst.HostBackToMenu();
    }

    // Called when a player disconnects from the room.
    public void OnPhotonPlayerDisconnected (PhotonPlayer player)
    {
        RemovePlayer(player);
    }
}