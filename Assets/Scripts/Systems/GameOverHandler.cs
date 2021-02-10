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
    public void ServerPlayerHasLost(Player player, WinConditionType type)
    {
        GameManager.Players.Remove(player);
        Debug.LogWarning($"{player.name} has lost, there are {GameManager.Players.Count} Players");

        foreach (Fort fort in player.MyForts)
        {
            fort.HideBuyCells();
            fort.MyTeam.SetTeam(0);
        }
        foreach (Unit unit in player.MyUnits)
        {
            unit.MyTeam.SetTeam(0);
            unit.MyCell.DecreaseVisibility();
        }

        player.MyUnits.Clear();
        player.MyForts.Clear();

        // HACK: fix this function up later
        if (player.connectionToClient != null)
        {
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
        }

        player.enabled = false;

        ServerCheckIfGameOver();
    }

    [Server]
    public void ServerCheckIfGameOver()
    {
        if (GameManager.Players.Count != 1) return;

        RpcGameOver(GameManager.Players[0].Info.PlayerName);

        GameManager.Players[0].enabled = false;

        GameManager.ServerStopGame();

        NetworkServer.Destroy(GameManager.Singleton.gameObject);
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [TargetRpc]
    private void TargetDefeat(NetworkConnection conn)
    {
        TargetOnDefeat?.Invoke();

        PlayerMenu.PlayerHasLost();

        if (isServer) return;

        HumanPlayer player = conn.identity.GetComponent<HumanPlayer>();

        foreach (Fort fort in player.MyForts) fort.HideBuyCells();
        foreach (Unit unit in player.MyUnits) unit.MyCell.DecreaseVisibility();

        player.MyUnits.Clear();
        player.MyForts.Clear();

        player.enabled = false;
    }

    [ClientRpc]
    private void RpcGameOver(string winner)
    {
        ClientOnGameOver?.Invoke(winner);

        HumanPlayer player = NetworkClient.connection.identity.GetComponent<HumanPlayer>();

        //if (player) Destroy(player);
    }

    #endregion
    /************************************************************/
    #region Server Functions

    private void Start()
    {
        Singleton = this;
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

    

    #endregion
}
