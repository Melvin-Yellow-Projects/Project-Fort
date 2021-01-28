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

    protected override void ActiveCollision(Unit otherUnit)
    {
        // is the enemy an axe?
        if (otherUnit.Id == 0)
        {
            MyUnit.Die(); // I am captured by the axe
        }

        // are we both traveling to the same cell? is there a gap between us?
        else if (MyUnit.Movement.EnRouteCell == otherUnit.Movement.EnRouteCell)
        {
            MyUnit.Movement.CanMove = false;
        }

        // the enemy is adjacent to me
        else
        {
            MyUnit.Movement.CancelAction();
        }
    }

    protected override void IdleCollision(Unit otherUnit)
    {
        // TODO: unless I am the poison?
        MyUnit.Movement.CancelAction();
    }

    #endregion
}
