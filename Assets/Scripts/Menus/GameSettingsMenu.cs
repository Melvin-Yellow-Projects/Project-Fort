/**
 * File Name: GameSettingsMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 3, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 
/// </summary>
public class GameSettingsMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]

    //[Header("Turn Timer")]
    [SerializeField] Toggle turnTimerToggle = null;
    [SerializeField] Slider turnTimerSlider = null;
    [SerializeField] TMP_Text turnTimerText = null;

    #endregion
    /************************************************************/
    #region Public Class Functions

    public void Get()
    {
        /** Turn Timer **/
        turnTimerToggle.isOn = GameSession.IsUsingTurnTimer;
        turnTimerSlider.value = GameSession.TurnTimerLength / 10;
        SetTurnTimerInteractable();
    }

    public void Set()
    {
        /** Turn Timer **/
        GameSession.IsUsingTurnTimer = turnTimerToggle.isOn;
        GameSession.TurnTimerLength = (int) turnTimerSlider.value * 10;
        SetTurnTimerInteractable();
    }

    #endregion
    /************************************************************/
    #region Private Class Functions

    private void SetTurnTimerInteractable()
    {
        if (Mirror.NetworkClient.connection.identity.GetComponent<PlayerInfo>().IsPartyLeader)
        {
            turnTimerToggle.interactable = true;
            if (GameSession.IsUsingTurnTimer)
            {
                turnTimerSlider.interactable = true;
                turnTimerText.text = GetTurnTimerText();
            }
            else
            {
                turnTimerSlider.interactable = false;
                turnTimerText.text = "off";
            }
        }
        else
        {
            turnTimerToggle.interactable = false;
            turnTimerSlider.interactable = false;
        }    
    }

    private string GetTurnTimerText()
    {
        int min = GameSession.TurnTimerLength / 60;
        int sec = GameSession.TurnTimerLength % 60;

        return  $"{min} min {sec} sec";
    }
    #endregion
}
