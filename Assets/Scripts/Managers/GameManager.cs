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

/// <summary>
/// 
/// </summary>
public class GameManager : MonoBehaviour
{
    /********** MARK: Class Functions **********/
    #region Scene Functions

    public void ExecuteMoves()
    {
        // each player submits their moves
        //  

        HexGrid grid = FindObjectOfType<HexGrid>();
        for(int i = 0; i < grid.units.Count; i++) // FIXME: this should be a list of player units, not grid
        {
            HexUnit unit = grid.units[i];
            unit.Move(1); // FIXME: correct number of steps
        }
    }

    #endregion
    
}
