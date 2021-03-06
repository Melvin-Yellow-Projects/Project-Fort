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

    [Header("Turn Timer")]
    [SerializeField] Toggle turnTimerToggle = null;
    [SerializeField] Slider turnTimerSlider = null;
    [SerializeField] TMP_Text turnTimerText = null;

    [Header("Player Credits")]
    [SerializeField] TMP_Text startingCreditsText = null;
    [SerializeField] Slider startingCreditsSlider = null;
    [SerializeField] TMP_Text creditsPerFortText = null;
    [SerializeField] Slider creditsPerFortSlider = null;

    #endregion
    /************************************************************/
    #region Properties

    public static GameSettingsMenu Singleton { get; private set; }

    public bool Interactable
    {
        set
        {
            /** Turn Timer **/
            turnTimerToggle.interactable = value;
            turnTimerSlider.interactable = value && GameSession.IsUsingTurnTimer;

            /** Player Credit **/
            startingCreditsSlider.interactable = value;
            creditsPerFortSlider.interactable = value;
        }
    }

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        Singleton = this;
        Subscribe();
        if (GameSession.Singleton) RefreshGameSettings();
    }

    private void OnDestroy()
    {
        //Singleton = null;
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Public Functions

    public void SetGameSettings()
    {
        // HACK this code is a work around for toggle's OnValueChanged activating with toggle.isOn
        Player player = GeneralUtilities.GetPlayerFromClientConnection();
        if (!player || !player.Info.IsPartyLeader) return;

        /** Turn Timer **/
        GameSession.IsUsingTurnTimer = turnTimerToggle.isOn;
        GameSession.TurnTimerLength = (int) turnTimerSlider.value * 10;
        turnTimerSlider.interactable = GameSession.IsUsingTurnTimer;
        turnTimerText.text = GetTurnTimerText();

        /** Player Credit **/
        GameSession.StartingCredits = (int) startingCreditsSlider.value * 25;
        GameSession.CreditsPerFort = (int) creditsPerFortSlider.value * 25;
        startingCreditsText.text = $"{GameSession.StartingCredits}";
        creditsPerFortText.text = $"{GameSession.CreditsPerFort}";

        // if this is the server, the sync var's will transmit the data
        if (player.isServer) return;
        GameSession.Singleton.CmdSetGameSettings(GameSession.GetCopyOfGameSettings());
    }

    public void RefreshGameSettings()
    {
        /** Turn Timer **/
        turnTimerToggle.isOn = GameSession.IsUsingTurnTimer;
        turnTimerSlider.value = GameSession.TurnTimerLength / 10;
        turnTimerText.text = GetTurnTimerText();

        /** Player Credit **/
        startingCreditsSlider.value = GameSession.StartingCredits / 25;
        creditsPerFortSlider.value = GameSession.CreditsPerFort / 25;
        startingCreditsText.text = $"{GameSession.StartingCredits}";
        creditsPerFortText.text = $"{GameSession.CreditsPerFort}";

        // HACK this code is a work around for toggle's OnValueChanged activating with toggle.isOn
        Player player = GeneralUtilities.GetPlayerFromClientConnection();
        if (!player) return;
        Interactable = player.Info.IsPartyLeader;
    }
    #endregion
    /************************************************************/
    #region Private Functions

    private string GetTurnTimerText()
    {
        if (GameSession.IsUsingTurnTimer)
        {
            int min = GameSession.TurnTimerLength / 60;
            int sec = GameSession.TurnTimerLength % 60;

            return $"{min} min {sec} sec";
        }
        else
        {
            return "off";
        }
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

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
