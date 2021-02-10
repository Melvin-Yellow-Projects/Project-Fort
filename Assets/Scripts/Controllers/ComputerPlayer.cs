﻿/**
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
    #region Variables

    [Header("Settings")]
    [SerializeField] bool canMakeMoves = true;

    [SerializeField, Range(0, 10)] float maxActionWaitTime = 5f;

    [SerializeField, Range(0, 1)] float chanceToSkipAction = 0.5f;

    HexCell targetCell = null;

    #endregion
    /************************************************************/
    #region Properties

    public static ComputerPlayer Prefab { get; set; }

    #endregion
    /************************************************************/
    #region Class Functions

    private IEnumerator BuyUnits()
    {
        foreach (Fort fort in MyForts)
        {
            foreach (HexCell cell in fort.GetBuyCells())
            {
                yield return new WaitForSeconds(Random.Range(0, maxActionWaitTime));

                // HACK cpu's will not buy the Bow (because they don't know how to use it)
                if (Random.Range(0f, 1f) > chanceToSkipAction)
                    ServerTryBuyUnit(Random.Range(0, 3), cell);
            }
        }
        GameManager.Singleton.ServerTryEndTurn();
    }

    private IEnumerator MoveUnits()
    {
        foreach (Unit unit in MyUnits)
        {
            yield return null;

            if (unit.Movement.CanMove && Random.Range(0f, 1f) > chanceToSkipAction)
            {
                if (!targetCell) targetCell = GetTargetCell();

                unit.Movement.Path.Cells = UnitPathfinding.FindPath(unit, unit.MyCell, targetCell);

                ServerSetAction(this, UnitData.Instantiate(unit));
            }
        }
        GameManager.Singleton.ServerTryEndTurn();
    }

    private HexCell GetTargetCell()
    {
        // target players first
        Player targetPlayer = null; 
        foreach (Player player in GameManager.Players)
        {
            if (player as HumanPlayer && player.MyTeam != MyTeam)
            {
                targetPlayer = player;
                break;
            }
        }

        // target other cpus if no player is found, might return to its own cell
        if (!targetPlayer) targetPlayer =
                GameManager.Players[Random.Range(0, GameManager.Players.Count)];
        return targetPlayer.MyForts[0].MyCell;
    }

    #endregion
    /************************************************************/
    #region Event Handler Functions

    protected override void ServerSubscribe()
    {
        GameManager.ServerOnStopTurn += HandleServerOnStopTurn;
        base.ServerSubscribe();
    }

    protected override void ServerUnsubscribe()
    {
        GameManager.ServerOnStopTurn -= HandleServerOnStopTurn;
        base.ServerUnsubscribe();
    }

    [Server]
    protected override void HandleServerOnStartRound()
    {
        base.HandleServerOnStartRound();

        HasEndedTurn = true; // player can prematurely end a cpu's turn

        Debug.Log($"{name} is buying");
        if (canMakeMoves) StartCoroutine(BuyUnits());
    }

    [Server]
    protected override void HandleServerOnStartTurn()
    {
        StopAllCoroutines();
        base.HandleServerOnStartTurn();

        HasEndedTurn = true; // player can kinda prematurely end a cpu's turn, but not really

        // HACK should the cpu listen to a lost flag from the base function call?

        Debug.Log($"{name} is moving");
        if (canMakeMoves) StartCoroutine(MoveUnits());
    }

    [Server]
    protected override void HandleServerOnPlayTurn()
    {
        StopAllCoroutines();
        base.HandleServerOnPlayTurn();
    }

    private void HandleServerOnStopTurn()
    {
        if (!targetCell) return;
        // if one of my units get to the target, break
        foreach (Unit unit in MyUnits)
        {
            if (unit.MyCell.Index == targetCell.Index)
            {
                targetCell = null;
                break;
            }
        }
    }

    #endregion
}
