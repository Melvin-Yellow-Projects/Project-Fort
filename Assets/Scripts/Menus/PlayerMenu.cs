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
using UnityEngine.UI;
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
    [SerializeField] Button endTurnButton = null;
    [SerializeField] TMP_Text endTurnButtonText = null;

    static Player player = null;

    #endregion
    /************************************************************/
    #region Class Events

    // FIXME: Verify Player Menu

    /// <summary>
    /// Server event for when a player has pressed their end turn button
    /// </summary>
    /// <subscriber class="HumanPlayer">sends client button press data to the server</subscriber>
    public static event Action ClientOnEndTurnButtonPressed;

    #endregion
    /************************************************************/
    #region Properties

    public static PlayerMenu Singleton { get; set; }

    public static Player MyPlayer 
    {
        get
        {
            return player;
        }
        set
        {
            if (!value) return; // HACK: is this line needed
            player = value;
            Singleton.enabled = true;
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

        if (!MyPlayer) return;

        string moveCountString = (MyPlayer.MoveCount > gm.MovesPerTurn) ?
            "MXX" : $"M{MyPlayer.MoveCount}";

        Singleton.moveCountText.text = $"R{GameManager.RoundCount}:" +
            $"T{GameManager.TurnCount}:" +
            moveCountString;
    }

    public static void EndTurnButtonPressed()
    {
        ClientOnEndTurnButtonPressed?.Invoke();

        if (MyPlayer.HasEndedTurn) Singleton.endTurnButtonText.text = "Cancel";
        else Singleton.endTurnButtonText.text = "End Turn";
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameManager.ClientOnStartRound += HandleClientOnStartRound;
        GameManager.ClientOnStartTurn += HandleClientOnStartTurn;
    }

    private void Unsubscribe()
    {
        GameManager.ClientOnStartRound -= HandleClientOnStartRound;
        GameManager.ClientOnStartTurn -= HandleClientOnStartTurn;
    }

    private void HandleClientOnStartRound()
    {
        Singleton.endTurnButtonText.text = "End Turn";

        RefreshMoveCountText();
    }

    private void HandleClientOnStartTurn()
    {
        Singleton.endTurnButtonText.text = "End Turn";

        RefreshMoveCountText();
    }

    #endregion
}