using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public GameState gameState;                         // Current state of the game.
    public GameModeType gameMode;                       // Game Mode we're currently playing.
    public SpellDistributionType spellDistribution;     // How the spells are given to the player in-game (not including pickups).

    [Header("Win Conditions")]
    [HideInInspector]
    public int winningScore;                // The score needed to win the game (Score Based game mode).
    [HideInInspector]
    public float timeLimit;                 // Time until the game ends (Time Based game mode).
    private float startTime;                // Time the game started - used to keep track of time remaining.
    public float endGameHangTime;           // How long do we wait after the game ends, before going back to the menu?

    [Header("Death")]
    public float deadDuration;              // The time between the player dying and respawning.
    public float minYKillPos;               // Kills the player if they go below this height (so they don't fall forever).

    [Header("Components")]
    public PhotonView photonView;           // PhotonView component.
    private Camera cam;                     // Main camera, used to calculate the min Y kill pos.

    // Map
    [HideInInspector]
    public Vector3[] spawnPoints;           // Array of player spawn points.
    [HideInInspector]
    public bool mapLoaded;                  // Has the map loaded in?

    // Events
    [HideInInspector] public UnityEvent onMapLoaded;            // Called when the map has been loaded in.
    [HideInInspector] public UnityEvent onPlayersReady;         // Called when all the players are spawned in and ready to go.
    [HideInInspector] public UnityEvent onGameStart;            // Called when the game begins and the players can start fighting.
    [HideInInspector] public System.Action<int> onGameWin;      // Called when a player has won the game.

    // Instance
    public static GameManager inst;

    #region Subscribing to Events

    void OnEnable ()
    {
        // Subscribe to events.
        onMapLoaded.AddListener(OnMapLoaded);
        onPlayersReady.AddListener(OnPlayersReady);
    }

    void OnDisable ()
    {
        // Un-subscribe from events.
        onMapLoaded.RemoveListener(OnMapLoaded);
        onPlayersReady.RemoveListener(OnPlayersReady);
    }

    #endregion

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

        SetState(GameState.Initiation);
    }

    void Start ()
    {
        if(!PhotonNetwork.inRoom)
        {
            Debug.LogError("Cannot directly play this scene before connecting to network! <b>Start game in Menu scene.</b>");
            return;
        }

        // Initiate the game mode.
        InitiateGameMode();

        // Load the map.
        MapLoader.inst.LoadMap(PhotonNetwork.room.CustomProperties["map"].ToString());

        // Get the camera.
        cam = Camera.main;
    }

    void Update ()
    {
        // Are we currently playing the game?
        if(gameState == GameState.Playing)
        {
            // Host checks win condition every frame if it's a Time Based game mode.
            if(PhotonNetwork.isMasterClient && gameMode == GameModeType.TimeBased)
            {
                if(Time.time > startTime + timeLimit)
                    CheckWinCondition();
            }
        }
    }

    // Called when the map has been loaded in.
    void OnMapLoaded ()
    {
        // Calculate the min Y kill pos.
        minYKillPos = MapLoader.inst.GetLowestPoint() - 4.0f;

        // When the map has been loaded, tell everyone we're in the game and ready to go.
        if(PhotonNetwork.inRoom)
            NetworkManager.inst.photonView.RPC("ImInGame", PhotonTargets.AllBuffered);
    }

    // Called when all the players are spawned and ready to go.
    void OnPlayersReady ()
    {
        SetState(GameState.PreGame);
    }

    // Gets the gamemode from custom properties.
    void InitiateGameMode ()
    {
        int gm = (int)PhotonNetwork.room.CustomProperties["gamemode"];
        int gmProp = (int)PhotonNetwork.room.CustomProperties["gamemodeprop"];

        gameMode = (GameModeType)gm;

        switch(gameMode)
        {
            case GameModeType.ScoreBased:
                winningScore = gmProp;
                break;
            case GameModeType.TimeBased:
                timeLimit = (float)gmProp;
                break;
        }
    }

    // Called by the host after the countdown to begin the game.
    [PunRPC]
    public void StartGame ()
    {
        SetState(GameState.Playing);
        startTime = Time.time;
        onGameStart.Invoke();
    }

    // Checks if a player / can win the game. If so - end the game.
    public void CheckWinCondition ()
    {
        // If we're not currently playing the game, return.
        if(gameState != GameState.Playing) return;

        // Get the winning player.
        Player winningPlayer = GetWinningPlayer();
        bool hasWon = false;

        switch(gameMode)
        {
            case GameModeType.ScoreBased:
                hasWon = winningPlayer.score >= winningScore;
                break;
            case GameModeType.TimeBased:
                hasWon = Time.time > startTime + timeLimit;
                break;
        }

        // If the conditions are right for a win - then that player wins.
        if(hasWon)
            photonView.RPC("WinGame", PhotonTargets.All, winningPlayer.networkPlayer.networkID);
    }

    // Called when a player wins the game.
    [PunRPC]
    public void WinGame (int winningPlayer)
    {
        SetState(GameState.EndGame);
        onGameWin.Invoke(winningPlayer);

        // Go back to the menu in 'endGameHangTime' seconds.
        Invoke("GoBackToMenu", endGameHangTime);
    }

    // Called after a player wins the game, we go back to the menu.
    void GoBackToMenu ()
    {
        NetworkManager.inst.HostBackToMenu();
    }

    // Returns the player who is currently ahead in score.
    public Player GetWinningPlayer ()
    {
        Player winningPlayer = PlayerManager.inst.players[0].player;

        // Loop through all players, except for the first one as they're winning by default.
        for(int x = 1; x < PlayerManager.inst.players.Length; ++x)
        {
            Player curPlayer = PlayerManager.inst.players[x].player;

            if(curPlayer.score > winningPlayer.score)
                winningPlayer = curPlayer;
        }

        return winningPlayer;
    }

    // Sets the current game state.
    public void SetState (GameState state)
    {
        gameState = state;
    }

    // If this is of game mode Score Based, return the remaining time.
    public float TimeRemaining ()
    {
        return (startTime + timeLimit) - Time.time;
    }
}

// The game state keeps track of the current state of the game. This can dictate what gets checked and what things are allowed to happen.
public enum GameState
{
    Initiation,         // If we're currently waiting for players, spawning the map, initiating the players.
    PreGame,            // Counting down before the game begins.
    Playing,            // When the game is in progress.
    EndGame             // When the game has ended and a player has won.
}

// The game mode dictates the win condition for the game.
public enum GameModeType
{
    ScoreBased,         // First player to a specific score (kills) - wins.
    TimeBased           // After a set duration, the player with the most kills wins.
}

// How the spells are given to the player in-game (not including pickups).
public enum SpellDistributionType
{
    RandomOnStart,      // Random spell at the start of the game.
    RandomOnSpawn       // Random spell everytime the player spawns.
}