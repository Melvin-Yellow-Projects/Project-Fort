/**
 * File Name: PlayerInfo.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 22, 2020
 * 
 * Additional Comments: 
 * 
 *      HACK: Properties don't need to be server protected since SyncVar is from server to client
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

    [SyncVar(hook = nameof(HookOnIsPartyOwner))]
    bool isPartyOwner = false;

    [SyncVar(hook = nameof(HookOnPlayerName))]
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
        }
    }

    public string PlayerName
    {
        get
        {
            return playerName;
        }

        [Server]
        set
        {
            playerName = value;
        }
    }

    #endregion

    /********** MARK: Client Functions **********/
    #region Client Functions

    public override void OnStopClient()
    {
        OnClientPlayerInfoUpdate?.Invoke();
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

    private void HookOnIsPartyOwner(bool oldValue, bool newValue)
    {
        if (!hasAuthority) return;

        OnClientPlayerInfoUpdate?.Invoke();
    }

    private void HookOnPlayerName(string oldValue, string newValue)
    {
        OnClientPlayerInfoUpdate?.Invoke();
        name = playerName;
    }

    #endregion
}
