/**
 * File Name: ComputerPlayer.cs
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
using Mirror;

public class ComputerPlayer : Player
{
    /************************************************************/
    #region Properties

    public static ComputerPlayer Prefab { get; set; }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    [Server]
    protected override void HandleServerOnStartRound()
    {
        base.HandleServerOnStartRound();
        HasEndedTurn = true;
    }

    [Server]
    protected override void HandleServerOnStartTurn()
    {
        base.HandleServerOnStartTurn();
        HasEndedTurn = true;
    }

    #endregion
}
