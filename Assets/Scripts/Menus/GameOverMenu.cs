/**
 * File Name: GameOverMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 20, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [SerializeField] private TMP_Text winnerText = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void LeaveGame()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void ClientHandleGameOver(string winner)
    {
        winnerText.text = $"{winner} Has Won!";

        gameObject.SetActive(true);
    }
    #endregion
}
