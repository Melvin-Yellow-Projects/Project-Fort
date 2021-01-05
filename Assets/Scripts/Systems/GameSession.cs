/**
 * File Name: GameSession.cs
 * Description: Handles the current session of the game; Carries data between different levels
 * 
 * Authors: Will Lacey
 * Date Created: March 27, 2020
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
    /********** MARK: Variables **********/
    #region Variables

    [Tooltip("how fast to run the game's internal clock speed")]
    [SerializeField] [Range(0, 10)] private float gameSpeed = 1f;

    //[SyncVar]
    BinaryReader binaryReaderBuffer;

    #endregion

    /********** MARK: Public Properties **********/
    # region Public Properties

    public static GameSession Singleton { get; private set; }

    public bool IsOnline { get; set; } = false;

    public BinaryReader BinaryReaderBuffer
    {
        get
        {
            return binaryReaderBuffer;
        }

        set // TODO: this might need to be a server only attribute
        {
            binaryReaderBuffer = value;
        }
    }

    #endregion

    /********** MARK: Unity Functions **********/
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

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void InitGameSession()
    {
        Singleton = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void LoadMapFromReader() 
    {
        SaveLoadMenu.LoadMapFromReader();
    }

    /// <summary>
    ///     Destroys GameObject containing Game Session Class
    /// </summary>
    public void DestroyGameSession()
    {
        Singleton.gameSpeed = gameSpeed;
        Destroy(gameObject);
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    private void HandleActiveSceneChanged(Scene current, Scene next)
    {
        //if (next.name.StartsWith("Game Scene")) SpawnOfflinePlayer(); // HACK: i think this can be removed

        LoadMapFromReader(); // HACK: is this overkill to do it every scene change?
    }

    #endregion

    /********** MARK: Debug Functions **********/
    #region Debug Functions

    /**
    private void SpawnOfflinePlayer()
    {
        if (IsOnline) return;

        GameObject offlinePlayer = Instantiate(GameNetworkManager.Singleton.playerPrefab);
        HumanPlayer humanPlayer = offlinePlayer.GetComponent<HumanPlayer>();

        humanPlayer.enabled = true;
        humanPlayer.MyTeam.TeamIndex = 1;
    }
    */

    #endregion
}
