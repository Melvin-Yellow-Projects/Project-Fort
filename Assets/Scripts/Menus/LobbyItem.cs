/**
 * File Name: LobbyItem.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 1, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyItem : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [SerializeField] TMP_Text playerNameText = null;
    [SerializeField] RawImage playerSteamImage = null;

    #endregion

    /************************************************************/
    #region Class Functions

    public void SetName(string name)
    {
        playerNameText.text = name;
        playerNameText.GetComponent<EllipsisSetter>().enabled = false;
    }

    public void ClearName()
    {
        playerNameText.text = "Waiting For Player";
        playerNameText.GetComponent<EllipsisSetter>().enabled = true;
    }

    #endregion
}
