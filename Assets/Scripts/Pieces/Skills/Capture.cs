/**
 * File Name: Capture.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 23, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "Active Capture Skill",
    menuName = "Skills/Collision Skill/Capture")]
public class Capture : Skill
{
    /************************************************************/
    #region Variables

    //bool isActive;

    #endregion
    /************************************************************/
    #region Class Functions

    public override void Invoke(Piece myPiece)
    {
        Piece otherPiece = myPiece.CollisionHandler.OtherPiece;

        // does this piece die?
        if (IsPieceCaptured(myPiece, otherPiece.Configuration.CaptureTypes))
        {
            myPiece.Die();
        }

        // does this piece capture the other piece?
        else if (IsPieceCaptured(otherPiece, myPiece.Configuration.CaptureTypes))
        {
            myPiece.Movement.CanMove = false;
        }

        // otherwise, this piece is neither capturing nor captured
        else
        {
            myPiece.Movement.CancelAction();
        }
    }

    private bool IsPieceCaptured(Piece piece, PieceType[] captureTypes)
    {
        foreach (PieceType type in captureTypes) if (piece.Configuration.Type == type) return true;
        return false;
    }

    #endregion
}