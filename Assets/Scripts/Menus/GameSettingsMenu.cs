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

    [Header("Player Credit")]
    [SerializeField] TMP_Text startingCreditText = null;
    [SerializeField] Slider startingCreditSlider = null;
    [SerializeField] TMP_Text creditPerFortText = null;
    [SerializeField] Slider creditPerFortSlider = null;

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
            startingCreditSlider.interactable = value;
            creditPerFortSlider.interactable = value;
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
        SetTurnTimerInteractable();

        /** Player Credit **/
        GameSession.StartingCredit = (int) startingCreditSlider.value * 25;
        GameSession.CreditPerFort = (int) creditPerFortSlider.value * 25;
        startingCreditText.text = $"{GameSession.StartingCredit}";
        creditPerFortText.text = $"{GameSession.CreditPerFort}";

        // if this is the server, the sync var's will transmit the data
        if (player.isServer) return;
        GameSession.Singleton.CmdSetGameSettings(GameSession.GetGameSettings());
    }

    public void RefreshGameSettings()
    {
        /** Turn Timer **/
        turnTimerToggle.isOn = GameSession.IsUsingTurnTimer;
        turnTimerSlider.value = GameSession.TurnTimerLength / 10;
        SetTurnTimerInteractable();

        /** Player Credit **/
        startingCreditSlider.value = GameSession.StartingCredit / 25;
        creditPerFortSlider.value = GameSession.CreditPerFort / 25;
        startingCreditText.text = $"{GameSession.StartingCredit}";
        creditPerFortText.text = $"{GameSession.CreditPerFort}";
    }
    #endregion
    /************************************************************/
    #region Private Functions

    private void SetTurnTimerInteractable()
    {
        // HACK this code is a work around for toggle's OnValueChanged activating with toggle.isOn
        Player player = GeneralUtilities.GetPlayerFromClientConnection();

        if (player && player.Info.IsPartyLeader)
        {
            turnTimerToggle.interactable = true;
            turnTimerSlider.interactable = GameSession.IsUsingTurnTimer;
        }
        else
        {
            turnTimerToggle.interactable = false;
            turnTimerSlider.interactable = false;
        }
        turnTimerText.text = GetTurnTimerText();
    }

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
