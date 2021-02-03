/**
 * File Name: SystemMenu.cs
 * Description: Manages the settings and general user interface
 * 
 * Authors: Will Lacey
 * Date Created: December 16, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [Header("Cached References")]
    [SerializeField] Button downloadButton = null;
    [SerializeField] Button helpButton = null;
    [SerializeField] Button settingsButton = null;
    [SerializeField] Button exitButton = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        if (Mirror.NetworkServer.active)
        {
            downloadButton.interactable = true;
            helpButton.interactable = true;
            settingsButton.interactable = true;
            exitButton.interactable = true;
        }
        else
        {
            downloadButton.interactable = false;
            helpButton.interactable = true;
            settingsButton.interactable = true;
            exitButton.interactable = true;
        }
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void DownloadButtonPressed()
    {
        // TODO: this should download the current player map
        string title = "Download Map";
        string description = "sorry, this feature is not yet implemented";

        PopupMenu.Open(title, description, isConfirmationPopup: false);
    }

    public void HelpButtonPressed()
    {
        // TODO: should be a help menu
        string title = "Help";
        string description = "sorry, this feature is not yet implemented";

        PopupMenu.Open(title, description, isConfirmationPopup : false);
    }

    public void SettingsButtonPressed()
    {
        // TODO: should be a settings menu
        string title = "Settings";
        string description = "sorry, this feature is not yet implemented";

        PopupMenu.Open(title, description, isConfirmationPopup: false);
    }

    public void ExitButtonPressed()
    {
        // HACK: verify this line
        if (Mirror.NetworkServer.active) 
        {
            string title = "Exit";
            string description = "would you like to leave?";

            PopupMenu.Open(title, description,
                isConfirmationPopup: true, func: SceneLoader.LoadStartScene); 
        }
        else
        {
            Debug.LogWarning("Quitting Application");
            Application.Quit();
        }
    }

    #endregion
}
