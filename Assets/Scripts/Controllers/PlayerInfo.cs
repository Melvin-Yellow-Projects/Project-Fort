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

public class PlayerInfo : NetworkBehaviour
{
    /********** MARK: Variables **********/
    #region Variables

    //[SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
    bool isPartyOwner = false;

    //[SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    string displayName;

    [SyncVar]
    Color teamColor = new Color();

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
