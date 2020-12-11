﻿/**
 * File Name: GameMode.cs
 * Description: Configuration script for handling the game mode settings
 * 
 * Authors: Will Lacey
 * Date Created: December 6, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Mode Settings")]
public class GameMode : ScriptableObject
{
    /********** MARK: Game Variables **********/
    #region Game Variables

    //[SerializeField] bool hasMaximumRounds = false;

    //[SerializeField] int maxRounds = 0;

    [Tooltip("turnsPerRound")]
    [SerializeField] int turnsPerRound = 3;

    [SerializeField] int movesPerTurn = 5;

    //[SerializeField] int minMovesPerTurn = 2;

    [SerializeField] float turnTimerLength = 10f;

    [SerializeField] bool isUsingTurnTimer = false;



    //[SerializeField] bool isHotseat = false;

    //[SerializeField] bool canPlayerBreak = false;

    //[SerializeField] float playerBreakingTimerLength;

    #endregion

    /********** MARK: Game Instance Variables **********/
    #region Game Instance Variables

    //public bool isPlayerBreaking = false;

    #endregion

    /********** MARK: Class Properties **********/
    #region Class Properties

    public static GameMode Singleton { get; set; }

    public float TurnTimerLength
    {
        get
        {
            return turnTimerLength;
        }
    }

    public int TurnsPerRound
    {
        get
        {
            return turnsPerRound;
        }
    }

    public int MovesPerTurn
    {
        get
        {
            return movesPerTurn;
        }
    }

    #endregion
}