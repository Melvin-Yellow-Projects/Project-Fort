/**
 * File Name: PikeCombat.cs
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

public class PikeCombat : PieceCombat
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

        // if the enemy is not a horse, die
        else if (otherUnit.Id != 1)
        {
            MyPiece.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }

        else
        {
            // do nothing, cut through the enemy horse with charge
        }
    }

    protected override void ActiveBorderCollision(Piece otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyPiece.Movement.CancelAction();
        }

        // if the enemy is not a horse, die
        else if (otherUnit.Id != 1)
        {
            gameObject.SetActive(false);
            MyPiece.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }

        else
        {
            // do nothing, cut through the enemy horse with charge
        }
    }

    protected override void IdleCollision(Piece otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyPiece.Movement.CancelAction();
        }

        else
        {
            // do nothing, cut through the enemy with charge
        }
    }

    #endregion
}
