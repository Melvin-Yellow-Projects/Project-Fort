/**
 * File Name: AxeMovement.cs
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

public class AxeMovement : PieceMovement
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
        if (MyPiece.CombatHandler.HasCaptured)
        {
            CanMove = true;
            // HACK: this is not a good solution
            if (connectionToClient != null) TargetOnGaleforceActivate(CanMove);
        }
        else if (currentMovement < maxMovement)
        {
            CanMove = false;
        }

        base.HandleServerOnStopTurn();
    }

    [ClientRpc]
    protected override void HandleRpcOnStopTurn()
    {
        if (!isClientOnly) return;

        if (MyPiece.CombatHandler.HasCaptured) CanMove = true;
        else if (currentMovement < maxMovement) CanMove = false;

        base.HandleRpcOnStopTurn();
    }

    [TargetRpc]
    private void TargetOnGaleforceActivate(bool canMove)
    {
        CanMove = canMove;
    }

    #endregion
}
