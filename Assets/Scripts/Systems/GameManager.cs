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
using TMPro;

/// <summary>
/// 
/// </summary>
public class GameManager : MonoBehaviour
{
    /********** MARK: Private Variables **********/
    #region Private Variables

    [SerializeField] TMP_Text moveTimerText = null;

    float turnTimer = 0f;

    int roundCount = 0;

    int turnCount = 0;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a new round has begun
    /// </summary>
    /// <subscriber class="Unit">refreshes unit's movement</subscriber>
    public static event Action OnStartRound;

    /// <summary>
    /// Event for when a new turn has begun
    /// </summary>
    public static event Action OnStartTurn;

    /// <summary>
    /// Event for when unit moves have started
    /// </summary>
    /// <subscriber class="Player">disables controls when units are moving</subscriber>
    public static event Action OnStartMoveUnits;

    /// <summary>
    /// Event for starting/executing unit combat
    /// </summary>
    /// <subscriber class="HexCell">subs when unit enters a cell, later handles combat</subscriber>
    public static event Action OnUnitCombat;

    /// <summary>
    /// Event for when unit moves have finished
    /// </summary>
    /// <subscriber class="Player">enables controls when units are moving</subscriber>
    public static event Action OnStopMoveUnits; 

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
    }

    private void Start()
    {
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
            moveTimerText.text = $"{Math.Max(turnTimer - Time.time, 0)}0000".Substring(0, 3);
        }
    }

    #endregion

    /********** MARK: Game Flow Functions **********/
    #region Game Flow Functions

    private void StartRound() // HACK: maybe these functions should be reversed... i.e. RoundStart()
    {
        roundCount++;
        turnCount = 0;

        OnStartRound?.Invoke();

        StartTurn();
    }

    private void StartTurn()
    {
        turnCount++;

        OnStartTurn?.Invoke();

        ResetTimer();
        enabled = true;
    }

    private void PlayTurn()
    {
        MoveUnits();
    }

    private void StopTurn()
    {
        if (turnCount >= GameMode.Singleton.TurnsPerRound) StartRound();
        else StartTurn();
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void MoveUnits()
    {
        enabled = false;
        moveTimerText.text = "Executing Turn";

        StopAllCoroutines();
        StartCoroutine(MoveUnits(8));
    }

    private IEnumerator MoveUnits(int numberOfSteps) // HACK:  units are looped over three times
    {
        OnStartMoveUnits?.Invoke();

        HexGrid grid = HexGrid.Singleton;

        for (int stepCount = 0; stepCount < numberOfSteps; stepCount++)
        {
            // each player submits their moves
            for (int i = 0; i < grid.units.Count; i++)
            {
                Unit unit = grid.units[i];
                unit.PrepareNextMove();
            }

            OnUnitCombat?.Invoke();

            for (int i = 0; i < grid.units.Count; i++) // FIXME: this should be a list of player units, not grid
            {
                Unit unit = grid.units[i];
                unit.ExecuteNextMove(); // FIXME: correct number of steps
            }

            yield return new WaitForSeconds(0.4f); // HACK: hardcoded

        }

        OnStopMoveUnits?.Invoke();

        StopTurn();
    }

    private void ResetTimer()
    {
        //timeOfNextMove += GameMode.Singleton.TurnTimerLength;
        turnTimer = Time.time + GameMode.Singleton.TurnTimerLength;
    }

    #endregion
}
