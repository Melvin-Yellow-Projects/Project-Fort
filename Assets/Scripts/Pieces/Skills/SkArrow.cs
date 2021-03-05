/**
 * File Name: SkArrow.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 24, 2021
 * 
 * Additional Comments: 
 * 
 *      Previously known as Laser.cs
 *      HACK: im pretty sure the WillDie flag will fail if the turn ends before the WillDie step
 *              flag clears and the piece dies
 **/

using System;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "Arrow", menuName = "Skills/Non-Collision Skills/Arrow")]
public class SkArrow : Skill
{
    /************************************************************/
    #region Variables

    /** Capture Settings **/
    [Header("Capture Settings")]
    [Tooltip("a list of piece types that this skill can Capture")]
    [SerializeField] PieceType[] captureTypes;

    [Header("Skill Settings")]
    [Tooltip("number of steps the piece can take and still fire")]
    [SerializeField, Range(1, 10)] int maxStepsBeforeFire = 2;
    [Tooltip("number of cells the effective range is")]
    [SerializeField] int range = 10000;

    #endregion
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        if (!CanFire(myPiece)) return;
        Piece piece = null;

        HexCell targetCell = myPiece.MyCell.GetNeighbor(myPiece.Movement.Direction);
        for (int i = 0; !piece && targetCell && i < range; i++)
        {
            //Debug.Log($"{i}: {targetCell.Index}");

            if (targetCell.HasTheHighGround(myPiece.MyCell)) break;

            if (targetCell.MyPiece) piece = targetCell.MyPiece;

            targetCell = targetCell.GetNeighbor(myPiece.Movement.Direction);
        }

        myPiece.Movement.CanMove = false; // HACK: this prevents the piece from continuously firing

        if (!piece) return;

        if (piece.MyTeam == myPiece.MyTeam) return;

        // HACK: when a piece can die but not have racetime errors, change this to Die();
        if (CanCapturePiece(piece, captureTypes)) piece.WillDie = true;
    }

    private bool CanFire(Piece myPiece)
    {
        PieceMovement movement = myPiece.Movement;
        int stepsTaken = movement.MaxMovement - movement.CurrentMovement;
        return (0 < stepsTaken && stepsTaken <= maxStepsBeforeFire && !movement.Path.HasPath);
    }

    private bool CanCapturePiece(Piece piece, PieceType[] withCaptureTypes)
    {
        foreach (PieceType type in withCaptureTypes)
            if (piece.Configuration.Type == type) return true;
        return false;
    }

    #endregion
}
