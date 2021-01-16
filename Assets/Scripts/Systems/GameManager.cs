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
public class GameManager : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    float turnTimer = 0f;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a new round has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    /// <subscriber class="Unit">refreshes unit's movement</subscriber>
    public static event Action OnStartRound;

    /// <summary>
    /// Event for when a new turn has begun
    /// </summary>
    /// <subscriber class="PlayerMenu">refreshes the round and turn count UI</subscriber>
    public static event Action OnStartTurn;

    /// <summary>
    /// Event for turn is playing
    /// </summary>
    /// <subscriber class="Player">disables controls when units are moving</subscriber>
    public static event Action OnPlayTurn;

    /// <summary>
    /// Event for turn has stopped playing
    /// </summary>
    /// <subscriber class="Player">enables controls when units are moving</subscriber>
    /// <subscriber class="Fort">checks to see if team has updated</subscriber>
    /// <subscriber class="UnitMovement">sets a unit's movement to 0 if it has moved</subscriber>
    public static event Action OnStopTurn;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public static GameManager Singleton { get; private set; }

    public int RoundCount { get; private set; } = 0;

    public int TurnCount { get; private set; } = 0;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        if (NetworkServer.localConnection == null ||
            NetworkServer.localConnection.connectionId != NetworkClient.connection.connectionId)
        {

            Destroy(gameObject);
        }
        else
        {
            turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
            Singleton = this;
        }
    }

    private void Start()
    {
        enabled = GameMode.Singleton.IsUsingTurnTimer;
        StartRound();
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
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

    /********** MARK: Game Flow Functions **********/
    #region Game Flow Functions

    public void StartRound() // HACK: maybe these functions should be reversed... i.e. RoundStart()
    {
        RoundCount++;
        TurnCount = 0;

        OnStartRound?.Invoke();

        StartTurn();
    }

    private void StartTurn()
    {
        TurnCount++;

        OnStartTurn?.Invoke();

        // update timer and its text
        if (GameMode.Singleton.IsUsingTurnTimer) ResetTimer();
        else PlayerMenu.UpdateTimerText("Your Turn");
    }

    public void PlayTurn()
    {
        enabled = false;
        PlayerMenu.UpdateTimerText("Executing Turn");

        StopAllCoroutines();
        StartCoroutine(PlayTurn(8));
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    // HACK:  units are looped over several times
    private IEnumerator PlayTurn(int numberOfTurnSteps) 
    {
        OnPlayTurn?.Invoke();

        // How many Moves/Steps Units can Utilize
        for (int step = 0; step < numberOfTurnSteps; step++)
        {
            MoveUnits();

            yield return WaitForUnits();

            CompleteTurnStep();
        }

        OnStopTurn?.Invoke();

        // Finished Turn, either start new one or start a new round
        if (TurnCount >= GameMode.Singleton.TurnsPerRound) StartRound();
        else StartTurn();
    }

    private void MoveUnits()
    {
        // Moving Units
        for (int i = 0; i < HexGrid.Singleton.units.Count; i++)
        {
            Unit unit = HexGrid.Singleton.units[i];
            unit.Movement.DoAction(); // TODO: correct number of steps
        }
    }

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

    private void CompleteTurnStep()
    {
        // Setting new cell for Units now that they moved
        for (int i = 0; i < HexGrid.Singleton.units.Count; i++)
        {
            Unit unit = HexGrid.Singleton.units[i];
            unit.Movement.CompleteAction();
        }
    }

    private void ResetTimer()
    {
        //timeOfNextMove += GameMode.Singleton.TurnTimerLength;
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
        enabled = true;
    }

    #endregion
}
