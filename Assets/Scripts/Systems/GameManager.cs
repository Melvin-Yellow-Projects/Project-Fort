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
    /// <subscriber class="Unit">clears a unit's path</subscriber>
    public static event Action OnStartTurn;

    /// <summary>
    /// Event for when unit moves have started
    /// </summary>
    /// <subscriber class="Player">disables controls when units are moving</subscriber>
    public static event Action OnStartMoveUnits;

    /// <summary>
    /// Event for when unit moves have finished
    /// </summary>
    /// <subscriber class="Player">enables controls when units are moving</subscriber>
    /// <subscriber class="Fort">checks to see if team has updated</subscriber>
    /// <subscriber class="Unit">sets a unit's movement to 0 if it has moved</subscriber>
    public static event Action OnStopMoveUnits;

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public static GameManager Singleton { get; set; }

    public int RoundCount { get; private set; } = 0;

    public int TurnCount { get; private set; } = 0;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
        Singleton = this;
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
            PlayerMenu.Singleton.UpdateTimerText(text);
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

        if (GameMode.Singleton.IsUsingTurnTimer) ResetTimer();
        else
        {
            PlayerMenu.Singleton.UpdateTimerText("Your Turn");
        }
    }

    public void PlayTurn()
    {
        MoveUnits();
    }

    private void StopTurn()
    {
        if (TurnCount >= GameMode.Singleton.TurnsPerRound) StartRound();
        else StartTurn();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void MoveUnits()
    {
        enabled = false;
        PlayerMenu.Singleton.UpdateTimerText("Executing Turn");

        StopAllCoroutines();
        StartCoroutine(MoveUnits(8));
    }

    private IEnumerator MoveUnits(int numberOfSteps) // HACK:  units are looped over several times
    {
        OnStartMoveUnits?.Invoke();

        HexGrid grid = HexGrid.Singleton;

        // How many Moves/Steps Units can Utilize
        for (int stepCount = 0; stepCount < numberOfSteps; stepCount++)
        {
            // Moving Units
            for (int i = 0; i < grid.units.Count; i++) 
            {
                Unit unit = grid.units[i];
                unit.DoAction(); // TODO: correct number of steps
            }

            // Waiting for Units
            for (int i = 0; i < grid.units.Count; i++)
            {
                Unit unit = grid.units[i];
                if (unit.IsEnRoute)
                {
                    i = 0;
                    yield return null;
                }
                
            }

            // Setting new cell for Units now that they moved
            for (int i = 0; i < grid.units.Count; i++) 
            {
                Unit unit = grid.units[i];
                unit.CompleteAction();
            }
        }

        OnStopMoveUnits?.Invoke();

        StopTurn();
    }

    private void ResetTimer()
    {
        //timeOfNextMove += GameMode.Singleton.TurnTimerLength;
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
        enabled = true;
    }

    #endregion
}
