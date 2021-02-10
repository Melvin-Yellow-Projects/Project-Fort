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
    [SerializeField] TMP_Text resourcesText = null;
    [SerializeField] TMP_Text gamePhaseText = null;
    [SerializeField] TMP_Text turnTimerText = null;
    [SerializeField] GameObject buyPanel = null;
    [SerializeField] Button endTurnButton = null;
    [SerializeField] TMP_Text endTurnButtonText = null;

    [SerializeField] TMP_Text[] unitTexts = null;
    [SerializeField] TMP_Text[] costTexts = null;

    static HumanPlayer player = null; 
    static int unitId = 0;

    static float timer = 0;

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

        for (int i = 0; i < Unit.Prefabs.Count; i++)
        {
            unitTexts[i].text = Unit.Prefabs[i].ClassTitle;
            //unitTexts[i].text = Unit.Prefabs[i].PieceTitle;
            costTexts[i].text = $"{Unit.Prefabs[i].Resources}";
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

        string moveCountString = $"M{MyPlayer.MoveCount}";

        Singleton.moveCountText.text = $"R{GameManager.RoundCount}: T{GameManager.TurnCount}:" +
            moveCountString;

        Singleton.enabled = true;
    }

    public static void RefreshResourcesText()
    {
        Singleton.resourcesText.text = $"{MyPlayer.Resources}";
    }

    private static void UpdateTimerText()
    {
        timer = Math.Max(GameManager.TurnTimer - Time.time, 0);

        if (timer > 10 || timer == 0) Singleton.turnTimerText.text = $"{(int)timer}";
        else Singleton.turnTimerText.text = $"{timer}0000".Substring(0, 3);
    }

    public static void EndTurnButtonPressed()
    {
        MyPlayer.EndTurnButtonPressed();
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

        GameManager.ClientOnStartRound += HandleClientOnStartRound;
        GameManager.ClientOnStartTurn += HandleClientOnStartTurn;
        GameManager.ClientOnPlayTurn += HandleClientOnPlayTurn;

        //Player.ClientOnResourcesUpdated += null;
        Player.ClientOnHasEndedTurn += HandleClientOnHasEndedTurn;
    }

    private void Unsubscribe()
    {
        Debug.Log("PlayerMenu Unsubscribing");

        GameManager.ClientOnStartRound -= HandleClientOnStartRound;
        GameManager.ClientOnStartTurn -= HandleClientOnStartTurn;
        GameManager.ClientOnPlayTurn -= HandleClientOnPlayTurn;

        //Player.ClientOnResourcesUpdated -= null;
        Player.ClientOnHasEndedTurn -= HandleClientOnHasEndedTurn;
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

    private void HandleClientOnHasEndedTurn()
    {
        if (MyPlayer.HasEndedTurn) Singleton.endTurnButtonText.text = "Cancel";
        else Singleton.endTurnButtonText.text = "End Turn";
    }

    #endregion
}