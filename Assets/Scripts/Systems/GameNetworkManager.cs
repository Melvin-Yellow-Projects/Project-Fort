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
    /********** MARK: Variables **********/
    #region Variables

    bool isGameInProgress = false;

    #endregion

    /********** MARK: Class Events **********/
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

    /********** MARK: Properties **********/
    #region Properties

    public static GameNetworkManager Singleton
    {
        get
        {
            return singleton as GameNetworkManager;
        }
    }

    public List<HumanPlayer> HumanPlayers { get; } = new List<HumanPlayer>();

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    #endregion

    /********** MARK: Server Functions **********/
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

        ServerChangeScene("Game Scene");
    }

    [Server]
    public override void OnServerSceneChanged(string sceneName) // HACK move this into SceneLoader?
    {
        Debug.Log("It's time to spawn a map!");
        HexGrid.SpawnMap();

        if (!SceneLoader.IsGameScene) return;

        //GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
        //NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

        // this is needed because the HumanPlayer Script causes errors in the lobby menu if enabled
        for (int i = 0; i < HumanPlayers.Count; i++) HumanPlayers[i].enabled = true;
    }

    #endregion

    /********** MARK: Client Functions **********/
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
