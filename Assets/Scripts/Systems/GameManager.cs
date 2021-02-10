/**
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
    /// <subscriber class="MapCamera">refreshes 'Next' index</subscriber>
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
    /// <subscriber class="MapCamera">refreshes 'Next' index</subscriber>
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

    public static bool IsGameInProgress { get; private set; } = false;

    public static int RoundCount { get; private set; }

    public static int TurnCount { get; private set; }

    public static bool IsEconomyPhase { get; private set; } = false;

    public static bool IsPlayingTurn { get; private set; } = false;

    public static float TurnTimer { get; private set; }

    public static List<Player> Players { get; set; } = new List<Player>();

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        enabled = false;
        Singleton = this;
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    [ServerCallback]
    private void LateUpdate()
    {
        // wait for timer to end or for players to end their turn
        if (Time.time <= TurnTimer) return;

        //enabled = false;
        if (isServer) ServerEndTurn();
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

        IsGameInProgress = true;
        IsPlayingTurn = false;
        RoundCount = 0;

        ServerStartRound();
    }

    [Server]
    public void ServerStartRound() 
    {
        if (!IsGameInProgress) return;

        RoundCount++;
        TurnCount = 0;
        IsEconomyPhase = true;

        // give 100 to a player for each fort they own // TODO: make this a setting
        foreach (Player player in Players) player.Resources += player.MyForts.Count * 100;

        ServerOnStartRound?.Invoke();
        RpcInvokeClientOnStartRound();

        // update timer and its text 
        if (GameSession.IsUsingTurnTimer) ResetTimer();  
    }

    [Server]
    private void ServerStartTurn()
    {
        if (!IsGameInProgress) return;

        TurnCount++;

        ServerOnStartTurn?.Invoke();
        RpcInvokeClientOnStartTurn();

        // update timer and its text
        if (GameSession.IsUsingTurnTimer) ResetTimer();
    }

    [Server]
    public void ServerPlayTurn()
    {
        StopAllCoroutines();
        StartCoroutine(ServerPlayTurn(8));
    }

    [Server]
    public static void ServerStopGame()
    {
        if (!IsGameInProgress) return;

        Players.Clear();
        IsGameInProgress = false;
    }

    #endregion
    /************************************************************/
    #region Other Server Functions

    [Server]
    public void ServerTryEndTurn()
    {
        bool playTurn = true;
        foreach (Player player in Players) playTurn &= player.HasEndedTurn;

        if (playTurn && !IsPlayingTurn) ServerEndTurn();
    }

    [Server]
    public void ServerEndTurn()
    {
        enabled = false;
        if (IsEconomyPhase)
        {
            ServerCheckIfPlayerLost();

            IsEconomyPhase = false;
            RpcInvokeClientOnStopEconomyPhase();
            ServerStartTurn();
        }
        else
        {
            ServerPlayTurn();
        }
    }

    [Server]
    public void ServerCheckIfPlayerLost()
    {
        for (int i = Players.Count; i > 0; i--)
        {
            Player player = Players[i - 1];

            Debug.Log($"{player.name} has {player.MyForts.Count} forts");
            if (player.MyForts.Count == 0)
                GameOverHandler.Singleton.ServerPlayerHasLost(player, WinConditionType.Conquest);
            else if (player.MyUnits.Count == 0)
                GameOverHandler.Singleton.ServerPlayerHasLost(player, WinConditionType.Routed);
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

        ServerCheckIfPlayerLost();

        // Finished Turn, either start new one or start a new round
        if (TurnCount >= GameSession.TurnsPerRound) ServerStartRound();
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

        // HACK: Bow Calculations
        ServerBowCalculations();
    }

    [Server]
    private void ServerBowCalculations()
    {
        List<Unit> deadUnits = new List<Unit>();

        for (int i = 0; i < HexGrid.Units.Count; i++)
        {
            BowCombat bow = HexGrid.Units[i].CombatHandler as BowCombat;

            if (bow && bow.CanFire) deadUnits.Add(bow.Fire());
        }
        foreach (Unit unit in deadUnits) if (unit) unit.Die();
    }

    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcInvokeClientOnStartRound()
    {
        if (LoadingDisplay.Singleton) LoadingDisplay.Done();

        Debug.Log("RpcInvokeClientOnStartRound");
        if (isClientOnly)
        {
            if (GameSession.IsUsingTurnTimer) ResetTimer();

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
        if (isClientOnly)
        {
            if (GameSession.IsUsingTurnTimer) ResetTimer();
            TurnCount++;
        }
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
    /************************************************************/
    #region Class Functions

    private void ResetTimer()
    {
        TurnTimer = Time.time + GameSession.TurnTimerLength;
        if (isServer) enabled = true;
    }

    #endregion
}
