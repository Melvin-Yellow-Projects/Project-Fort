﻿/**
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
    /********** MARK: Unity Functions **********/
    #region Unity Functions

    private void Start()
    {
        Subscribe();
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
        GameNetworkManager.OnClientConnected += HandleOnClientConnected;
    }

    private void Unsubscribe()
    {
        GameNetworkManager.OnClientConnected -= HandleOnClientConnected;
    }

    private void HandleOnClientConnected()
    {
        gameObject.SetActive(false);
    }

    #endregion
}
