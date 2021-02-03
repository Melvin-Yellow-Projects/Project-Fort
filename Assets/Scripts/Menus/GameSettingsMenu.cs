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
        turnTimerToggle.isOn = GameSettings.IsUsingTurnTimer;
        turnTimerSlider.value = GameSettings.TurnTimerLength / 10;
        SetTurnTimerInteractable();
    }

    public void Set()
    {
        /** Turn Timer **/
        GameSettings.IsUsingTurnTimer = turnTimerToggle.isOn;
        GameSettings.TurnTimerLength = turnTimerSlider.value * 10;
        SetTurnTimerInteractable();
    }

    #endregion
    /************************************************************/
    #region Private Class Functions

    private void SetTurnTimerInteractable()
    {
        if (GameSettings.IsUsingTurnTimer)
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

    private string GetTurnTimerText()
    {
        int min = (int)GameSettings.TurnTimerLength / 60;
        int sec = (int)GameSettings.TurnTimerLength % 60;

        return  $"{min} min {sec} sec";
    }
    #endregion
}
