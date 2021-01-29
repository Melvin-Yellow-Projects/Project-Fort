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

    [Header("Settings")]
    [Tooltip("whether or not this build is using Steam")]
    [SerializeField] bool isUsingSteam = false;

    [Tooltip("how long to wait for a player to load the map terrain")]
    [SerializeField, Range(0f, 60f)] float waitForPlayerToSpawnTerrain = 30f;

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

    public static bool IsUsingSteam { get; private set; } = false;

    public static bool IsGameInProgress { get; set; }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnValidate()
    {
        // the build has been changed from before, now time to change the transport
        if (isUsingSteam != IsUsingSteam) ChangeTransport();
        
        base.OnValidate();
    }

    [Server]
    public override void OnServerConnect(NetworkConnection conn)
    {
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            Debug.LogWarning($"{item.Key} has connection {item.Value.connectionId}");
        }

        Debug.LogWarning($"Now there are a total of {NetworkServer.connections.Count} conns!");

        if (!GameSession.Singleton.IsOnline) return;

        // TODO: make player a spectator
        if (!IsGameInProgress) return;

        conn.Disconnect();
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (!SceneLoader.IsGameScene) return;

        GameManager.Players.Remove(conn.identity.GetComponent<Player>());

        //base.OnServerDisconnect(conn);
        NetworkServer.Destroy(conn.identity.gameObject);

        // TODO: revoke authority and team on previously owned entities, see the code ->
        //          base.OnServerDisconnect(conn);
    }

    [Server]
    public override void OnStopServer()
    {
        GameManager.Players.Clear();

        autoCreatePlayer = true;
        GameSession.Singleton.IsOnline = false;
        GameSession.Singleton.IsEditorMode = false;

        IsGameInProgress = false;
    }

    [Server]
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();
        PlayerInfo playerInfo = conn.identity.GetComponent<PlayerInfo>();

        GameManager.Players.Add(player);

        player.MyTeam.SetTeam(GameManager.Players.Count); // TODO: move to playerInfo
        playerInfo.IsPartyOwner = (GameManager.Players.Count == 1);
        playerInfo.PlayerName = $"Player {GameManager.Players.Count}";
    }

    [Server]
    public void ServerStartGame() // HACK move this into SceneLoader?
    {
        // HACK: hardcoded and tethered to LobbyMenu.UpdatePlayerTags()
        if (GameManager.Players.Count < 2) return;

        IsGameInProgress = true;

        Debug.Log("Changing scene");

        ServerChangeScene(SceneLoader.GameSceneName);
    }

    [Server]
    public override void OnServerSceneChanged(string sceneName) 
    {
        // HACK: this code is really jank
        // HACK: move part of this into SceneLoader?

        Debug.Log($"Server has changed the Scene to {sceneName}");

        if (SceneLoader.IsGameScene && GameManager.Players.Count < 2)
            ServerSpawnComputerPlayer();

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

        // checks every connection to see if they are ready to load the rest of the map
        bool isReady = true;
        foreach (KeyValuePair<int, NetworkConnectionToClient> item in NetworkServer.connections)
        {
            isReady &= item.Value.identity.GetComponent<HumanPlayer>().HasCreatedMap;
        }

        if (waitCoroutine != null) StopCoroutine(waitCoroutine);

        // if connections aren't ready, set function param "isWaiting" to true
        waitCoroutine = StartCoroutine(WaitToSpawnMapEntities(isWaiting: !isReady)); 
    }

    [Server] // HACK: maybe this could be named better
    private IEnumerator WaitToSpawnMapEntities(bool isWaiting)
    {
        // forces wait for one frame so subscription methods can fire before units are spawned
        if (isWaiting) yield return new WaitForSeconds(waitForPlayerToSpawnTerrain);
        else yield return null;

        HexGrid.ServerSpawnMapEntities();

        yield return null; // waits one frame for connections to spawn entities, then launches game

        GameManager.Singleton.ServerStartGame();
    }

    [Server]
    public static void ServerSpawnComputerPlayer()
    {
        Debug.LogWarning("Spawning Computer Player");
        ComputerPlayer player = Instantiate(ComputerPlayer.Prefab);

        PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();

        GameManager.Players.Add(player);

        player.MyTeam.SetTeam(GameManager.Players.Count); // TODO: move to playerInfo
        playerInfo.PlayerName = $"Computer Player {GameManager.Players.Count}";

        NetworkServer.Spawn(player.gameObject);
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

    [Client]
    public override void OnStopClient()
    {
        GameManager.Players.Clear();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void ChangeTransport()
    {
        // update property to reflect the editor change
        IsUsingSteam = isUsingSteam;

        Transport kcpTransport = GetComponent<kcp2k.KcpTransport>();
        SteamManager steamManager = GetComponent<SteamManager>();
        Transport steamTransport = GetComponent<Mirror.FizzySteam.FizzySteamworks>();

        kcpTransport.enabled = !IsUsingSteam;
        steamManager.enabled = IsUsingSteam;
        steamTransport.enabled = IsUsingSteam;

        if (IsUsingSteam)
        {
            Debug.LogWarning("Changing Network Transport to Fizzy Steamworks");

            transport = steamTransport;
        }
        else
        {
            Debug.LogWarning("Changing Network Transport to KcpTransport");

            transport = kcpTransport;
        }
    }

    #endregion
}
