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

public class BowCombat : PieceCombat
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
            PieceMovement movement = MyPiece.Movement;
            int stepsTaken = movement.MaxMovement - movement.CurrentMovement;
            return (0 < stepsTaken && stepsTaken <= maxStepsBeforeFire && !movement.Path.HasPath);
        }
    }

    #endregion
    /************************************************************/
    #region Base Class Functions

    protected override void AllyCollision(Piece otherUnit)
    {
        MyPiece.Movement.CancelAction();
    }

    protected override void ActiveCenterCollision(Piece otherUnit)
    {
        // is the enemy a wall or bow?
        if (otherUnit.Id == 3 || otherUnit.Id == 4)
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
        // is the enemy a wall or bow?
        if (otherUnit.Id == 3 || otherUnit.Id == 4)
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
        MyPiece.Movement.CancelAction();
    }

    #endregion
    /************************************************************/
    #region Class Functions

    // HACK this function is so jank home slice
    public Piece Fire()
    {
        //Debug.LogError($"{MyUnit.name} is FIRING");

        Piece unit = null;

        HexCell targetCell = MyPiece.MyCell.GetNeighbor(MyPiece.Movement.Direction);
        for (int i = 0; !unit && targetCell && i < range; i++)
        {
            //Debug.Log($"{i}: {targetCell.Index}");

            if (targetCell.HasTheHighGround(MyPiece.MyCell)) break;

            if (targetCell.MyPiece) unit = targetCell.MyPiece;

            targetCell = targetCell.GetNeighbor(MyPiece.Movement.Direction);
        }

        MyPiece.Movement.CanMove = false;

        if (!unit) return null;

        if (unit.MyTeam == MyPiece.MyTeam) return null;

        if (unit.Id == 3) return null;

        return unit;
    }

    #endregion
}
