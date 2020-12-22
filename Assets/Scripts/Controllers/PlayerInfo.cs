/**
 * File Name: PlayerInfo.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 22, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerInfo : NetworkBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    [SyncVar]
    bool isPartyOwner = false;

    [SyncVar]
    string playerName;

    //[SyncVar]
    //Color playerColor = new Color();

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    /// <summary>
    /// Event for when a client disconnects from the server
    /// </summary>
    public static event Action OnClientPlayerInfoUpdate;

    #endregion

    /********** MARK: Class Events **********/
    #region Class Events

    public bool IsPartyOwner
    {
        get
        {
            return isPartyOwner;
        }

        [Server]
        set
        {
            isPartyOwner = value;
            OnClientPlayerInfoUpdate?.Invoke();
        }
    }

    public string PlayerName
    {
        get
        {
            return playerName;
        }

        set
        {
            playerName = value;
            OnClientPlayerInfoUpdate?.Invoke();
        }
    }

    #endregion

    /********** MARK: Event Handler Functions **********/
    #region Event Handler Functions

    private void Subscribe()
    {
        
    }

    private void Unsubscribe()
    {
        
    }

    #endregion
}
