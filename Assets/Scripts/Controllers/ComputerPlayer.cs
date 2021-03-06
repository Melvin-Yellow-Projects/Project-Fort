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

    private IEnumerator BuyPieces()
    {
        foreach (Fort fort in MyForts)
        {
            foreach (HexCell cell in fort.GetBuyCells())
            {
                yield return new WaitForSeconds(Random.Range(0, maxActionWaitTime));

                if (Random.Range(0f, 1f) > chanceToSkipAction)
                    ServerTryBuyPiece(Random.Range(0, 4), cell);
            }
        }
        HasEndedTurn = true;
    }

    private IEnumerator MovePieces()
    {
        foreach (Piece piece in MyPieces)
        {
            yield return null;

            if (piece.Movement.CanMove && Random.Range(0f, 1f) > chanceToSkipAction)
            {
                if (!targetCell) targetCell = GetTargetCell();

                piece.Movement.Path.Cells = PiecePathfinding.FindPath(piece, piece.MyCell, targetCell);

                ServerSetAction(this, PieceData.Instantiate(piece));
            }
        }
        HasEndedTurn = true;
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

        Debug.Log($"{name} is buying");
        if (canMakeMoves) StartCoroutine(BuyPieces());
        else HasEndedTurn = true; 
    }

    [Server]
    protected override void HandleServerOnStartTurn()
    {
        StopAllCoroutines();
        base.HandleServerOnStartTurn();

        // HACK should the cpu listen to a lost flag from the base function call?

        Debug.Log($"{name} is moving");
        if (canMakeMoves) StartCoroutine(MovePieces());
        else HasEndedTurn = true;
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
        // if one of my pieces get to the target, break
        foreach (Piece piece in MyPieces)
        {
            if (piece.MyCell.Index == targetCell.Index)
            {
                targetCell = null;
                break;
            }
        }
    }

    #endregion
}
