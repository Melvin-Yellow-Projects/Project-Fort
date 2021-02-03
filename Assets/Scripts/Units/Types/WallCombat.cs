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

public class WallCombat : UnitCombat
{
    /************************************************************/
    #region Class Functions

    protected override void AllyCollision(Unit otherUnit)
    {
        MyUnit.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Unit otherUnit)
    {
        // is the enemy an axe?
        if (otherUnit.Id == 0)
        {
            MyUnit.Die(); // I am captured by the axe
            otherUnit.CombatHandler.HasCaptured = true;
        }

        // is the enemy a wall? 
        else if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CancelAction();
        }

        else
        {
            MyUnit.Movement.CanMove = false;
        }
    }

    protected override void ActiveBorderCollision(Unit otherUnit)
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

    protected override void IdleCollision(Unit otherUnit)
    {
        // TODO: unless I am the poison?
        MyUnit.Movement.CancelAction();
    }

    #endregion
}
