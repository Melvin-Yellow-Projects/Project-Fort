/**
 * File Name: GameManager.cs
 * Description: Manages scene loading and persistent data
 * 
 * Authors: Will Lacey
 * Date Created: October 22, 2020
 * 
 * Additional Comments: 
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

    [SyncVar]
    int roundCount = 0;

    [SyncVar]
    int turnCount = 1;

    #endregion
    /************************************************************/
    #region Class Events

    // FIXME: Verify Player Menu

    /// <summary>
    /// Server event for when a new round has begun
    /// </summary>
    /// <subscriber class="Unit">refreshes unit's movement</subscriber>
    public static event Action ServerOnStartRound;

    /// <summary>
    /// Server event for when a new turn has begun
    /// </summary>
    public static event Action ServerOnStartTurn;

    /// <summary>
    /// Server event for turn is playing
    /// </summary>
    public static event Action ServerOnPlayTurn; // TODO: add validation for player command during turn

    /// <summary>
    /// Server event for turn has stopped playing
    /// </summary>
    /// <subscriber class="Fort">checks to see if team has updated</subscriber>
    /// <subscriber class="UnitMovement">sets a unit's movement to 0 if it has moved</subscriber>
    public static event Action ServerOnStopTurn; // TODO: add validation for player command during turn

    /// <summary>
    /// Client event for when a new round has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    public static event Action ClientOnStartRound;

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

    public static int RoundCount
    {
        get
        {
            return Singleton.roundCount;
        }
        set
        {
            Singleton.roundCount = value;
        }
    }

    public static int TurnCount
    {
        get
        {
            return Singleton.turnCount;
        }
        set
        {
            Singleton.turnCount = value;
        }
    }

    public static bool IsPlayingTurn { get; [Server] private set; } = false;

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

        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
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
            // Update Timer
            string text = $"{Math.Max(turnTimer - Time.time, 0)}0000".Substring(0, 3);
            PlayerMenu.UpdateTimerText(text);
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

        gameObject.SetActive(true);
        enabled = GameMode.Singleton.IsUsingTurnTimer;
        ServerStartRound();
    }

    [Server]
    public void ServerStartRound() // HACK: maybe these functions should be reversed... i.e. RoundStart()
    {
        RoundCount++;
        TurnCount = 0;

        ServerOnStartRound?.Invoke();
        RpcInvokeClientOnStartRound();

        ServerStartTurn();
    }

    [Server]
    private void ServerStartTurn()
    {
        TurnCount++;

        ServerOnStartTurn?.Invoke();
        RpcInvokeClientOnStartTurn();

        // update timer and its text
        if (GameMode.Singleton.IsUsingTurnTimer) ServerResetTimer();
        else PlayerMenu.UpdateTimerText("Your Turn");
    }

    [Server]
    public void ServerPlayTurn()
    {
        enabled = false;
        PlayerMenu.UpdateTimerText("Executing Turn");

        StopAllCoroutines();
        StartCoroutine(ServerPlayTurn(8));
    }

    #endregion
    /************************************************************/
    #region Other Server Functions

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
        if (TurnCount >= GameMode.Singleton.TurnsPerRound) ServerStartRound();
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
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
        enabled = true;
    }
    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void RpcInvokeClientOnStartRound()
    {
        Debug.Log("RpcInvokeClientOnStartRound");
        ClientOnStartRound?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnStartTurn()
    {
        Debug.Log("RpcInvokeClientOnStartTurn");
        ClientOnStartTurn?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnPlayTurn()
    {
        Debug.Log("RpcInvokeClientOnPlayTurn");
        ClientOnPlayTurn?.Invoke();
    }

    [ClientRpc]
    private void RpcInvokeClientOnStopTurn()
    {
        Debug.Log("RpcInvokeClientOnStopTurn");
        ClientOnStopTurn?.Invoke();
    }

    #endregion
}
