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
    #region Unity Functions

    /// <summary>
    ///     Unity Method; Awake() is called before Start() upon GameObject creation
    /// </summary>
    private void Awake()
    {
        if (!Singleton) InitGameSession();

        else DestroyGameSession();
    }

    /// <summary>
    ///     Unity Method; Update() is called once per frame
    /// </summary>
    private void Update()
    {
        Time.timeScale = gameSpeed;
    }
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

        RpcSetGameMode(settings);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcSetGameMode(GameSettings settings)
    {
        // set the game settings
        Debug.Log("Client is recieving new game settings");
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void InitGameSession()
    {
        Singleton = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    ///     Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroyGameSession()
    {
        Singleton.gameSpeed = gameSpeed;
        Destroy(gameObject);
    }

    public static void GoOffline()
    {
        Singleton.IsOnline = false;
    }

    #endregion
}
