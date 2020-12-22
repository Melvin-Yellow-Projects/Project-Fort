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
    /********** MARK: Variables **********/
    #region Variables

    [SerializeField] LobbyMenu lobbyMenu = null;

    #endregion

    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Start()
    {
        Subscribe();
        lobbyMenu.Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
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
