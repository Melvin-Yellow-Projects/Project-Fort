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

    [Header("Laser Settings")]
    [SerializeField, Range(1, 10)] int maxStepsBeforeFire = 2;
    [SerializeField] int range = 10000;

    #endregion
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        Debug.Log("hello");
        if (!CanFire(myPiece)) return;
        Debug.Log("hey");
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

        Debug.Log("hello again");
    }

    private bool CanFire(Piece myPiece)
    {
        PieceMovement movement = myPiece.Movement;
        int stepsTaken = movement.MaxMovement - movement.CurrentMovement;
        return (0 < stepsTaken && stepsTaken <= maxStepsBeforeFire && !movement.Path.HasPath);
    }

    private bool CanCapturePiece(Piece piece, PieceType[] withCaptureTypes)
    {
        // TODO: be absolutely certain this line is needed
        if (piece.IsDying) return false;

        foreach (PieceType type in withCaptureTypes)
            if (piece.Configuration.Type == type) return true;
        return false;
    }

    #endregion
}
