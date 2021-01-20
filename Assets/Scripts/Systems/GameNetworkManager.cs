/**
 * File Name: GameNetworkManager.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    /************************************************************/
    #region Variables

    [Tooltip("how long to wait for a player to load the map terrain")]
    [SerializeField, Range(0f, 60f)] float waitForPlayerToSpawnTerrain = 30f;

    bool isGameInProgress = false;

    Coroutine waitCoroutine;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Event for when a client connects to the server
    /// </summary>
    /// <subscriber class="MainMenu">...</subscriber>
    /// <subscriber class="PreLobbyMenu">...</subscriber>
    /// <subscriber class="LobbyMenu">...</subscriber>
    public static event Action OnClientConnectEvent;

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    public static event Action OnClientDisconnectEvent;

    #endregion
    /************************************************************/
    #region Properties

    public static GameNetworkManager Singleton
    {
        get
        {
            return singleton as GameNetworkManager;
        }
    }

    public static List<HumanPlayer> HumanPlayers { get; } = new List<HumanPlayer>();

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (!GameSession.Singleton.IsOnline) return;

        // TODO: make player a spectator
        if (!isGameInProgress) return;

        conn.Disconnect();
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (!autoCreatePlayer) return;

        HumanPlayers.Remove(conn.identity.GetComponent<HumanPlayer>());

        base.OnServerDisconnect(conn);
    }

    [Server]
    public override void OnStopServer()
    {
        HumanPlayers.Clear();

        isGameInProgress = false;
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();
        PlayerInfo playerInfo = conn.identity.GetComponent<PlayerInfo>();

        HumanPlayers.Add(player);

        player.MyTeam.TeamIndex = HumanPlayers.Count; // TODO: move to playerInfo
        playerInfo.IsPartyOwner = (HumanPlayers.Count == 1);
        playerInfo.PlayerName = $"Player {HumanPlayers.Count}";
    }

    [Server]
    public void ServerStartGame() // HACK move this into SceneLoader?
    {
        if (HumanPlayers.Count < 1) return;

        isGameInProgress = true;

        //Debug.Log("Changing scene");

        ServerChangeScene("Game Scene");
    }

    [Server]
    public override void OnServerSceneChanged(string sceneName) // HACK move this into SceneLoader?
    {
        Debug.Log("It's time to spawn a map!");
        HexGrid.ServerSpawnMapTerrain();

        if (!SceneLoader.IsGameScene) return;

        GameOverHandler gameOverHandler = Instantiate(GameOverHandler.Prefab);
        NetworkServer.Spawn(gameOverHandler.gameObject);
    }

    [Server]
    public void ServerPlayerHasCreatedMap(HumanPlayer player)
    {
        if (player.HasCreatedMap) return;
        player.HasCreatedMap = true;

        //Debug.Log("New Player Has Created Map.");

        bool isReady = true;
        foreach (HumanPlayer p in HumanPlayers) isReady &= p.HasCreatedMap;

        if (waitCoroutine != null) StopCoroutine(waitCoroutine);

        // if players aren't ready, set function param "isWaiting" to true
        waitCoroutine = StartCoroutine(WaitToSpawnMapEntities(!isReady)); 
    }

    [Server] // HACK: maybe this could be named better
    private IEnumerator WaitToSpawnMapEntities(bool isWaiting)
    {
        // forces wait for one frame so subscription methods can fire before units are spawned
        if (isWaiting) yield return new WaitForSeconds(waitForPlayerToSpawnTerrain);
        else yield return null;

        HexGrid.ServerSpawnMapEntities();

        yield return null;

        GameManager.Singleton.ServerStartGame();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [Client]
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        if (GameSession.Singleton.IsOnline) OnClientConnectEvent?.Invoke();
    }

    [Client]
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        if (GameSession.Singleton.IsOnline) OnClientDisconnectEvent?.Invoke();
    }

    public override void OnStopClient()
    {
        HumanPlayers.Clear();
    }

    #endregion
}
