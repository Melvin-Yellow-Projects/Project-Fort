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
using UnityEngine.SceneManagement;
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
            ExecuteMoves(isResettingTimer:true);
        }

        // HACK: 0000 pads the buffer for the first few frames
        moveTimerText.text = $"{timeOfNextMove - Time.time}0000".Substring(0, 3);
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void ExecuteMoves(bool isResettingTimer)
    {
        // each player submits their moves
        //  

        HexGrid grid = FindObjectOfType<HexGrid>();
        for(int i = 0; i < grid.units.Count; i++) // FIXME: this should be a list of player units, not grid
        {
            HexUnit unit = grid.units[i];
            unit.Move(1); // FIXME: correct number of steps
        }

        if (isResettingTimer) ResetTimer();
    }

    private void ResetTimer()
    {
        timeOfNextMove += timeToMove;
    }

    #endregion
    
}
