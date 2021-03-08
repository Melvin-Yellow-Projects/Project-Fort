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
using System;

public class GameSession : NetworkBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [Tooltip("game settings to store in the game's session")]
    [SerializeField] GameSettings gameSettings = null;

    [Header("Settings")]
    [Tooltip("how fast to run the game's internal clock speed")]
    [SerializeField] [Range(0, 10)] float gameSpeed = 1f;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when the client recieves a GameSettings update from the server
    /// </summary>
    /// <subscriber class="GameSettingsMenu">refreshes Game Settings Menu informtion</subscriber>
    public static event Action Client_OnGameSettingsChanged;

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    /// <subscriber class="LobbyMenu">...</subscriber>
    public static event Action OnClientDisconnectEvent;

    #endregion
    /************************************************************/
    #region Public Properties

    public static GameSession Singleton { get; private set; }

    public static bool IsOnline { get; set; } = false; // HACK: this property isn't mega accurate 

    public static bool IsEditorMode { get; set; } = false; // HACK change this, relocate it

    #endregion
    /************************************************************/
    #region Game Settings Properties

    // HACK: this is kinda shameless, but it should work for now

    [SyncVar(hook = nameof(HookOnGameSettingsInt32))]
    int turnsPerRound;
    public static int TurnsPerRound
    {
        get
        {
            return Singleton.turnsPerRound;
        }
        set
        {
            Singleton.turnsPerRound = value;
            Singleton.gameSettings.turnsPerRound = value;
        }
    }

    [SyncVar(hook = nameof(HookOnGameSettingsInt32))]
    int movesPerTurn;
    public static int MovesPerTurn
    {
        get
        {
            return Singleton.movesPerTurn;
        }
        set
        {
            Singleton.movesPerTurn = value;
            Singleton.gameSettings.movesPerTurn = value;
        }
    }

    [SyncVar(hook = nameof(HookOnGameSettingsBool))]
    bool isUsingTurnTimer;
    public static bool IsUsingTurnTimer
    {
        get
        {
            return Singleton.isUsingTurnTimer;
        }
        set
        {
            Singleton.isUsingTurnTimer = value;
            Singleton.gameSettings.isUsingTurnTimer = value;
        }
    }

    [SyncVar(hook = nameof(HookOnGameSettingsInt32))]
    int turnTimerLength;
    public static int TurnTimerLength
    {
        get
        {
            return Singleton.turnTimerLength;
        }
        set
        {
            Singleton.turnTimerLength = value;
            Singleton.gameSettings.turnTimerLength = value;
        }
    }

    [SyncVar(hook = nameof(HookOnGameSettingsInt32))]
    int startingCredits;
    public static int StartingCredits
    {
        get
        {
            return Singleton.startingCredits;
        }
        set
        {
            Singleton.startingCredits = value;
            Singleton.gameSettings.startingCredits = value;
        }
    }

    [SyncVar(hook = nameof(HookOnGameSettingsInt32))]
    int creditsPerFort;
    public static int CreditsPerFort
    {
        get
        {
            return Singleton.creditsPerFort;
        }
        set
        {
            Singleton.creditsPerFort = value;
            Singleton.gameSettings.creditsPerFort = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    /// <summary>
    /// Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        if (!Singleton) InitalizeGameSettings();

        else Destroy(gameObject);
    }

    /// <summary>
    /// Unity Method; Update() is called once per frame
    /// </summary>
    //private void Update()
    //{
    //    Time.timeScale = gameSpeed;
    //}

    #endregion
    /************************************************************/
    #region Server Functions

    [Command(ignoreAuthority = true)]
    public void CmdSetGameSettings(GameSettings settings, NetworkConnectionToClient conn = null)
    {
        if (GameNetworkManager.HasLaunchedGame) return;

        Player player = conn.identity.GetComponent<Player>();
        if (!player.Info.IsPartyLeader) return;

        Debug.LogWarning("Server is setting new game settings!");
        SetGameSettings(settings);
    }

    [Command(ignoreAuthority = true)] // HACK: i dont like this function here
    public void CmdLaunchGame(NetworkConnectionToClient conn = null)
    {
        if (!conn.identity.GetComponent<PlayerInfo>().IsPartyLeader) return;

        if (GameNetworkManager.HasLaunchedGame) return;

        GameNetworkManager.Singleton.ServerLaunchGame();
    }

    #endregion
    /************************************************************/
    #region Client Functions
    
    [ClientRpc]
    public void RpcClientHasDisconnected()
    {
        // HACK: this doesn't really belong here
        Debug.LogWarning("Client has disconnected");

        OnClientDisconnectEvent?.Invoke();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void InitalizeGameSettings()
    {
        Singleton = this;

        SetGameSettings(gameSettings);

        DontDestroyOnLoad(Singleton.gameObject);

        Debug.LogWarning("Game Session Initialized");
    }

    /// <summary>
    /// Sets the session's game settings; if this is the server, it will fire the sync vars for
    /// everything
    /// </summary>
    /// <param name="gameSettings"></param>
    private void SetGameSettings(GameSettings gameSettings)
    {
        turnsPerRound = gameSettings.turnsPerRound;
        movesPerTurn = gameSettings.movesPerTurn;
        isUsingTurnTimer = gameSettings.isUsingTurnTimer;
        turnTimerLength = gameSettings.turnTimerLength;
        startingCredits = gameSettings.startingCredits;
        creditsPerFort = gameSettings.creditsPerFort;
    }

    /// <summary>
    /// This function saves the current game settings in memory to 'disk' and then transmits the 
    /// data over the internet
    /// </summary>
    /// <returns></returns>
    public static GameSettings GetCopyOfGameSettings()
    {
        GameSettings newSettings = ScriptableObject.CreateInstance<GameSettings>();

        newSettings.turnsPerRound = TurnsPerRound;
        newSettings.movesPerTurn = MovesPerTurn;
        newSettings.isUsingTurnTimer = IsUsingTurnTimer;
        newSettings.turnTimerLength = TurnTimerLength;
        newSettings.startingCredits = StartingCredits;
        newSettings.creditsPerFort = CreditsPerFort;

        return newSettings;
    }

    /// <summary>
    /// Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroySession()
    {
        Debug.LogWarning("Destroying Session");
        Singleton = null;
        Destroy(gameObject);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void HookOnGameSettingsInt32(int oldValue, int newValue)
    {
        //Debug.LogWarning("Updating GameSettings");
        Client_OnGameSettingsChanged?.Invoke();
    }

    private void HookOnGameSettingsBool(bool oldValue, bool newValue)
    {
        //Debug.LogWarning("Updating GameSettings");
        Client_OnGameSettingsChanged?.Invoke();
    }

    #endregion
}
