/**
 * File Name: ComputerPlayer.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: December 21, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ComputerPlayer : Player
{
    /************************************************************/
    #region Properties

    public static ComputerPlayer Prefab { get; set; }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    [Server]
    protected override void HandleServerOnStartRound()
    {
        base.HandleServerOnStartRound();

        //Debug.Log($"{name} is buying");

        //foreach (Fort fort in MyForts)
        //{
        //    foreach (HexCell cell in fort.GetBuyCells())
        //    {
        //        CmdTryBuyUnit(Random.Range(0, Unit.Prefabs.Count), cell);
        //    }
        //}

        HasEndedTurn = true;
    }

    [Server]
    protected override void HandleServerOnStartTurn()
    {
        base.HandleServerOnStartTurn();

        Debug.Log($"{name} is moving");

        //foreach (Unit unit in MyUnits)
        //{
        //    if (unit.Movement.CanMove)
        //    {
        //        unit.Movement.Path.Cells =
        //            UnitPathfinding.FindPath(unit, unit.MyCell, HexGrid.Forts[0].MyCell);

        //        CmdSetAction(UnitData.Instantiate(unit));
        //    }
        //}

        HasEndedTurn = true;
    }

    #endregion
}
