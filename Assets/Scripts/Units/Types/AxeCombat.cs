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

public class AxeCombat : UnitCombat
{
    /************************************************************/
    #region Class Functions

    protected override void AllyCollision(Unit otherUnit)
    {
        MyUnit.Movement.CancelAction();
    }

    protected override void ActiveCollision(Unit otherUnit)
    {
        // is the enemy a wall?
        if (otherUnit.Id == 3)
        {
            MyUnit.Movement.CanMove = false; // capture wall

            // TODO: flag for galeforce
        }

        // active combat has been triggered
        else
        {
            MyUnit.Die();
            otherUnit.CombatHandler.HasCaptured = true;
        }
    }

    protected override void IdleCollision(Unit otherUnit)
    {
        // TODO: maybe retreat if the poison wall has moved?

        // I have captured enemy piece, now stop
        MyUnit.Movement.CanMove = false;

        // TODO: flag for galeforce
    }

    #endregion
}
