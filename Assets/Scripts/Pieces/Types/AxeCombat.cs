/**
 * File Name: AxeCombat.cs
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

public class AxeCombat : PieceCombat
{
    /************************************************************/
    #region Base Class Functions

    protected override void AllyCollision(Piece otherUnit)
    {
        MyPiece.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Piece otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyPiece.Movement.CanMove = false; // capture wall

            // TODO: flag for galeforce
        }

        // active combat has been triggered
        else
        {
            MyPiece.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }
    }

    protected override void ActiveBorderCollision(Piece otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyPiece.Movement.CanMove = false; // capture wall

            // TODO: flag for galeforce
        }

        // active combat has been triggered
        else
        {
            gameObject.SetActive(false);
            MyPiece.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }
    }

    protected override void IdleCollision(Piece otherUnit)
    {
        // TODO: maybe retreat if the poison wall has moved?

        // I have captured enemy piece, now stop
        MyPiece.Movement.CanMove = false;

        // TODO: flag for galeforce
    }

    #endregion
}
