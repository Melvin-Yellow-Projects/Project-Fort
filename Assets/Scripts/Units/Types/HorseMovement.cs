/**
 * File Name: HorseMovement.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: January 22, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HorseMovement : UnitMovement
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
        base.HandleServerOnStopTurn();
    }

    [ClientRpc]
    protected override void HandleRpcOnStopTurn()
    {
        if (!isClientOnly) return;

        base.HandleRpcOnStopTurn();
    }

    #endregion
}
