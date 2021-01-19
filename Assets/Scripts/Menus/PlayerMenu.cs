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
using TMPro;

/// <summary>
/// 
/// </summary>
public class PlayerMenu : MonoBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    /* Cached References */
    [Header("Cached References")]
    [SerializeField] TMP_Text moveCountText = null;
    [SerializeField] TMP_Text moveTimerText = null;

    Player player = null;

    #endregion

    /********** MARK: Properties **********/
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

    /********** MARK: Class Functions **********/
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

    /********** MARK: Class Functions **********/
    #region Class Functions

    public static void UpdateTimerText(string text)
    {
        Singleton.moveTimerText.text = text;
    }

    public static void RefreshMoveCountText()
    {
        GameMode gm = GameMode.Singleton;

        if (!Singleton.MyPlayer) return;

        string moveCountString = (Singleton.MyPlayer.MoveCount >= gm.MovesPerTurn) ?
            "MXX" : $"M{Singleton.MyPlayer.MoveCount}";

        Singleton.moveCountText.text = $"R{GameManager.Singleton.RoundCount}:" +
            $"T{GameManager.Singleton.TurnCount}:" +
            moveCountString;
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameManager.RpcOnStartRound += RefreshMoveCountText;
        GameManager.RpcOnStartTurn += RefreshMoveCountText;
    }

    private void Unsubscribe()
    {
        GameManager.RpcOnStartRound -= RefreshMoveCountText;
        GameManager.RpcOnStartTurn -= RefreshMoveCountText;
    }
    #endregion
}