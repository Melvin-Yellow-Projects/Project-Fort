/**
 * File Name: HorseCombat.cs
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

public class HorseCombat : PieceCombat
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
            MyPiece.Movement.CancelAction();
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
            MyPiece.Movement.CancelAction();
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
        // is the enemy a wall or a pike?
        if (otherUnit.Id == 3 || otherUnit.Id == 2)
        {
            MyPiece.Movement.CancelAction();
        }

        // I have captured enemy piece
        else
        {
            //MyUnit.Movement.Path.Clear();
            MyPiece.Movement.ServerClearAction();
        }
    }
    #endregion
}
