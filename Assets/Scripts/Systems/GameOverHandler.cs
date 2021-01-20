/**
 * File Name: GameOverHandler.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 19, 2020
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

    public static event Action ServerOnGameOver;

    public static event Action<string> ClientOnGameOver;

    #endregion
    /************************************************************/
    #region Server Functions

    public static GameOverHandler Prefab { get; set; }

    #endregion
    /************************************************************/
    #region Server Functions

    public override void OnStartServer()
    {
        Subscribe();
    }

    public override void OnStopServer()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [Server]
    private void Subscribe()
    {
        // 1. Flag when a fort is captured (or at the end of the breaking state)
        // 2. Get a fort's team and then it's associated player
        // 3. check if a player has lost all of his forts, if so, player has lost 
        Fort.ServerOnFortCaptured += HandleServerOnFortCaptured;


        // 1. Flag when a unit dies
        // 2. Get a unit's team and then it's associated player
        // 3. check if a player is out of units, if so, player has lost
        //Unit.OnUnitDepawned += null;
    }

    [Server]
    private void Unsubscribe()
    {
        Fort.ServerOnFortCaptured -= HandleServerOnFortCaptured;
        //Unit.OnUnitDepawned -= null;
    }

    [Server]
    private void HandleServerOnFortCaptured(Fort fort)
    {
        Debug.LogWarning($"Fort \"{fort.name}\" has been captured by {fort.MyCell.MyUnit.name}");

        //if (bases.Count != 1) { return; }

        //int playerId = bases[0].connectionToClient.connectionId;

        //RpcGameOver($"Player {playerId}");

        //ServerOnGameOver?.Invoke();
    }

    //[ClientRpc]
    //private void RpcGameOver(string winner)
    //{
    //    ClientOnGameOver?.Invoke(winner);
    //}

    #endregion
}
