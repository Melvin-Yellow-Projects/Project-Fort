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

    [SerializeField] private GameObject gameOverDisplayParent = null;
    [SerializeField] private TMP_Text winnerNameText = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Start()
    {
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
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
        winnerNameText.text = $"{winner} Has Won!";

        gameOverDisplayParent.SetActive(true);
    }
    #endregion
}
