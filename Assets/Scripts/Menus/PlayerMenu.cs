﻿/**
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
    [SerializeField] TMP_Text resourcesText = null;
    [SerializeField] TMP_Text moveTimerText = null;
    [SerializeField] GameObject buyPanel = null;
    [SerializeField] Button endTurnButton = null;
    [SerializeField] TMP_Text endTurnButtonText = null;

    static HumanPlayer player = null; 
    static int unitId = 0;

    #endregion
    /************************************************************/
    #region Properties

    public static PlayerMenu Singleton { get; set; }

    public static HumanPlayer MyPlayer 
    {
        get
        {
            return player;
        }
        set
        {
            if (!value) return; // HACK: is this line needed?
            player = value;
            Singleton.enabled = true;
        }
    }

    public static int UnitId
    {
        get
        {
            return unitId;
        }
        set
        {
            unitId = Mathf.Clamp(value, 0, Unit.Prefabs.Count);
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

    public static void RefreshMoveCountText()
    {
        if (!MyPlayer) return;

        string moveCountString = $"M{MyPlayer.MoveCount}";

        Singleton.moveCountText.text = $"R{GameManager.RoundCount}: T{GameManager.TurnCount}:" +
            moveCountString;
    }

    public static void RefreshResourcesText()
    {
        Singleton.resourcesText.text = $"{MyPlayer.Resources}";
    }

    // HACK: this should fetch the timer from the Game Manager
    public static void UpdateTimerText(string text)
    {
        Singleton.moveTimerText.text = text;
    }

    public static void EndTurnButtonPressed()
    {
        MyPlayer.EndTurnButtonPressed();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameManager.ClientOnStartRound += HandleClientOnStartRound;
        GameManager.ClientOnStartTurn += HandleClientOnStartTurn;
        GameManager.ClientOnPlayTurn += HandleClientOnPlayTurn;

        //Player.ClientOnResourcesUpdated += null;
        Player.ClientOnHasEndedTurn += HandleClientOnHasEndedTurn;
    }

    private void Unsubscribe()
    {
        GameManager.ClientOnStartRound -= HandleClientOnStartRound;
        GameManager.ClientOnStartTurn -= HandleClientOnStartTurn;
        GameManager.ClientOnPlayTurn -= HandleClientOnPlayTurn;

        //Player.ClientOnResourcesUpdated -= null;
        Player.ClientOnHasEndedTurn -= HandleClientOnHasEndedTurn;
    }

    private void HandleClientOnStartRound()
    {
        UpdateTimerText("Economy Phase");

        buyPanel.SetActive(true);

        Singleton.endTurnButtonText.text = "End Turn";
        endTurnButton.interactable = true;

        RefreshMoveCountText();
    }

    private void HandleClientOnStartTurn()
    {
        UpdateTimerText("Your Turn");

        buyPanel.SetActive(false);

        Singleton.endTurnButtonText.text = "End Turn";
        endTurnButton.interactable = true;

        RefreshMoveCountText();
    }

    private void HandleClientOnPlayTurn()
    {
        UpdateTimerText("Executing Turn");

        endTurnButton.interactable = false;
    }

    private void HandleClientOnHasEndedTurn()
    {
        if (MyPlayer.HasEndedTurn) Singleton.endTurnButtonText.text = "Cancel";
        else Singleton.endTurnButtonText.text = "End Turn";
    }

    #endregion
}