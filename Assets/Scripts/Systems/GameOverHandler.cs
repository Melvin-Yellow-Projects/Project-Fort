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

    public override void OnStartServer()
    {

    }

    public override void OnStopServer()
    {

    }

    [Server]
    private void ServerHandleFortCaptured()
    {
        //if (bases.Count != 1) { return; }

        //int playerId = bases[0].connectionToClient.connectionId;

        //RpcGameOver($"Player {playerId}");

        //ServerOnGameOver?.Invoke();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
