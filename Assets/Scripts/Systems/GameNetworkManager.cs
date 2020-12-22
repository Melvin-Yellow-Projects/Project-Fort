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

public class GameNetworkManager : NetworkManager
{
    /********** MARK: Variables **********/
    #region Variables

    bool isGameInProgress = false;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    public static event Action<HumanPlayer> OnServerAddPlayerEvent;

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
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();

        HumanPlayers.Add(player);

        OnServerAddPlayerEvent?.Invoke(player);
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

    #endregion
}
