/**
 * File Name: PlayerMenu.cs
 * Description: Manages the player's User Interface
 * 
 * Authors: Will Lacey
 * Date Created: December 7, 2020
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
public class PlayerMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [SerializeField] TMP_Text moveCountText = null;
    [SerializeField] TMP_Text moveTimerText = null;

    Player player = null;

    #endregion
    /************************************************************/
    #region Class Events

    // FIXME: Verify Player Menu

    /// <summary>
    /// Server event for when a player has pressed their end turn button
    /// </summary>
    public static event Action ClientOnEndTurnButtonPressed;

    #endregion
    /************************************************************/
    #region Properties

    public static PlayerMenu Singleton { get; set; }

    public Player MyPlayer
    {
        get
        {
            return player;
        }
        set
        {
            if (!value) return;
            player = value;
            enabled = true;
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void Awake()
    {
        Singleton = this;
        enabled = false;

        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public static void UpdateTimerText(string text)
    {
        Singleton.moveTimerText.text = text;
    }

    public static void RefreshMoveCountText()
    {
        GameMode gm = GameMode.Singleton;

        if (!Singleton.MyPlayer) return;

        string moveCountString = (Singleton.MyPlayer.MoveCount > gm.MovesPerTurn) ?
            "MXX" : $"M{Singleton.MyPlayer.MoveCount}";

        Singleton.moveCountText.text = $"R{GameManager.RoundCount}:" +
            $"T{GameManager.TurnCount}:" +
            moveCountString;
    }

    public static void EndTurnButtonPressed()
    {
        ClientOnEndTurnButtonPressed?.Invoke();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameManager.ClientOnStartRound += RefreshMoveCountText;
        GameManager.ClientOnStartTurn += RefreshMoveCountText;
    }

    private void Unsubscribe()
    {
        GameManager.ClientOnStartRound -= RefreshMoveCountText;
        GameManager.ClientOnStartTurn -= RefreshMoveCountText;
    }
    #endregion
}