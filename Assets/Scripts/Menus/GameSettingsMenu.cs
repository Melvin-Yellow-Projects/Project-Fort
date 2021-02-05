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

    public bool Interactable { get; set; } = false; // TODO: write this functionality

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Start()
    {
        Subscribe();
        //Refresh();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void SetGameSettings()
    {
        if (!GeneralUtilities.GetPlayerFromClientConnection().Info.IsPartyLeader) return;

        // HACK this line isn't great, but it checks for whether or not the data should be sent
        //if (Input.getmou(0)) return; 
        Debug.Log("Setting Settings...");

        /** Turn Timer **/
        GameSession.IsUsingTurnTimer = turnTimerToggle.isOn;
        GameSession.TurnTimerLength = (int) turnTimerSlider.value * 10;
        //SetTurnTimerInteractable();
    }

    public void RefreshGameSettings()
    {
        //if (GeneralUtilities.GetPlayerFromClientConnection().Info.IsPartyLeader) return;

        Debug.LogError("Getting Data");

        /** Turn Timer **/
        turnTimerToggle.isOn = GameSession.IsUsingTurnTimer;
        turnTimerSlider.value = GameSession.TurnTimerLength / 10;
        //SetTurnTimerInteractable();
    }

    private void SetTurnTimerInteractable()
    {
        if (GeneralUtilities.GetPlayerFromClientConnection().Info.IsPartyLeader)
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
    /************************************************************/
    #region Private Class Functions

    private void Subscribe()
    {
        Debug.Log("Subscribing GameSettingsMenu");
        GameSession.ClientOnGameSettingsChanged += RefreshGameSettings; 
    }

    private void Unsubscribe()
    {
        Debug.Log("Unsubscribing GameSettingsMenu");
        GameSession.ClientOnGameSettingsChanged -= RefreshGameSettings;
    }

    #endregion
}
