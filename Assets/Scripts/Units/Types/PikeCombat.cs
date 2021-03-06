﻿/**
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

public class PikeCombat : UnitCombat
{
    /************************************************************/
    #region Base Class Functions

    protected override void AllyCollision(Unit otherUnit)
    {
        MyUnit.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Unit otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CancelAction();
        }

        // if the enemy is not a horse, die
        else if (otherUnit.Id != 1)
        {
            MyUnit.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }

        else
        {
            // do nothing, cut through the enemy horse with charge
        }
    }

    protected override void ActiveBorderCollision(Unit otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CancelAction();
        }

        // if the enemy is not a horse, die
        else if (otherUnit.Id != 1)
        {
            gameObject.SetActive(false);
            MyUnit.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }

        else
        {
            // do nothing, cut through the enemy horse with charge
        }
    }

    protected override void IdleCollision(Unit otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CancelAction();
        }

        else
        {
            // do nothing, cut through the enemy with charge
        }
    }

    #endregion
}
