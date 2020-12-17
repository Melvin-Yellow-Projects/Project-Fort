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

    #endregion

    /********** MARK: Properties **********/
    #region Properties

    public static PlayerMenu Singleton { get; set; }

    public Player MyPlayer { get; set; }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        // HACK: this will not work when there are multiple players
        MyPlayer = FindObjectOfType<Player>();

        GameManager.OnStartRound += SetMoveCountText;
        GameManager.OnStartTurn += SetMoveCountText;
        MyPlayer.OnCommandChange += SetMoveCountText; 
    }

    private void OnDestroy()
    {
        GameManager.OnStartRound -= SetMoveCountText;
        GameManager.OnStartTurn -= SetMoveCountText;
        MyPlayer.OnCommandChange -= SetMoveCountText;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    public void UpdateTimerText(string text)
    {
        moveTimerText.text = text;
    }

    private void SetMoveCountText()
    {
        GameMode gm = GameMode.Singleton;

        string moveCountString = (MyPlayer.MoveCount >= gm.MovesPerTurn) ?
            "MXX" : $"M{MyPlayer.MoveCount}";

        moveCountText.text = $"R{GameManager.Singleton.RoundCount}:" +
            $"T{GameManager.Singleton.TurnCount}:" +
            moveCountString;
    }

    #endregion

}
