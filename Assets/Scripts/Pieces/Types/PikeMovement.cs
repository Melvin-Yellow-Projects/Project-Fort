﻿/**
 * File Name: PikeMovement.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 21, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// 
/// </summary>
public class PikeMovement : PieceMovement
{
    /************************************************************/
    #region Variables
    
    #endregion
    /************************************************************/
    #region Class Functions

    public override bool IsValidEdgeForPath(HexCell current, HexCell neighbor)
    {
        return base.IsValidEdgeForPath(current, neighbor);
    }

    public override bool IsValidCellForPath(HexCell current, HexCell neighbor)
    {
        return base.IsValidCellForPath(current, neighbor);
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    [Server]
    protected override void HandleServerOnStopTurn()
    {
        if (currentMovement < MaxMovement) CanMove = false;

        base.HandleServerOnStopTurn();
    }

    [ClientRpc]
    protected override void HandleRpcOnStopTurn()
    {
        if (!isClientOnly) return;

        if (currentMovement < MaxMovement) CanMove = false;

        base.HandleRpcOnStopTurn();
    }

    #endregion
}
