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
public class PlayerDisplay : MonoBehaviour
{
    /************************************************************/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [SerializeField] TMP_Text moveCountText = null;
    [SerializeField] TMP_Text creditsText = null;
    [SerializeField] TMP_Text gamePhaseText = null;
    [SerializeField] TMP_Text turnTimerText = null;
    [SerializeField] GameObject buyPanel = null;
    [SerializeField] Button endTurnButton = null;
    [SerializeField] TMP_Text endTurnButtonText = null;

    [SerializeField] TMP_Text[] pieceTexts = null;
    [SerializeField] TMP_Text[] costTexts = null;

    static HumanPlayer player = null;
    static int pieceId = 0;

    static float timer = 0;

    #endregion
    /************************************************************/
    #region Properties

    public static PlayerDisplay Singleton { get; set; }

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

    public static int PieceId
    {
        get
        {
            return pieceId;
        }
        set
        {
            pieceId = Mathf.Clamp(value, 0, Piece.Prefabs.Count);
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    private void Awake()
    {
        Singleton = this;
        enabled = false;
        PieceId = 0;

        for (int i = 0; i < Piece.Prefabs.Count; i++)
        {
            pieceTexts[i].text = Piece.Prefabs[i].Type.ToString(); // HACK: this should be piece title
            //pieceTexts[i].text = Piece.Prefabs[i].PieceTitle; 
            costTexts[i].text = $"{Piece.Prefabs[i].Credits}";
        }

        Subscribe();
    }

    private void LateUpdate()
    {
        UpdateTimerText();
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

        Singleton.endTurnButtonText.text = "End Turn";
        Singleton.endTurnButton.interactable = true;

        string moveCountString =
            $"M{MyPlayer.CurrentAvailableMoves} -> {MyPlayer.GetAvailableMovesForNextTurn()}";

        Singleton.moveCountText.text = $"R{GameManager.RoundCount}: T{GameManager.TurnCount}: " +
            moveCountString;

        Singleton.enabled = true;
    }

    public static void RefreshCreditsText()
    {
        Singleton.creditsText.text = $"{MyPlayer.Credits}";
    }

    private static void UpdateTimerText()
    {
        timer = Math.Max(GameManager.TurnTimer - Time.time, 0);

        if (timer > 10 || timer == 0) Singleton.turnTimerText.text = $"{(int)timer}";
        else Singleton.turnTimerText.text = $"{timer}0000".Substring(0, 3);
    }

    public static void EndTurnButtonPressed()
    {
        MyPlayer.HasEndedTurn = !MyPlayer.HasEndedTurn;

        if (MyPlayer.HasEndedTurn) Singleton.endTurnButtonText.text = "Cancel";
        else Singleton.endTurnButtonText.text = "End Turn";
    }

    public static void PlayerHasLost()
    {
        Singleton.endTurnButton.gameObject.SetActive(false);
        Singleton.buyPanel.gameObject.SetActive(false);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        Debug.Log("PlayerMenu Subscribing");

        GameManager.Client_OnStartRound += HandleClientOnStartRound;
        GameManager.Client_OnStartTurn += HandleClientOnStartTurn;
        GameManager.Client_OnPlayTurn += HandleClientOnPlayTurn;
    }

    private void Unsubscribe()
    {
        Debug.Log("PlayerMenu Unsubscribing");

        GameManager.Client_OnStartRound -= HandleClientOnStartRound;
        GameManager.Client_OnStartTurn -= HandleClientOnStartTurn;
        GameManager.Client_OnPlayTurn -= HandleClientOnPlayTurn;
    }

    private void HandleClientOnStartRound()
    {
        gamePhaseText.text = "Economy Phase";

        if (MyPlayer.enabled) buyPanel.SetActive(true);

        RefreshMoveCountText();
    }

    private void HandleClientOnStartTurn()
    {
        gamePhaseText.text = "Your Turn";

        buyPanel.SetActive(false);

        RefreshMoveCountText();
    }

    private void HandleClientOnPlayTurn()
    {
        gamePhaseText.text = "Executing Turn";

        endTurnButton.interactable = false;

        enabled = false;
    }

    #endregion
}