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

    [SerializeField] float timeToMove = 10f;

    private float timeOfNextMove = 0f;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for executing unit combat
    /// </summary>
    /// <subscriber name="HandleOnUnitCombat">HexCell Class, subs when unit enters cell</subscriber>
    public static event Action OnUnitCombat;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Awake()
    {
        timeOfNextMove = Time.time + timeToMove;
    }

    /// <summary>
    /// Unity Method; LateUpdate is called every frame, if the Behaviour is enabled and after all
    /// Update functions have been called
    /// </summary>
    private void LateUpdate()
    {
        if (Time.time > timeOfNextMove)
        {
            //MoveUnits(isResettingTimer:true);
            MoveUnits();
            enabled = false;
        }

        // HACK: 0000 pads the buffer for the first few frames
        moveTimerText.text = $"{Math.Max(timeOfNextMove - Time.time, 0)}0000".Substring(0, 3);
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void MoveUnits()
    {
        StopAllCoroutines();
        StartCoroutine(MoveUnits(8));
    }

    private IEnumerator MoveUnits(int numberOfSteps)
    {
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

        ResetTimer();
    }

    private void ResetTimer()
    {
        //timeOfNextMove += timeToMove;
        timeOfNextMove = timeToMove + Time.time;
        enabled = true;
    }

    #endregion
    
}
