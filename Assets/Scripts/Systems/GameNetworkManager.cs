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

    //public static List<HumanPlayer> HumanPlayers { get; } = new List<HumanPlayer>();

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnServerConnect(NetworkConnection conn)
    {
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            Debug.LogWarning($"{item.Key} has connection {item.Value}");
        }

        Debug.LogWarning($"Now there are a total of {NetworkServer.connections.Count} conns!");

        if (!GameSession.Singleton.IsOnline) return;

        // TODO: make player a spectator
        if (!isGameInProgress) return;

        conn.Disconnect();
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        GameManager.Players.Remove(conn.identity.GetComponent<Player>());

        base.OnServerDisconnect(conn);
    }

    [Server]
    public override void OnStopServer()
    {
        GameManager.Players.Clear();

        isGameInProgress = false;
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();
        PlayerInfo playerInfo = conn.identity.GetComponent<PlayerInfo>();

        GameManager.Players.Add(player);

        player.MyTeam.TeamIndex = GameManager.Players.Count; // TODO: move to playerInfo
        playerInfo.IsPartyOwner = (GameManager.Players.Count == 1);
        playerInfo.PlayerName = $"Player {GameManager.Players.Count}";
    }

    [Server]
    public void ServerStartGame() // HACK move this into SceneLoader?
    {
        // HACK: hardcoded and tethered to LobbyMenu.UpdatePlayerTags()
        if (GameManager.Players.Count < 2) return; 

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

        bool isReady = true;
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            Debug.LogWarning($"{item.Key} has connection {item.Value}");

            if (item.Value.identity.TryGetComponent(out player))
            isReady &= player.HasCreatedMap;
        }

        Debug.LogError("TESTING!");

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
        GameManager.Players.Clear();
    }

    #endregion
}
