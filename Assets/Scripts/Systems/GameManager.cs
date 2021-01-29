﻿/**
 * File Name: GameManager.cs
 * Description: Manages scene loading and persistent data
 * 
 * Authors: Will Lacey
 * Date Created: October 22, 2020
 * 
 * Additional Comments: 
 * 
 *      HACK: this maybe could be a Monobehaviour
 *      HACK: functions can be updated to static methods
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

/// <summary>
/// 
/// </summary>
public class GameManager : NetworkBehaviour
{
    /************************************************************/
    #region Private Variables

    float turnTimer = 0f;

    #endregion
    /************************************************************/
    #region Class Events

    /// <summary>
    /// Server event for when a new round has begun
    /// </summary>
    /// <subscriber class="Unit">refreshes unit's movement</subscriber>
    public static event Action ServerOnStartRound;

    /// <summary>
    /// Server event for when a new turn has begun
    /// </summary>
    /// <subscriber class="Player">refreshes a player's turn/move status</subscriber>
    public static event Action ServerOnStartTurn;

    /// <summary>
    /// Server event for turn is playing
    /// </summary>
    /// <subscriber class="Player">refreshes a player's turn status</subscriber>
    public static event Action ServerOnPlayTurn;

    /// <summary>
    /// Server event for turn has stopped playing
    /// </summary>
    /// <subscriber class="Fort">checks to see if team has updated</subscriber>
    /// <subscriber class="UnitMovement">sets a unit's movement to 0 if it has moved</subscriber>
    public static event Action ServerOnStopTurn;

    /// <summary>
    /// Client event for when a new round has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    /// <subscriber class="HumanPlayer">updates player's buy cells and unit displays</subscriber>
    public static event Action ClientOnStartRound;

    /// <summary>
    /// Client event for when the Economy Phase ends
    /// </summary>
    /// <subscriber class="HumanPlayer">updates player's buy cells and unit displays</subscriber>
    public static event Action ClientOnStopEconomyPhase;

    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    public static event Action ClientOnStartTurn;

    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="HumanPlayer">disables controls when units are moving</subscriber>
    public static event Action ClientOnPlayTurn;

    /// <summary>
    /// Client event for turn has stopped playing
    /// </summary>
    /// <subscriber class="HumanPlayer">enables controls when units are moving</subscriber>
    public static event Action ClientOnStopTurn;

    #endregion
    /************************************************************/
    #region Properties

    public static GameManager Singleton { get; private set; }

    public static int RoundCount { get; private set; }

    public static int TurnCount { get; private set; }

    public static bool IsEconomyPhase { get; private set; }

    public static bool IsPlayingTurn { get; private set; } = false;

    public static List<Player> Players { get; set; } = new List<Player>();

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        gameObject.SetActive(false);
        enabled = false;
        Singleton = this;

        if (!isServer) return;

        turnTimer = Time.time + GameMode.TurnTimerLength;
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    [Server]
    private void LateUpdate()
    {
        // if Timer is done...
        if (Time.time > turnTimer)
        {
            ServerPlayTurn();
        }
        else
        {
            // Update Timer FIXME: this is just wrong
            //string text = $"{Math.Max(turnTimer - Time.time, 0)}0000".Substring(0, 3);
            //PlayerMenu.UpdateTimerText(text);
        }
    }

    private void OnDestroy()
    {
        Singleton = null;
    }

    #endregion
    /************************************************************/
    #region Server Game Flow Functions

    [Server]
    public void ServerStartGame()
    {
        Debug.LogWarning($"Starting Game with {Players.Count} Players");

        gameObject.SetActive(true); // turn on for just Server
        enabled = GameMode.IsUsingTurnTimer;
        ServerStartRound();
    }

    [Server]
    public void ServerStartRound() 
    {
        foreach (Player player in Players) player.Resources += 100;

        RoundCount++;
        TurnCount = 0;
        IsEconomyPhase = true;

        ServerOnStartRound?.Invoke();
        RpcInvokeClientOnStartRound();

        // update timer and its text // TODO: this should be the economy timer
        if (GameMode.IsUsingTurnTimer) ServerResetTimer();  // FIXME: this line must be moved to
                                                                        // PlayerMenu
    }

    [Server]
    private void ServerStartTurn()
    {
        TurnCount++;

        ServerOnStartTurn?.Invoke();
        RpcInvokeClientOnStartTurn();

        // update timer and its text
        if (GameMode.IsUsingTurnTimer) ServerResetTimer();
    }

    [Server]
    public void ServerPlayTurn()
    {
        enabled = false;

        StopAllCoroutines();
        StartCoroutine(ServerPlayTurn(8));
    }

    #endregion
    /************************************************************/
    #region Other Server Functions

    [Server]
    public void ServerTryEndTurn()
    {
        bool playTurn = true;
        foreach (Player player in Players) playTurn &= player.HasEndedTurn;

        if (playTurn && !IsPlayingTurn)
        {
            if (IsEconomyPhase)
            {
                IsEconomyPhase = false;
                RpcInvokeClientOnStopEconomyPhase();
                ServerStartTurn();
            }
            else
            {
                ServerPlayTurn();
            }
        }
    }

    [Server] // HACK:  units are looped over several times
    private IEnumerator ServerPlayTurn(int numberOfTurnSteps) 
    {
        IsPlayingTurn = true;
        ServerOnPlayTurn?.Invoke();
        RpcInvokeClientOnPlayTurn();

        // How many Moves/Steps Units can Utilize
        for (int step = 0; step < numberOfTurnSteps; step++)
        {
            ServerMoveUnits();

            yield return ServerWaitForUnits();

            ServerCompleteTurnStep();
        }

        ServerOnStopTurn?.Invoke();
        RpcInvokeClientOnStopTurn();

        IsPlayingTurn = false;

        // Finished Turn, either start new one or start a new round
        if (TurnCount >= GameMode.TurnsPerRound) ServerStartRound();
        else ServerStartTurn();
    }

    [Server]
    private void ServerMoveUnits()
    {
        // Moving Units
        for (int i = 0; i < HexGrid.Units.Count; i++)
        {
            Unit unit = HexGrid.Units[i];
            unit.Movement.ServerDoAction(); // TODO: correct number of steps
        }
    }

    [Server]
    private IEnumerator ServerWaitForUnits()
    {
        // Waiting for Units
        for (int i = 0; i < HexGrid.Units.Count; i++)
        {
            Unit unit = HexGrid.Units[i];
            if (unit.Movement.IsEnRoute)
            {
                i = -1;
                yield return null;
            }
        }
    }

    [Server]
    private void ServerCompleteTurnStep()
    {
        // Setting new cell for Units now that they moved
        for (int i = 0; i < HexGrid.Units.Count; i++)
        {
            Unit unit = HexGrid.Units[i];
            unit.Movement.ServerCompleteAction();
        }
    }

    [Server]
    private void ServerResetTimer()
    {
        //timeOfNextMove += GameMode.Singleton.TurnTimerLength;
        turnTimer = Time.time + GameMode.TurnTimerLength;
        enabled = true;
    }
    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcInvokeClientOnStartRound()
    {
        Debug.Log("RpcInvokeClientOnStartRound");
        if (isClientOnly)
        {
            IsEconomyPhase = true;
            RoundCount++;
            TurnCount = 0;

            // TODO: check to see if connection is still active, or if this line is needed
            //NetworkClient.connection.identity.GetComponent<Player>().Resources += 100;
        }
        ClientOnStartRound?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnStopEconomyPhase()
    {
        if (isClientOnly) IsEconomyPhase = false;
        ClientOnStopEconomyPhase?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnStartTurn()
    {
        Debug.Log("RpcInvokeClientOnStartTurn");
        if (isClientOnly) TurnCount++;
        ClientOnStartTurn?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnPlayTurn()
    {
        //Debug.Log("RpcInvokeClientOnPlayTurn");
        ClientOnPlayTurn?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnStopTurn()
    {
        //Debug.Log("RpcInvokeClientOnStopTurn");
        ClientOnStopTurn?.Invoke();
    }

    #endregion
}
