using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameObject[] playerUIs;  // Array of all 4 player UI's.

    public Text headerText;         // Text displaying remaining time, etc.
    public Text centerText;         // Text counting down at the start of the game and showing the winner.

    // Instance
    public static GameUI inst;

    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    void Start ()
    {
        // Make sure we only have the player UI's we need for each player.
        for(int x = 0; x < playerUIs.Length; ++x)
            playerUIs[x].SetActive(x < PhotonNetwork.playerList.Length ? true : false);
    }

    #region Subscribing to Events

    void OnEnable ()
    {
        // Subscribe to events.
        GameManager.inst.onPlayersReady.AddListener(OnPlayersReady);
        GameManager.inst.onGameWin += OnGameWin;
    }

    void OnDisable ()
    {
        // Un-subscribe from events.
        GameManager.inst.onPlayersReady.RemoveListener(OnPlayersReady);
        GameManager.inst.onGameWin -= OnGameWin;
    }

    #endregion

    void Update ()
    {
        // If we're playing the game and it's a time based game mode.
        if(GameManager.inst.gameState == GameState.Playing && GameManager.inst.gameMode == GameModeType.TimeBased)
        {
            int curTime = (int)GameManager.inst.TimeRemaining();
            headerText.text = curTime.ToString();
        }
    }

    // GameManager.onPlayersReady event.
    // Called when everyone is in the game and ready to go.
    // Displays an on-screen countdown.
    public void OnPlayersReady ()
    {
        StartCoroutine(Countdown(3));
    }

    // Countdown on-screen from a specific number.
    IEnumerator Countdown (int countdownFrom)
    {
        int curCountdownNum = countdownFrom;

        centerText.gameObject.SetActive(true);

        while(curCountdownNum != 0)
        {
            centerText.text = curCountdownNum.ToString();
            yield return new WaitForSeconds(1.0f);
            curCountdownNum--;
        }

        centerText.gameObject.SetActive(false);

        // If we're the host, invoke the onGameStart event - starting the game.
        if(PhotonNetwork.isMasterClient)
            GameManager.inst.photonView.RPC("StartGame", PhotonTargets.All);
    }

    // GameManager.onGameWin event.
    // Called when the game is over and a player has won.
    // Display that winning player on-screen.
    void OnGameWin (int winningPlayer)
    {
        centerText.gameObject.SetActive(true);

        // Get the winning player's name.
        string playerName = PlayerManager.inst.GetPlayer(winningPlayer).photonPlayer.NickName;
        centerText.text = playerName + " Wins";
        centerText.color = PlayerManager.inst.GetPlayer(winningPlayer).player.ui.playerColor;
    }
}