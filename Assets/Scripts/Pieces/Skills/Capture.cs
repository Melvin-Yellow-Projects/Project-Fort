/**
 * File Name: Capture.cs
 * Description: 
 * 
 * Authors: Will Lacey
 * Date Created: February 24, 2021
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Piece attempts to take the place of the other collision piece
/// </summary>
[CreateAssetMenu(fileName = "Capture Skill", menuName = "Skills/Collision Skills/Capture")]
public class Capture : CollisionSkill
{
    /************************************************************/
    #region Class Functions

    protected override void ActiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToCapturePiece(myPiece)) return;
        if (otherPiece.TryToBlockPiece(myPiece)) return;
        myPiece.Movement.CanMove = false; // piece has not been captured nor blocked, it won
    }

    protected override void InactiveCollision(Piece myPiece, Piece otherPiece)
    {
        if (otherPiece.TryToBlockPiece(myPiece)) return;
        myPiece.Movement.CanMove = false;

        // HACK: this should be guaranteed to succeed
        myPiece.TryToCapturePiece(otherPiece); 
    }

    #endregion
}