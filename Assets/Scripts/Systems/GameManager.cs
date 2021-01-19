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
    public static event Action RpcOnStartRound;

    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    public static event Action RpcOnStartTurn;
    
    /// <summary>
    /// Client event for when a new turn has begun
    /// </summary>
    /// <subscriber class="Player">disables controls when units are moving</subscriber>
    public static event Action RpcOnPlayTurn;
    
    /// <summary>
    /// Client event for turn has stopped playing
    /// </summary>
    /// <subscriber class="Player">enables controls when units are moving</subscriber>
    public static event Action RpcOnStopTurn;

    #endregion
    /************************************************************/
    #region Properties

    public static GameManager Singleton { get; private set; }

    public int RoundCount { get; [Server] private set; } = 0;

    public int TurnCount { get; [Server] private set; } = 0;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        if (NetworkServer.localConnection == null ||
            NetworkServer.localConnection.connectionId != NetworkClient.connection.connectionId)
        {
            enabled = false;
            gameObject.SetActive(false);
        }
        else
        {
            turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
            
            Subscribe();
        }
    }

    [Server]
    private void Start()
    {
        enabled = GameMode.Singleton.IsUsingTurnTimer;
        StartRound();
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
            PlayTurn();
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
    public void StartRound() // HACK: maybe these functions should be reversed... i.e. RoundStart()
    {
        RoundCount++;
        TurnCount = 0;

        ServerOnStartRound?.Invoke();
        InvokeRpcOnStartRound();

        StartTurn();
    }

    [Server]
    private void StartTurn()
    {
        TurnCount++;

        ServerOnStartTurn?.Invoke();
        InvokeRpcOnStartTurn();

        // update timer and its text
        if (GameMode.Singleton.IsUsingTurnTimer) ResetTimer();
        else PlayerMenu.UpdateTimerText("Your Turn");
    }

    [Server]
    public void PlayTurn()
    {
        enabled = false;
        PlayerMenu.UpdateTimerText("Executing Turn");

        StopAllCoroutines();
        StartCoroutine(PlayTurn(8));
    }

    #endregion
    /************************************************************/
    #region Server Functions

    [Server] // HACK:  units are looped over several times
    private IEnumerator PlayTurn(int numberOfTurnSteps) 
    {
        ServerOnPlayTurn?.Invoke();
        InvokeRpcOnPlayTurn();

        // How many Moves/Steps Units can Utilize
        for (int step = 0; step < numberOfTurnSteps; step++)
        {
            MoveUnits();

            yield return WaitForUnits();

            CompleteTurnStep();
        }

        ServerOnStopTurn?.Invoke();
        InvokeRpcOnStopTurn();

        // Finished Turn, either start new one or start a new round
        if (TurnCount >= GameMode.Singleton.TurnsPerRound) StartRound();
        else StartTurn();
    }

    [Server]
    private void MoveUnits()
    {
        // Moving Units
        for (int i = 0; i < HexGrid.Singleton.units.Count; i++)
        {
            Unit unit = HexGrid.Singleton.units[i];
            unit.Movement.DoAction(); // TODO: correct number of steps
        }
    }

    [Server]
    private IEnumerator WaitForUnits()
    {
        // Waiting for Units
        for (int i = 0; i < HexGrid.Singleton.units.Count; i++)
        {
            Unit unit = HexGrid.Singleton.units[i];
            if (unit.Movement.IsEnRoute)
            {
                i = -1;
                yield return null;
            }
        }
    }

    [Server]
    private void CompleteTurnStep()
    {
        // Setting new cell for Units now that they moved
        for (int i = 0; i < HexGrid.Singleton.units.Count; i++)
        {
            Unit unit = HexGrid.Singleton.units[i];
            unit.Movement.CompleteAction();
        }
    }

    [Server]
    private void ResetTimer()
    {
        //timeOfNextMove += GameMode.Singleton.TurnTimerLength;
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
        enabled = true;
    }
    #endregion
    /************************************************************/
    #region Client Functions

    [ClientRpc]
    private void InvokeRpcOnStartRound()
    {
        RpcOnStartRound?.Invoke();
    }

    [ClientRpc]
    private void InvokeRpcOnStartTurn()
    {
        RpcOnStartTurn?.Invoke();
    }

    [ClientRpc]
    private void InvokeRpcOnPlayTurn()
    {
        RpcOnPlayTurn?.Invoke();
    }

    [ClientRpc]
    private void InvokeRpcOnStopTurn()
    {
        RpcOnStopTurn?.Invoke();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        
    }

    private void Unsubscribe()
    {
        
    }
    #endregion
}
