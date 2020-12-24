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
        // TODO: make player a spectator
        if (!isGameInProgress) return;
        conn.Disconnect();
    }

    [Server]
    public override void OnServerDisconnect(NetworkConnection conn)
    {
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

        //playerInfo.IsPartyOwner = (HumanPlayers.Count == 1);
        playerInfo.PlayerName = $"Player {HumanPlayers.Count}";
    }

    [Server]
    public void ServerStartGame()
    {
        if (HumanPlayers.Count < 2) return;

        isGameInProgress = true;

        ServerChangeScene("Game Scene");
    }

    [Server]
    public override void OnServerSceneChanged(string sceneName)
    {
        // HACK: string reference
        if (!SceneManager.GetActiveScene().name.StartsWith("Game Scene")) return;

        //GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
        //NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

        Debug.Log("It's time to spawn a map!");

        //foreach (RTSPlayer player in Players)
        //{
        //    // spawning unit base on server
        //    Vector3 pos = GetStartPosition().position;
        //    Quaternion rot = Quaternion.identity;
        //    GameObject unitBaseInstance = Instantiate(unitBasePrefab, pos, rot);
        //    UnitBase unitBase = unitBaseInstance.GetComponent<UnitBase>();
        //    unitBase.SteamId = player.GetComponent<RTSPlayerInfo>().SteamId;

        //    // spawning the small car on server
        //    pos = unitBase.SpawnPoint.position;
        //    GameObject smallCarInstance = Instantiate(smallCarPrefab, pos, rot);

        //    // server tells all clients to spawn instance, and sets authority to a connection
        //    NetworkServer.Spawn(unitBaseInstance, player.connectionToClient);
        //    NetworkServer.Spawn(smallCarInstance, player.connectionToClient);
        //}
    }

    #endregion

    /********** MARK: Client Functions **********/
    #region Client Functions

    [Client]
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnectEvent?.Invoke();

        Debug.Log("Hello! I have connected!"); // HACK: remove this code
    }

    [Client]
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnectEvent?.Invoke();
    }

    public override void OnStopClient()
    {
        HumanPlayers.Clear();
    }

    #endregion
}
