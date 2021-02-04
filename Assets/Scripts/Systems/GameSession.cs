/**
 * File Name: GameSession.cs
 * Description: Handles the current session of the game; Carries data between different levels
 * 
 * Authors: Will Lacey
 * Date Created: November 29, 2020
 * 
 * Additional Comments:
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using Mirror;

public class GameSession : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [Tooltip("game settings to store in the game's session")]
    //[SyncVar(hook = nameof(HookOnGameSettings))]
    [SerializeField] GameSettings gameSettings = null;

    [Header("Settings")]
    [Tooltip("how fast to run the game's internal clock speed")]
    [SerializeField] [Range(0, 10)] float gameSpeed = 1f;

    #endregion
    /************************************************************/
    #region Public Properties

    public static GameSession Singleton { get; private set; }

    public bool IsOnline { get; set; } = false; // HACK: this property isn't mega accurate 

    public bool IsEditorMode { get; set; } = false; // HACK change this, relocate it

    #endregion
    /************************************************************/
    #region Game Settings Properties

    // HACK: this is kinda shameless, but it should work for now

    public static GameSettings Settings
    {
        get
        {
            return Singleton.gameSettings;
        }
    }

    static int turnsPerRound;
    public static int TurnsPerRound
    {
        get
        {
            return turnsPerRound;
        }
        set
        {
            turnsPerRound = value;
            Singleton.gameSettings.turnsPerRound = value;
        }
    }

    static int movesPerTurn;
    public static int MovesPerTurn
    {
        get
        {
            return movesPerTurn;
        }
        set
        {
            movesPerTurn = value;
            Singleton.gameSettings.movesPerTurn = value;
        }
    }

    static bool isUsingTurnTimer;
    public static bool IsUsingTurnTimer
    {
        get
        {
            return isUsingTurnTimer;
        }
        set
        {
            isUsingTurnTimer = value;
            Singleton.gameSettings.isUsingTurnTimer = value;
        }
    }

    static int turnTimerLength;
    public static int TurnTimerLength
    {
        get
        {
            return turnTimerLength;
        }
        set
        {
            turnTimerLength = value;
            Singleton.gameSettings.turnTimerLength = value;
        }
    }

    static int startingPlayerResources;
    public static int StartingPlayerResources
    {
        get
        {
            return startingPlayerResources;
        }
        set
        {
            startingPlayerResources = value;
            Singleton.gameSettings.startingPlayerResources = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    /// <summary>
    ///     Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        if (!Singleton) InitalizeGameSettings();

        else DestroyGameSession();
    }

    /// <summary>
    ///     Unity Method; Update() is called once per frame
    /// </summary>
    //private void Update()
    //{
    //    Time.timeScale = gameSpeed;
    //}
    #endregion

    /************************************************************/
    #region Server Functions

    [Command(ignoreAuthority = true)]
    public void CmdSetGameMode(GameSettings settings, NetworkConnectionToClient conn = null)
    {
        if (GameNetworkManager.IsGameInProgress) return;

        Player player = conn.identity.GetComponent<Player>();
        if (!player.Info.IsPartyLeader) return;

        // coolio set the game settings!
        Debug.LogWarning("Server is setting new game settings!");
        SetGameSettings(settings);

        RpcSetGameMode(settings);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcSetGameMode(GameSettings settings)
    {
        if (isServer) return;
        // set the game settings
        Debug.Log("Client is recieving new game settings");
        SetGameSettings(settings);
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void InitalizeGameSettings()
    {
        Singleton = this;

        SetGameSettings(gameSettings);

        DontDestroyOnLoad(Singleton.gameObject);
    }

    private void SetGameSettings(GameSettings settings)
    {
        turnsPerRound = settings.turnsPerRound;
        movesPerTurn = settings.movesPerTurn;
        isUsingTurnTimer = settings.isUsingTurnTimer;
        turnTimerLength = settings.turnTimerLength;
        startingPlayerResources = settings.startingPlayerResources;
    }

    /// <summary>
    ///     Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroyGameSession()
    {
        Destroy(gameObject);
    }

    public static void GoOffline()
    {
        Singleton.IsOnline = false;
    }

    #endregion

    /************************************************************/
    #region Event Handler Functions

    private void HookOnGameSettings(GameSettings oldValue, GameSettings newValue)
    {
        Debug.LogError("Setting Game Settings");
    }    

    #endregion
}
