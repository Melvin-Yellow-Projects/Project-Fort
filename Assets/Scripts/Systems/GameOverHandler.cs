/**
 * File Name: GameOverHandler.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 19, 2021
 * 
 * Additional Comments: 
 **/

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    /************************************************************/
    #region Class Events

    //public static event Action ServerOnGameOver;

    /// <summary>
    /// 
    /// </summary>
    /// <subscriber class="GameOverMenu">displays the loss to the losing client</subscriber>
    public static event Action TargetOnDefeat;

    /// <summary>
    /// 
    /// </summary>
    /// <subscriber class="GameOverMenu">displays victory to all clients</subscriber>
    public static event Action<string> ClientOnGameOver;

    #endregion
    /************************************************************/
    #region Server Functions

    public static GameOverHandler Prefab { get; set; }

    public static GameOverHandler Singleton { get; private set; }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server]
    public override void OnStartServer()
    {
        Singleton = this;
        Subscribe();
    }

    [Server]
    public override void OnStopServer()
    {
        Debug.LogWarning("Server is Unsubbing the GameOverHandler");
        Singleton = null;
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [TargetRpc]
    private void TargetDefeat(NetworkConnection conn)
    {
        TargetOnDefeat?.Invoke();

        conn.identity.GetComponent<HumanPlayer>().enabled = false;
    }

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);

        NetworkClient.connection.identity.GetComponent<HumanPlayer>().enabled = false;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    [Server]
    private void Subscribe()
    {
        // 1. Flag when a fort is captured (or at the end of the breaking state)
        // 2. Get a fort's team and then it's associated player
        // 3. check if a player has lost all of his forts, if so, player has lost 
        Player.ServerOnPlayerDefeat += HandleServerOnPlayerDefeat;

        // 1. Flag when a unit dies
        // 2. Get a unit's team and then it's associated player
        // 3. check if a player is out of units, if so, player has lost
        //Unit.OnUnitDepawned += null;
    }

    [Server]
    private void Unsubscribe()
    {
        Player.ServerOnPlayerDefeat -= HandleServerOnPlayerDefeat;
        //Unit.OnUnitDepawned -= null;
    }

    private void HandleServerOnPlayerDefeat(Player player, WinConditionType type)
    {
        // HACK: assumes two players
        //ServerOnGameOver?.Invoke(); 
        RpcGameOver("The Winner");

        // TODO: check if player is AI, if AI, skip connections
        if (player.connectionToClient == null) return;

        switch (type)
        {
            case WinConditionType.Draw:
                TargetDefeat(player.connectionToClient);
                break;
            case WinConditionType.Conquest:
                TargetDefeat(player.connectionToClient);
                break;
            case WinConditionType.Routed:
                TargetDefeat(player.connectionToClient);
                break;
            case WinConditionType.Disconnect:
                TargetDefeat(player.connectionToClient); // will line ever be sent to legit client?
                break;
        }

        // TODO: check for remaining players, then broadcast RpcGameOver

    }
    #endregion
}
