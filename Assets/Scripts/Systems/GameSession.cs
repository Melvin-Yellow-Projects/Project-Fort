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
    public static event Action ClientOnGameSettingsChanged;

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
    int startingPlayerResources;
    public static int StartingPlayerResources
    {
        get
        {
            return Singleton.startingPlayerResources;
        }
        set
        {
            Singleton.startingPlayerResources = value;
            Singleton.gameSettings.startingPlayerResources = value;
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

    //[Command(ignoreAuthority = true)]
    //public void CmdSetGameMode(GameSettings settings, NetworkConnectionToClient conn = null)
    //{
    //    if (GameNetworkManager.IsGameInProgress) return;

    //    Player player = conn.identity.GetComponent<Player>();
    //    if (!player.Info.IsPartyLeader) return;

    //    // coolio set the game settings!
    //    Debug.LogWarning("Server is setting new game settings!");
    //    SetGameSettings(settings);

    //    RpcSetGameMode(settings);
    //}

    #endregion
    /************************************************************/
    #region Client Functions

    //[ClientRpc]
    //private void RpcSetGameMode(GameSettings settings)
    //{
    //    if (isServer) return;
    //    // set the game settings
    //    Debug.Log("Client is recieving new game settings");
    //    SetGameSettings(settings);
    //}

    //[ClientRpc]
    //private void Rpc()

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
    /// Destroys GameObject containing Game Session Class
    /// </summary>
    public static void DestroySession()
    {
        Destroy(Singleton.gameObject);
        Singleton = null;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void HookOnGameSettingsInt32(int oldValue, int newValue)
    {
        Debug.LogWarning("Updating GameSettings");
        ClientOnGameSettingsChanged?.Invoke();
    }

    private void HookOnGameSettingsBool(bool oldValue, bool newValue)
    {
        Debug.LogWarning("Updating GameSettings");
        ClientOnGameSettingsChanged?.Invoke();
    }

    #endregion
}
