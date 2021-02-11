/**
 * File Name: GameOverMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 20, 2021
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

    [SerializeField] private TMP_Text defeatText = null;
    [SerializeField] private TMP_Text victoryText = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Awake()
    {
        defeatText.gameObject.SetActive(false);
        victoryText.gameObject.SetActive(false);
        gameObject.SetActive(false);

        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void LeaveGame()
    {
        SceneLoader.StopConnectionAndLoadStartScene();
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameOverHandler.TargetOnDefeat += HandleTargetOnDefeat;
        GameOverHandler.ClientOnGameOver += HandleClientOnGameOver;
    }

    private void Unsubscribe()
    {
        GameOverHandler.TargetOnDefeat -= HandleTargetOnDefeat;
        GameOverHandler.ClientOnGameOver -= HandleClientOnGameOver;
    }

    private void HandleTargetOnDefeat()
    {
        defeatText.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }

    private void HandleClientOnGameOver(string winnerName)
    {
        victoryText.gameObject.SetActive(true);

        victoryText.text = $"{winnerName} Has Won!";

        gameObject.SetActive(true);
    }
    #endregion
}
