/**
 * File Name: WallCombat.cs
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

public class WallCombat : PieceCombat
{
    /************************************************************/
    #region Class Functions

    protected override void AllyCollision(Piece otherUnit)
    {
        MyPiece.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Piece otherUnit)
    {
        // is the enemy an axe?
        if (otherUnit.Id == 0)
        {
            MyPiece.Die(); // I am captured by the axe
            otherUnit.CombatHandler.HasCaptured = true;
        }

        // is the enemy a wall? 
        else if (otherUnit.Id == 3)
        {
            MyPiece.Movement.CancelAction();
        }

        else
        {
            MyPiece.Movement.CanMove = false;
        }
    }

    protected override void ActiveBorderCollision(Piece otherUnit)
    {
        //// is the enemy an axe?
        //if (otherUnit.Id == 0)
        //{
        //    MyUnit.Die(); // I am captured by the axe
        //    otherUnit.CombatHandler.HasCaptured = true;
        //}

        //// cancel action 
        //else
        //{
        //    MyUnit.Movement.CancelAction();
        //}
        ActiveCenterCollision(otherUnit); // TODO: allow for walls to creep
    }

    protected override void IdleCollision(Piece otherUnit)
    {
        // TODO: unless I am the poison?
        MyPiece.Movement.CancelAction();
    }

    #endregion
}
