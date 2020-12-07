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

    [SerializeField] TMP_Text moveCountText = null;

    public Player player = null; // HACK: This is definitely not a great way to do this

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void Awake()
    {
        GameManager.OnStartRound += SetMoveCountText;
        GameManager.OnStartTurn += SetMoveCountText;
        Player.OnCommandChange += SetMoveCountText;
    }

    private void OnDestroy()
    {
        GameManager.OnStartRound -= SetMoveCountText;
        GameManager.OnStartTurn -= SetMoveCountText;
        Player.OnCommandChange -= SetMoveCountText;
    }

    #endregion

    /********** MARK: Class Functions **********/
    #region Class Functions

    private void SetMoveCountText()
    {
        GameMode gm = GameMode.Singleton;

        string moveCountString = (player.MoveCount > gm.MovesPerTurn) ?
            "MXX" : $"M{player.MoveCount}";

        moveCountText.text = $"R{GameManager.Singleton.RoundCount}:" +
            $"T{GameManager.Singleton.TurnCount}:" +
            moveCountString;
    }

    #endregion

}
