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

public class SystemMenu : MonoBehaviour
{
    /************************************************************/
    #region Class Functions

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
            SceneLoader.LoadStartScene();
        }
        else
        {
            Debug.LogWarning("Quitting Application");
            Application.Quit();
        }
    }

    #endregion
}
