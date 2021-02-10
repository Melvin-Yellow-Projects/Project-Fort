/**
 * File Name: BowCombat.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 2, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowCombat : UnitCombat
{
    /************************************************************/
    #region Variables

    [Header("Bow Settings")]
    [SerializeField, Range(1, 10)] int maxStepsBeforeFire = 2;
    [SerializeField] int range = 3;

    #endregion
    /************************************************************/
    #region Properties

    public bool CanFire
    {
        get
        {
            UnitMovement movement = MyUnit.Movement;
            int stepsTaken = movement.MaxMovement - movement.CurrentMovement;
            return (0 < stepsTaken && stepsTaken <= maxStepsBeforeFire && !movement.Path.HasPath);
        }
    }

    #endregion
    /************************************************************/
    #region Base Class Functions

    protected override void AllyCollision(Unit otherUnit)
    {
        MyUnit.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Unit otherUnit)
    {
        // is the enemy a wall or bow?
        if (otherUnit.Id == 3 || otherUnit.Id == 4)
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
        // is the enemy a wall or bow?
        if (otherUnit.Id == 3 || otherUnit.Id == 4)
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
        MyUnit.Movement.CancelAction();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    // HACK this function is so jank home slice
    public Unit Fire()
    {
        //Debug.LogError($"{MyUnit.name} is FIRING");

        Unit unit = null;

        HexCell targetCell = MyUnit.MyCell.GetNeighbor(MyUnit.Movement.Direction);
        for (int i = 0; !unit && targetCell && i < range; i++)
        {
            //Debug.Log($"{i}: {targetCell.Index}");

            if (targetCell.HasTheHighGround(MyUnit.MyCell)) break;

            if (targetCell.MyUnit) unit = targetCell.MyUnit;

            targetCell = targetCell.GetNeighbor(MyUnit.Movement.Direction);
        }

        MyUnit.Movement.CanMove = false;

        if (!unit) return null;

        if (unit.MyTeam == MyUnit.MyTeam) return null;

        if (unit.Id == 3) return null;

        return unit;
    }

    #endregion
}
