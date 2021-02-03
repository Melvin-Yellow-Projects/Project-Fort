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

[CreateAssetMenu(menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
    /************************************************************/
    #region Game Variables

    //[SerializeField] bool hasMaximumRounds = false;

    //[SerializeField] int maxRounds = 0;

    [Tooltip("turnsPerRound")]
    [SerializeField] int turnsPerRound = 3;

    [SerializeField] int movesPerTurn = 5;

    [SerializeField] int startingPlayerResources = 1200;

    //[SerializeField] int minMovesPerTurn = 2;

    [SerializeField] bool isUsingTurnTimer = false;

    [SerializeField] float turnTimerLength = 10f;

    //[SerializeField] bool isHotseat = false;

    //[SerializeField] bool canPlayerBreak = false;

    //[SerializeField] float playerBreakingTimerLength;

    #endregion
    /************************************************************/
    #region Game Instance Variables

    //public bool isPlayerBreaking = false;

    #endregion
    /************************************************************/
    #region Class Properties

    public static GameSettings Singleton { get; set; }

    public static bool IsUsingTurnTimer
    {
        get
        {
            return Singleton.isUsingTurnTimer;
        }
        set
        {
            Singleton.isUsingTurnTimer = value;
        }
    }

    public static float TurnTimerLength
    {
        get
        {
            return Singleton.turnTimerLength;
        }
        set
        {
            Singleton.turnTimerLength = value;
        }
    }

    public static int TurnsPerRound
    {
        get
        {
            return Singleton.turnsPerRound;
        }
        set
        {
            Singleton.turnsPerRound = value;
        }
    }

    public static int MovesPerTurn
    {
        get
        {
            return Singleton.movesPerTurn;
        }
        set
        {
            Singleton.movesPerTurn = value;
        }
    }

    public static int StartingPlayerResources
    {
        get
        {
            return Singleton.startingPlayerResources;
        }
        set
        {
            Singleton.startingPlayerResources = value;
        }
    }

    #endregion
}
