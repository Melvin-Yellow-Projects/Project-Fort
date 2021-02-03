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
    [SerializeField, Range(1, 3)] int maxStepsBeforeFire = 2;
    [SerializeField, Range(1, 10)] int range = 3;

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
            MyUnit.Movement.CancelAction();
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
        MyUnit.Movement.CancelAction();
    }

    public void Fire()
    {
        //Debug.LogError($"{bow.name} FIRE AT WILL");

        //HexCell cell = bow.MyCell;
        //for (int j = 0; cell && j < bow.FiringRange; j++)
        //{
        //    cell = cell.GetNeighbor(bow.Direction);
        //    if (cell) Debug.Log($"{cell.name}");
        //    else Debug.Log(null);
        //}
        //bow.CanMove = false;
    }

    #endregion
}
