/**
 * File Name: GameMode.cs
 * Description: Configuration script for handling the game mode settings
 * 
 * Authors: Will Lacey
 * Date Created: December 6, 2020
 * 
 * Additional Comments: 
 * 
 *      Previously known as GameMode.cs
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    /************************************************************/
    #region Game Variables

    //[SerializeField] bool hasMaximumRounds = false;

    //[SerializeField] int maxRounds = 0;

    [Tooltip("turnsPerRound")]
    [SerializeField] public int turnsPerRound = 3;

    [SerializeField] public int movesPerTurn = 5;

    //[SerializeField] int minMovesPerTurn = 2;

    [SerializeField] public bool isUsingTurnTimer = false;

    [SerializeField] public int turnTimerLength = 10;

    [SerializeField] public int startingPlayerResources = 1200;

    //[SerializeField] bool isHotseat = false;

    //[SerializeField] bool canPlayerBreak = false;

    //[SerializeField] float playerBreakingTimerLength;

    #endregion
}