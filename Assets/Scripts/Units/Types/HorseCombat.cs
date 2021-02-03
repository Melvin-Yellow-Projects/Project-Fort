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

public class HorseCombat : UnitCombat
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

        // active combat has been triggered
        else
        {
            MyUnit.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }
    }

    protected override void ActiveBorderCollision(Unit otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CancelAction();
        }

        // active combat has been triggered
        else
        {
            gameObject.SetActive(false);
            MyUnit.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }
    }

    protected override void IdleCollision(Unit otherUnit)
    {
        // is the enemy a wall or a pike?
        if (otherUnit.Id == 3 || otherUnit.Id == 2)
        {
            MyUnit.Movement.CancelAction();
        }

        // I have captured enemy piece
        else
        {
            //MyUnit.Movement.Path.Clear();
            MyUnit.Movement.ServerClearAction();
        }
    }
    #endregion
}
