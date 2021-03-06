/**
 * File Name: MainMenu.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    /************************************************************/
    #region Variables

    [SerializeField] GameObject preLobbyMenu = null;
    [SerializeField] LobbyMenu lobbyMenu = null;

    #endregion
    /************************************************************/
    #region Unity Functions

    private void Start()
    {
        Subscribe();
        lobbyMenu.Subscribe(); // HACK: i really don't like this, but it works
    }

    private void OnDestroy()
    {
        Unsubscribe();
        lobbyMenu.Unsubscribe();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    public void OnlineButtonPressed()
    {
        gameObject.SetActive(false);
        preLobbyMenu.SetActive(true);
        GameSession.IsOnline = true;
    }

    public void MapEditorButtonPressed()
    {
        SceneLoader.LoadMapEditorScene();
    }

    public void GoOffline() // HACK: this function is a little off
    {
        GameSession.IsOnline = false;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    private void Subscribe()
    {
        GameNetworkManager.OnClientConnectEvent += HandleOnClientConnectEvent;
    }

    private void Unsubscribe()
    {
        GameNetworkManager.OnClientConnectEvent -= HandleOnClientConnectEvent;
    }

    private void HandleOnClientConnectEvent()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
